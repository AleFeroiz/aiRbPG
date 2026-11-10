using aiRbPG.Domain.Catalog;
using aiRbPG.Domain.Pricing;
using aiRbPG.Domain.Reservations;
using Microsoft.Data.SqlClient;
using System.Data;

namespace aiRbPG.Data.Reservations;

public sealed class SqlReservationRepository(SqlConnectionFactory connections) : IReservationRepository
{
    public async Task<IReadOnlyList<Reservation>> GetReservationsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT r.ReservaId, c.Nome, e.Nome, r.Inicio, r.Fim, r.QuantidadePessoas, r.Status, r.ValorTotal
            FROM dbo.Reservas r
            JOIN dbo.Clientes c ON c.ClienteId = r.ClienteId
            JOIN dbo.Espacos e ON e.EspacoId = r.EspacoId
            ORDER BY r.Inicio DESC;
            """;
        await using var connection = connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var rows = new List<Reservation>();
        while (await reader.ReadAsync(cancellationToken))
            rows.Add(new Reservation(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetDateTime(3), reader.GetDateTime(4), reader.GetInt32(5), reader.GetString(6), reader.GetDecimal(7)));
        return rows;
    }

    public async Task<IReadOnlyList<(Accessory Accessory, int Available)>> GetAvailableAccessoriesAsync(
        DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT a.AcessorioId, a.Nome, a.Descricao, a.QuantidadeEstoque, a.TarifaHora,
                   CONVERT(INT, a.QuantidadeEstoque - COALESCE(SUM(CASE WHEN r.ReservaId IS NULL THEN 0 ELSE ir.QuantidadeReservada END), 0)) AS Disponivel
            FROM dbo.Acessorios a
            LEFT JOIN dbo.ItensReserva ir ON ir.AcessorioId = a.AcessorioId
            LEFT JOIN dbo.Reservas r ON r.ReservaId = ir.ReservaId
                AND r.Status NOT IN (N'Cancelada', N'Finalizada') AND r.Inicio < @Fim AND r.Fim > @Inicio
            WHERE a.Ativo = 1
            GROUP BY a.AcessorioId, a.Nome, a.Descricao, a.QuantidadeEstoque, a.TarifaHora
            ORDER BY a.Nome;
            """;
        await using var connection = connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Inicio", SqlDbType.DateTime2).Value = start;
        command.Parameters.Add("@Fim", SqlDbType.DateTime2).Value = end;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var rows = new List<(Accessory, int)>();
        while (await reader.ReadAsync(cancellationToken))
            rows.Add((new Accessory(reader.GetInt32(0), reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2), reader.GetInt32(3), reader.GetDecimal(4)), reader.GetInt32(5)));
        return rows;
    }

    public async Task<int> CreateAsync(NewReservation reservation, CancellationToken cancellationToken = default)
    {
        if (reservation.End <= reservation.Start) throw new ArgumentException("O horário de término deve ser posterior ao início.");
        if (reservation.People <= 0) throw new ArgumentException("A reserva deve ter pelo menos uma pessoa.");
        var hours = (decimal)(reservation.End - reservation.Start).TotalMinutes / 60m;
        await using var connection = connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            await using var spaceCommand = new SqlCommand("""
                SELECT Capacidade, TarifaHora, TarifaPessoaHora FROM dbo.Espacos WITH (UPDLOCK, HOLDLOCK)
                WHERE EspacoId = @EspacoId AND Ativo = 1;
                """, connection, transaction);
            spaceCommand.Parameters.Add("@EspacoId", SqlDbType.Int).Value = reservation.SpaceId;
            await using var spaceReader = await spaceCommand.ExecuteReaderAsync(cancellationToken);
            if (!await spaceReader.ReadAsync(cancellationToken)) throw new InvalidOperationException("O espaço selecionado não está ativo.");
            var capacity = spaceReader.GetInt32(0);
            var spaceRate = spaceReader.GetDecimal(1);
            var personRate = spaceReader.GetDecimal(2);
            await spaceReader.CloseAsync();
            ReservationRules.Validate(reservation.Start, reservation.End, reservation.People, capacity);

            await using (var conflict = new SqlCommand("""
                SELECT COUNT_BIG(*) FROM dbo.Reservas WITH (UPDLOCK, HOLDLOCK)
                WHERE EspacoId = @EspacoId AND Status NOT IN (N'Cancelada', N'Finalizada')
                  AND Inicio < @Fim AND Fim > @Inicio;
                """, connection, transaction))
            {
                conflict.Parameters.Add("@EspacoId", SqlDbType.Int).Value = reservation.SpaceId;
                conflict.Parameters.Add("@Inicio", SqlDbType.DateTime2).Value = reservation.Start;
                conflict.Parameters.Add("@Fim", SqlDbType.DateTime2).Value = reservation.End;
                if ((long)await conflict.ExecuteScalarAsync(cancellationToken)! > 0)
                    throw new InvalidOperationException("Já existe uma reserva nesse espaço para o período escolhido.");
            }

            var accessoryPrices = new Dictionary<int, decimal>();
            foreach (var item in reservation.Accessories.Where(item => item.Quantity > 0))
            {
                await using var availability = new SqlCommand("""
                    SELECT a.TarifaHora, a.QuantidadeEstoque - COALESCE(SUM(CASE WHEN r.ReservaId IS NULL THEN 0 ELSE ir.QuantidadeReservada END), 0)
                    FROM dbo.Acessorios a WITH (UPDLOCK, HOLDLOCK)
                    LEFT JOIN dbo.ItensReserva ir ON ir.AcessorioId = a.AcessorioId
                    LEFT JOIN dbo.Reservas r ON r.ReservaId = ir.ReservaId AND r.Status NOT IN (N'Cancelada', N'Finalizada')
                        AND r.Inicio < @Fim AND r.Fim > @Inicio
                    WHERE a.AcessorioId = @AcessorioId AND a.Ativo = 1
                    GROUP BY a.TarifaHora, a.QuantidadeEstoque;
                    """, connection, transaction);
                availability.Parameters.Add("@AcessorioId", SqlDbType.Int).Value = item.AccessoryId;
                availability.Parameters.Add("@Inicio", SqlDbType.DateTime2).Value = reservation.Start;
                availability.Parameters.Add("@Fim", SqlDbType.DateTime2).Value = reservation.End;
                await using var reader = await availability.ExecuteReaderAsync(cancellationToken);
                if (!await reader.ReadAsync(cancellationToken)) throw new InvalidOperationException("Um acessório selecionado não está ativo.");
                var rate = reader.GetDecimal(0);
                var available = reader.GetInt32(1);
                await reader.CloseAsync();
                if (item.Quantity > available) throw new InvalidOperationException($"Estoque insuficiente para {item.Name}. Disponível: {available}.");
                accessoryPrices[item.AccessoryId] = rate;
            }

            var charges = reservation.Accessories.Where(item => item.Quantity > 0)
                .Select(item => new AccessoryCharge(item.Quantity, accessoryPrices[item.AccessoryId])).ToArray();
            var quote = PricingCalculator.Calculate(hours, reservation.People, spaceRate, personRate, charges);
            int reservationId;
            await using (var insert = new SqlCommand("""
                INSERT dbo.Reservas (ClienteId, EspacoId, CriadaPorUsuarioId, Inicio, Fim, QuantidadePessoas,
                    Status, Horas, TarifaEspacoHoraAplicada, TarifaPessoaHoraAplicada, SubtotalEspaco,
                    SubtotalPessoas, Desconto, Multas, ValorTotal, Observacao)
                OUTPUT INSERTED.ReservaId
                VALUES (@ClienteId, @EspacoId, @UsuarioId, @Inicio, @Fim, @Pessoas, N'Pendente', @Horas,
                    @TarifaEspaco, @TarifaPessoa, @SubtotalEspaco, @SubtotalPessoas, 0, 0, @Total, @Observacao);
                """, connection, transaction))
            {
                insert.Parameters.Add("@ClienteId", SqlDbType.Int).Value = reservation.ClientId;
                insert.Parameters.Add("@EspacoId", SqlDbType.Int).Value = reservation.SpaceId;
                insert.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = reservation.CreatedByUserId;
                insert.Parameters.Add("@Inicio", SqlDbType.DateTime2).Value = reservation.Start;
                insert.Parameters.Add("@Fim", SqlDbType.DateTime2).Value = reservation.End;
                insert.Parameters.Add("@Pessoas", SqlDbType.Int).Value = reservation.People;
                insert.Parameters.Add("@Horas", SqlDbType.Decimal).Value = hours;
                insert.Parameters.Add("@TarifaEspaco", SqlDbType.Decimal).Value = spaceRate;
                insert.Parameters.Add("@TarifaPessoa", SqlDbType.Decimal).Value = personRate;
                insert.Parameters.Add("@SubtotalEspaco", SqlDbType.Decimal).Value = quote.SpaceSubtotal;
                insert.Parameters.Add("@SubtotalPessoas", SqlDbType.Decimal).Value = quote.PeopleSubtotal;
                insert.Parameters.Add("@Total", SqlDbType.Decimal).Value = quote.Total;
                insert.Parameters.Add("@Observacao", SqlDbType.NVarChar, 500).Value = (object?)reservation.Notes?.Trim() ?? DBNull.Value;
                reservationId = (int)(await insert.ExecuteScalarAsync(cancellationToken))!;
            }

            foreach (var item in reservation.Accessories.Where(item => item.Quantity > 0))
            {
                var subtotal = Math.Round(hours * item.Quantity * accessoryPrices[item.AccessoryId], 2, MidpointRounding.AwayFromZero);
                await using var insertItem = new SqlCommand("""
                    INSERT dbo.ItensReserva (ReservaId, AcessorioId, QuantidadeReservada, TarifaHoraAplicada, Subtotal)
                    VALUES (@ReservaId, @AcessorioId, @Quantidade, @Tarifa, @Subtotal);
                    """, connection, transaction);
                insertItem.Parameters.Add("@ReservaId", SqlDbType.Int).Value = reservationId;
                insertItem.Parameters.Add("@AcessorioId", SqlDbType.Int).Value = item.AccessoryId;
                insertItem.Parameters.Add("@Quantidade", SqlDbType.Int).Value = item.Quantity;
                insertItem.Parameters.Add("@Tarifa", SqlDbType.Decimal).Value = accessoryPrices[item.AccessoryId];
                insertItem.Parameters.Add("@Subtotal", SqlDbType.Decimal).Value = subtotal;
                await insertItem.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return reservationId;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
