using aiRbPG.Domain;
using aiRbPG.Domain.Catalog;
using aiRbPG.Domain.Pricing;
using aiRbPG.Domain.Reservations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace aiRbPG.Data.Reservations
{
    public sealed class SqlReservationRepository : IReservationRepository
    {
        private readonly SqlConnectionFactory connections;
        public SqlReservationRepository(SqlConnectionFactory connections) { this.connections = connections; }

        public async Task<IReadOnlyList<Reservation>> GetReservationsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            const string sql = "SELECT r.ReservaId, c.Nome, e.Nome, r.Inicio, r.Fim, r.QuantidadePessoas, r.Status, r.ValorTotal, r.EspacoId FROM dbo.Reservas r JOIN dbo.Clientes c ON c.ClienteId=r.ClienteId JOIN dbo.Espacos e ON e.EspacoId=r.EspacoId ORDER BY r.Inicio DESC;";
            var rows = new List<Reservation>();
            using (var connection = connections.Create())
            {
                await connection.OpenAsync(cancellationToken);
                using (var command = new SqlCommand(sql, connection))
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    while (await reader.ReadAsync(cancellationToken))
                        rows.Add(new Reservation(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetDateTime(3), reader.GetDateTime(4), reader.GetInt32(5), reader.GetString(6), reader.GetDecimal(7), reader.GetInt32(8)));
            }
            return rows;
        }

        public async Task<IReadOnlyList<AccessoryAvailability>> GetAvailableAccessoriesAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default(CancellationToken))
        {
            const string sql = "SELECT a.AcessorioId, a.Nome, a.Descricao, a.QuantidadeEstoque, a.TarifaHora, CONVERT(INT, a.QuantidadeEstoque - COALESCE(SUM(CASE WHEN r.ReservaId IS NULL THEN 0 ELSE ir.QuantidadeReservada-CASE WHEN r.Status IN(N'EmAndamento',N'AguardandoDevolucao') THEN ir.QuantidadeDevolvida+ir.QuantidadeDanificada+ir.QuantidadePerdida ELSE 0 END END), 0)) AS Disponivel FROM dbo.Acessorios a LEFT JOIN dbo.ItensReserva ir ON ir.AcessorioId=a.AcessorioId LEFT JOIN dbo.Reservas r ON r.ReservaId=ir.ReservaId AND r.Status NOT IN (N'Cancelada',N'Finalizada') AND r.Inicio<@Fim AND (r.Fim>@Inicio OR r.Status IN(N'EmAndamento',N'AguardandoDevolucao')) WHERE a.Ativo=1 GROUP BY a.AcessorioId,a.Nome,a.Descricao,a.QuantidadeEstoque,a.TarifaHora ORDER BY a.Nome;";
            var rows = new List<AccessoryAvailability>();
            using (var connection = connections.Create())
            {
                await connection.OpenAsync(cancellationToken);
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@Inicio", SqlDbType.DateTime2).Value = start;
                    command.Parameters.Add("@Fim", SqlDbType.DateTime2).Value = end;
                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            var accessory = new Accessory(reader.GetInt32(0), reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2), reader.GetInt32(3), reader.GetDecimal(4));
                            rows.Add(new AccessoryAvailability(accessory, reader.GetInt32(5)));
                        }
                }
            }
            return rows;
        }

        public Task<OperationResult<int>> CreateAsync(NewReservation reservation, CancellationToken cancellationToken = default(CancellationToken))
        { return SaveAsync(0, reservation, cancellationToken); }

        public Task<OperationResult<int>> UpdateAsync(int id, NewReservation reservation, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (id <= 0) return Task.FromResult(OperationResult<int>.Failure("Selecione uma reserva."));
            return SaveAsync(id, reservation, cancellationToken);
        }

        private async Task<OperationResult<int>> SaveAsync(int id, NewReservation reservation, CancellationToken cancellationToken)
        {
            if (reservation == null) return OperationResult<int>.Failure("Informe os dados da reserva.");
            if (reservation.Accessories == null || reservation.Accessories.Any(x => x == null || x.Quantity < 0) || reservation.Accessories.GroupBy(x => x.AccessoryId).Any(g => g.Count() > 1))
                return OperationResult<int>.Failure("Informe acessórios sem duplicação e quantidades não negativas.");
            if (reservation.Notes != null && reservation.Notes.Length > 500) return OperationResult<int>.Failure("Observação limitada a 500 caracteres.");
            if (reservation.End <= reservation.Start) return OperationResult<int>.Failure("O horário de término deve ser posterior ao início.");
            if (reservation.People <= 0) return OperationResult<int>.Failure("A reserva deve ter pelo menos uma pessoa.");
            var hours = (decimal)(reservation.End - reservation.Start).TotalMinutes / 60m;
            if (Math.Round(hours, 2, MidpointRounding.AwayFromZero) == 0 || hours > 999999.99m)
                return OperationResult<int>.Failure("A duração está fora do intervalo aceito para reservas.");

            using (var connection = connections.Create())
            {
                await connection.OpenAsync(cancellationToken);
                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        using (var valid = new SqlCommand("SELECT COUNT(*) FROM dbo.Clientes WHERE ClienteId=@Cliente AND Ativo=1;", connection, transaction))
                        {
                            valid.Parameters.AddWithValue("@Cliente", reservation.ClientId);
                            if (Convert.ToInt32(await valid.ExecuteScalarAsync(cancellationToken)) != 1) return OperationResult<int>.Failure("Cliente inativo ou inexistente.");
                            valid.CommandText = "SELECT COUNT(*) FROM dbo.Usuarios WHERE UsuarioId=@Usuario AND Ativo=1;";
                            valid.Parameters.AddWithValue("@Usuario", reservation.CreatedByUserId);
                            if (Convert.ToInt32(await valid.ExecuteScalarAsync(cancellationToken)) != 1) return OperationResult<int>.Failure("Usuário inativo ou inexistente.");
                        }
                        if (id != 0)
                        {
                            using (var editable = new SqlCommand("SELECT COUNT(*) FROM dbo.Reservas WITH (UPDLOCK,HOLDLOCK) WHERE ReservaId=@Id AND Status IN (N'Pendente',N'Confirmada') AND NOT EXISTS(SELECT 1 FROM dbo.Pagamentos p WHERE p.ReservaId=@Id);", connection, transaction))
                            {
                                editable.Parameters.AddWithValue("@Id", id);
                                if (Convert.ToInt32(await editable.ExecuteScalarAsync(cancellationToken)) != 1) return OperationResult<int>.Failure("Só é possível alterar reserva pendente/confirmada sem pagamentos.");
                            }
                        }
                        int capacity;
                        decimal spaceRate;
                        decimal personRate;
                        using (var command = new SqlCommand("SELECT Capacidade,TarifaHora,TarifaPessoaHora FROM dbo.Espacos WITH (UPDLOCK,HOLDLOCK) WHERE EspacoId=@EspacoId AND Ativo=1;", connection, transaction))
                        {
                            command.Parameters.Add("@EspacoId", SqlDbType.Int).Value = reservation.SpaceId;
                            using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                            {
                                if (!await reader.ReadAsync(cancellationToken)) return OperationResult<int>.Failure("O espaço selecionado não está ativo.");
                                capacity = reader.GetInt32(0); spaceRate = reader.GetDecimal(1); personRate = reader.GetDecimal(2);
                            }
                        }
                        var validation = ReservationRules.Validate(reservation.Start, reservation.End, reservation.People, capacity);
                        if (validation != null) return OperationResult<int>.Failure(validation);

                        using (var conflict = new SqlCommand("SELECT COUNT_BIG(*) FROM dbo.Reservas WITH (UPDLOCK,HOLDLOCK) WHERE ReservaId<>@Id AND EspacoId=@EspacoId AND Status NOT IN (N'Cancelada',N'Finalizada') AND Inicio<@Fim AND (Fim>@Inicio OR Status IN(N'EmAndamento',N'AguardandoDevolucao'));", connection, transaction))
                        {
                            conflict.Parameters.AddWithValue("@Id", id);
                            conflict.Parameters.Add("@EspacoId", SqlDbType.Int).Value = reservation.SpaceId;
                            conflict.Parameters.Add("@Inicio", SqlDbType.DateTime2).Value = reservation.Start;
                            conflict.Parameters.Add("@Fim", SqlDbType.DateTime2).Value = reservation.End;
                            if (Convert.ToInt64(await conflict.ExecuteScalarAsync(cancellationToken)) > 0)
                                return OperationResult<int>.Failure("Já existe uma reserva nesse espaço para o período escolhido.");
                        }

                        var accessoryPrices = new Dictionary<int, decimal>();
                        foreach (var item in reservation.Accessories.Where(x => x.Quantity > 0).OrderBy(x => x.AccessoryId))
                        {
                            using (var availability = new SqlCommand("SELECT a.TarifaHora,a.QuantidadeEstoque-COALESCE(SUM(CASE WHEN r.ReservaId IS NULL THEN 0 ELSE ir.QuantidadeReservada-CASE WHEN r.Status IN(N'EmAndamento',N'AguardandoDevolucao') THEN ir.QuantidadeDevolvida+ir.QuantidadeDanificada+ir.QuantidadePerdida ELSE 0 END END),0) FROM dbo.Acessorios a WITH (UPDLOCK,HOLDLOCK) LEFT JOIN dbo.ItensReserva ir ON ir.AcessorioId=a.AcessorioId LEFT JOIN dbo.Reservas r ON r.ReservaId=ir.ReservaId AND r.ReservaId<>@Id AND r.Status NOT IN (N'Cancelada',N'Finalizada') AND r.Inicio<@Fim AND (r.Fim>@Inicio OR r.Status IN(N'EmAndamento',N'AguardandoDevolucao')) WHERE a.AcessorioId=@AcessorioId AND a.Ativo=1 GROUP BY a.TarifaHora,a.QuantidadeEstoque;", connection, transaction))
                            {
                                availability.Parameters.AddWithValue("@Id", id);
                                availability.Parameters.Add("@AcessorioId", SqlDbType.Int).Value = item.AccessoryId;
                                availability.Parameters.Add("@Inicio", SqlDbType.DateTime2).Value = reservation.Start;
                                availability.Parameters.Add("@Fim", SqlDbType.DateTime2).Value = reservation.End;
                                using (var reader = await availability.ExecuteReaderAsync(cancellationToken))
                                {
                                    if (!await reader.ReadAsync(cancellationToken)) return OperationResult<int>.Failure("Um acessório selecionado não está ativo.");
                                    var rate = reader.GetDecimal(0);
                                    var available = reader.GetInt32(1);
                                    if (item.Quantity > available) return OperationResult<int>.Failure("Estoque insuficiente para " + item.Name + ". Disponível: " + available + ".");
                                    accessoryPrices[item.AccessoryId] = rate;
                                }
                            }
                        }

                        var charges = reservation.Accessories.Where(x => x.Quantity > 0).Select(x => new AccessoryCharge(x.Quantity, accessoryPrices[x.AccessoryId])).ToArray();
                        var pricing = PricingCalculator.Calculate(hours, reservation.People, spaceRate, personRate, charges);
                        if (!pricing.Succeeded) return OperationResult<int>.Failure(pricing.Error);
                        var quote = pricing.Value;
                        if (quote.Total > 99999999.99m)
                            return OperationResult<int>.Failure("O valor total da reserva excede R$ 99.999.999,99.");
                        int reservationId;
                        const string insertSql = "INSERT dbo.Reservas (ClienteId,EspacoId,CriadaPorUsuarioId,Inicio,Fim,QuantidadePessoas,Status,Horas,TarifaEspacoHoraAplicada,TarifaPessoaHoraAplicada,SubtotalEspaco,SubtotalPessoas,Desconto,Multas,ValorTotal,Observacao) OUTPUT INSERTED.ReservaId VALUES (@ClienteId,@EspacoId,@UsuarioId,@Inicio,@Fim,@Pessoas,N'Pendente',@Horas,@TarifaEspaco,@TarifaPessoa,@SubtotalEspaco,@SubtotalPessoas,0,0,@Total,@Observacao);";
                        const string updateSql = "UPDATE dbo.Reservas SET ClienteId=@ClienteId,EspacoId=@EspacoId,Inicio=@Inicio,Fim=@Fim,QuantidadePessoas=@Pessoas,Horas=@Horas,TarifaEspacoHoraAplicada=@TarifaEspaco,TarifaPessoaHoraAplicada=@TarifaPessoa,SubtotalEspaco=@SubtotalEspaco,SubtotalPessoas=@SubtotalPessoas,Desconto=0,Multas=0,ValorTotal=@Total,Observacao=@Observacao OUTPUT INSERTED.ReservaId WHERE ReservaId=@Id; DELETE dbo.ItensReserva WHERE ReservaId=@Id;";
                        using (var insert = new SqlCommand(id == 0 ? insertSql : updateSql, connection, transaction))
                        {
                            insert.Parameters.AddWithValue("@Id", id);
                            insert.Parameters.Add("@ClienteId", SqlDbType.Int).Value = reservation.ClientId;
                            insert.Parameters.Add("@EspacoId", SqlDbType.Int).Value = reservation.SpaceId;
                            insert.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = reservation.CreatedByUserId;
                            insert.Parameters.Add("@Inicio", SqlDbType.DateTime2).Value = reservation.Start;
                            insert.Parameters.Add("@Fim", SqlDbType.DateTime2).Value = reservation.End;
                            insert.Parameters.Add("@Pessoas", SqlDbType.Int).Value = reservation.People;
                            AddDecimal(insert, "@Horas", hours, 8, 2);
                            AddDecimal(insert, "@TarifaEspaco", spaceRate, 10, 2);
                            AddDecimal(insert, "@TarifaPessoa", personRate, 10, 2);
                            AddDecimal(insert, "@SubtotalEspaco", quote.SpaceSubtotal, 10, 2);
                            AddDecimal(insert, "@SubtotalPessoas", quote.PeopleSubtotal, 10, 2);
                            AddDecimal(insert, "@Total", quote.Total, 10, 2);
                            insert.Parameters.Add("@Observacao", SqlDbType.NVarChar, 500).Value = (object)OptionalTrim(reservation.Notes) ?? DBNull.Value;
                            reservationId = Convert.ToInt32(await insert.ExecuteScalarAsync(cancellationToken));
                        }

                        foreach (var item in reservation.Accessories.Where(x => x.Quantity > 0))
                        {
                            var subtotal = Math.Round(hours * item.Quantity * accessoryPrices[item.AccessoryId], 2, MidpointRounding.AwayFromZero);
                            using (var insertItem = new SqlCommand("INSERT dbo.ItensReserva (ReservaId,AcessorioId,QuantidadeReservada,TarifaHoraAplicada,Subtotal) VALUES (@ReservaId,@AcessorioId,@Quantidade,@Tarifa,@Subtotal);", connection, transaction))
                            {
                                insertItem.Parameters.Add("@ReservaId", SqlDbType.Int).Value = reservationId;
                                insertItem.Parameters.Add("@AcessorioId", SqlDbType.Int).Value = item.AccessoryId;
                                insertItem.Parameters.Add("@Quantidade", SqlDbType.Int).Value = item.Quantity;
                                AddDecimal(insertItem, "@Tarifa", accessoryPrices[item.AccessoryId], 10, 2);
                                AddDecimal(insertItem, "@Subtotal", subtotal, 10, 2);
                                await insertItem.ExecuteNonQueryAsync(cancellationToken);
                            }
                        }
                        using (var audit = new SqlCommand("INSERT dbo.HistoricoReservas(ReservaId,UsuarioId,Acao,Observacao) VALUES(@Id,@User,@Acao,@Nota);",connection,transaction))
                        {
                            audit.Parameters.AddWithValue("@Id",reservationId); audit.Parameters.AddWithValue("@User",reservation.CreatedByUserId);
                            audit.Parameters.AddWithValue("@Acao",id == 0 ? "Criação" : "Alteração");
                            audit.Parameters.AddWithValue("@Nota",id == 0 ? "Reserva criada." : "Período e itens revalidados; tarifas atuais aplicadas e desconto zerado.");
                            await audit.ExecuteNonQueryAsync(cancellationToken);
                        }
                        transaction.Commit();
                        return reservationId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static string OptionalTrim(string value) { return string.IsNullOrWhiteSpace(value) ? null : value.Trim(); }
        private static void AddDecimal(SqlCommand command, string name, decimal value, byte precision, byte scale)
        {
            var parameter = command.Parameters.Add(name, SqlDbType.Decimal);
            parameter.Precision = precision; parameter.Scale = scale; parameter.Value = value;
        }
    }
}
