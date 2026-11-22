using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using aiRbPG.Domain.Users;

namespace aiRbPG.Data.Reservations
{
    public sealed class ReturnLine
    {
        public int ItemId { get; set; }
        public int Good { get; set; }
        public int Damaged { get; set; }
        public int Lost { get; set; }
    }

    // Each operation validates the active database identity and locks the reservation.
    public sealed class SqlOperationsRepository
    {
        private readonly SqlConnectionFactory factory;
        private readonly int userId;
        public SqlOperationsRepository(SqlConnectionFactory factory, UserAccount user)
        { this.factory = factory; if (user == null) throw new ArgumentNullException("user"); userId = user.Id; }

        private Task<T> Run<T>(bool admin, Func<SqlConnection, SqlTransaction, T> operation)
        {
            return Task.Run(() =>
            {
                using (var connection = factory.Create())
                {
                    connection.Open();
                    using (var tx = connection.BeginTransaction(IsolationLevel.Serializable))
                    {
                        using (var cmd = Command(connection, tx, "SELECT Perfil FROM dbo.Usuarios WITH(HOLDLOCK) WHERE UsuarioId=@User AND Ativo=1;"))
                        {
                            cmd.Parameters.AddWithValue("@User", userId);
                            var role = Convert.ToString(cmd.ExecuteScalar());
                            if (role != "Administrador" && role != "Atendente") throw new UnauthorizedAccessException("Usuário inativo ou sem acesso.");
                            if (admin && role != "Administrador") throw new UnauthorizedAccessException("Operação exclusiva do Administrador.");
                        }
                        var result = operation(connection, tx);
                        tx.Commit();
                        return result;
                    }
                }
            });
        }

        private static SqlCommand Command(SqlConnection c, SqlTransaction tx, string sql, int id = 0)
        {
            var cmd = new SqlCommand(sql, c, tx);
            cmd.Parameters.AddWithValue("@Id", id);
            return cmd;
        }
        private static DataTable Table(SqlCommand cmd)
        { using (cmd) using (var reader = cmd.ExecuteReader()) { var table = new DataTable(); table.Load(reader); return table; } }
        private static string State(SqlConnection c, SqlTransaction tx, int id)
        {
            using (var cmd = Command(c, tx, "SELECT Status FROM dbo.Reservas WITH(UPDLOCK,HOLDLOCK) WHERE ReservaId=@Id;", id))
            { var state = Convert.ToString(cmd.ExecuteScalar()); if (state.Length == 0) throw new InvalidOperationException("Reserva não encontrada."); return state; }
        }
        private static decimal Paid(SqlConnection c, SqlTransaction tx, int id)
        { using (var cmd = Command(c, tx, "SELECT COALESCE(SUM(Valor),0) FROM dbo.Pagamentos WHERE ReservaId=@Id;", id)) return Convert.ToDecimal(cmd.ExecuteScalar()); }
        private static decimal Total(SqlConnection c, SqlTransaction tx, int id)
        { using (var cmd = Command(c, tx, "SELECT ValorTotal FROM dbo.Reservas WHERE ReservaId=@Id;", id)) return Convert.ToDecimal(cmd.ExecuteScalar()); }
        private static void Money(decimal amount)
        { if (amount < 0 || amount > 99999999.99m || Math.Round(amount, 2) != amount) throw new ArgumentException("Informe valor não negativo com até duas casas decimais."); }
        private void Log(SqlConnection c, SqlTransaction tx, int id, string action, string note)
        {
            if (note != null && note.Length > 500) throw new ArgumentException("Justificativa limitada a 500 caracteres.");
            using (var cmd = Command(c, tx, "INSERT dbo.HistoricoReservas(ReservaId,UsuarioId,Acao,Observacao) VALUES(@Id,@User,@Acao,@Nota);", id))
            { cmd.Parameters.AddWithValue("@User", userId); cmd.Parameters.AddWithValue("@Acao", action); cmd.Parameters.AddWithValue("@Nota", (object)note ?? DBNull.Value); cmd.ExecuteNonQuery(); }
        }

        public Task<DataTable> DetailsAsync(int id)
        { return Run(false, (c, tx) => Table(Command(c, tx, "SELECT r.ReservaId,r.ClienteId,r.EspacoId,r.Inicio,r.Fim,r.QuantidadePessoas,r.Status,r.ValorTotal,COALESCE((SELECT SUM(p.Valor) FROM dbo.Pagamentos p WHERE p.ReservaId=r.ReservaId),0) AS Pago,r.Observacao FROM dbo.Reservas r WHERE ReservaId=@Id;", id))); }
        public Task<DataTable> ItemsAsync(int id)
        { return Run(false, (c, tx) => Table(Command(c, tx, "SELECT i.ItemReservaId,i.AcessorioId,a.Nome,i.QuantidadeReservada AS Reservados,i.QuantidadeRetirada AS Retirados,i.QuantidadeDevolvida AS Devolvidos,i.QuantidadeDanificada AS Danificados,i.QuantidadePerdida AS Perdidos,i.TarifaHoraAplicada FROM dbo.ItensReserva i JOIN dbo.Acessorios a ON a.AcessorioId=i.AcessorioId WHERE ReservaId=@Id;", id))); }
        public Task<DataTable> PaymentsAsync(int id)
        { return Run(false, (c, tx) => Table(Command(c, tx, "SELECT PagamentoId,DataHora,Valor,Metodo,Observacao FROM dbo.Pagamentos WHERE ReservaId=@Id ORDER BY PagamentoId;", id))); }
        public Task<DataTable> HistoryAsync(int id)
        { return Run(false, (c, tx) => Table(Command(c, tx, "SELECT h.DataHora,u.Nome AS Operador,h.Acao,h.Observacao FROM dbo.HistoricoReservas h JOIN dbo.Usuarios u ON u.UsuarioId=h.UsuarioId WHERE ReservaId=@Id ORDER BY HistoricoId;", id))); }

        public Task<bool> ConfirmAsync(int id)
        {
            return Run(false, (c, tx) =>
            {
                if (State(c, tx, id) != "Pendente") throw new InvalidOperationException("Somente reservas pendentes podem ser confirmadas.");
                using (var cmd = Command(c, tx, "UPDATE dbo.Reservas SET Status=N'Confirmada' WHERE ReservaId=@Id;", id)) cmd.ExecuteNonQuery();
                Log(c, tx, id, "Confirmação", null); return true;
            });
        }
        public Task<bool> CancelAsync(int id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Informe o motivo do cancelamento.");
            return Run(false, (c, tx) =>
            {
                var state = State(c, tx, id);
                if (state != "Pendente" && state != "Confirmada") throw new InvalidOperationException("Reserva já iniciada ou encerrada não pode ser cancelada.");
                if (Paid(c, tx, id) != 0) throw new InvalidOperationException("Reserva com pagamento exige estorno externo; cancelamento não disponível nesta entrega.");
                using (var cmd = Command(c, tx, "UPDATE dbo.Reservas SET Status=N'Cancelada' WHERE ReservaId=@Id;", id)) cmd.ExecuteNonQuery();
                Log(c, tx, id, "Cancelamento", reason); return true;
            });
        }
        public Task<bool> PayAsync(int id, decimal amount, string method, string note)
        {
            Money(amount);
            if (amount == 0 || !new[] { "Dinheiro", "Pix", "Cartão", "Transferência" }.Contains(method)) throw new ArgumentException("Informe pagamento positivo e método válido.");
            return Run(false, (c, tx) =>
            {
                var state = State(c, tx, id);
                if (state == "Cancelada" || state == "Finalizada") throw new InvalidOperationException("Reserva encerrada não recebe pagamento.");
                if (amount > Total(c, tx, id) - Paid(c, tx, id)) throw new InvalidOperationException("Pagamento maior que o saldo em aberto.");
                using (var cmd = Command(c, tx, "INSERT dbo.Pagamentos(ReservaId,RegistradoPorUsuarioId,Valor,Metodo,Observacao,DataHora) VALUES(@Id,@User,@Valor,@Metodo,@Nota,SYSDATETIME());", id))
                {
                    cmd.Parameters.AddWithValue("@User", userId); cmd.Parameters.AddWithValue("@Valor", amount);
                    cmd.Parameters.AddWithValue("@Metodo", method); cmd.Parameters.AddWithValue("@Nota", (object)note ?? DBNull.Value); cmd.ExecuteNonQuery();
                }
                Log(c, tx, id, "Pagamento", amount.ToString("F2") + " - " + method); return true;
            });
        }
        public Task<bool> DiscountAsync(int id, decimal discount, string reason)
        {
            Money(discount);
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Informe justificativa do desconto.");
            return Run(true, (c, tx) =>
            {
                var state = State(c, tx, id);
                if (state != "Pendente" && state != "Confirmada") throw new InvalidOperationException("Desconto apenas antes da retirada.");
                using (var cmd = Command(c, tx, "UPDATE dbo.Reservas SET ValorTotal=ValorTotal+Desconto-@Valor,Desconto=@Valor WHERE ReservaId=@Id AND ValorTotal+Desconto-@Valor>=@Pago;", id))
                {
                    cmd.Parameters.AddWithValue("@Valor", discount); cmd.Parameters.AddWithValue("@Pago", Paid(c, tx, id));
                    if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("Desconto excede o total ou deixa valor inferior ao já pago.");
                }
                Log(c, tx, id, "Desconto", discount.ToString("F2") + " - " + reason); return true;
            });
        }
        public Task<bool> PickupAsync(int id)
        {
            return Run(false, (c, tx) =>
            {
                if (State(c, tx, id) != "Confirmada") throw new InvalidOperationException("Confirme a reserva antes da retirada.");
                using (var cmd = Command(c, tx, "UPDATE dbo.ItensReserva SET QuantidadeRetirada=QuantidadeReservada WHERE ReservaId=@Id; UPDATE dbo.Reservas SET Status=N'EmAndamento' WHERE ReservaId=@Id;", id)) cmd.ExecuteNonQuery();
                Log(c, tx, id, "Retirada integral", null); return true;
            });
        }
        public Task<bool> ReturnAsync(int id, IList<ReturnLine> lines, decimal fine, string reason)
        {
            Money(fine);
            if (lines == null || lines.GroupBy(x => x.ItemId).Any(g => g.Count() != 1)) throw new ArgumentException("Lista de devolução inválida.");
            if (fine > 0 && string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Justifique a multa.");
            return Run(false, (c, tx) =>
            {
                var state = State(c, tx, id);
                if (state != "EmAndamento" && state != "AguardandoDevolucao") throw new InvalidOperationException("A reserva não tem retirada em aberto.");
                var changed = 0;
                foreach (var line in lines.OrderBy(x => x.ItemId))
                {
                    if (line.Good < 0 || line.Damaged < 0 || line.Lost < 0) throw new ArgumentException("Quantidades não podem ser negativas.");
                    var sum = (long)line.Good + line.Damaged + line.Lost;
                    if (sum == 0) continue;
                    using (var cmd = Command(c, tx, "UPDATE dbo.ItensReserva SET QuantidadeDevolvida=QuantidadeDevolvida+@Boas,QuantidadeDanificada=QuantidadeDanificada+@Danos,QuantidadePerdida=QuantidadePerdida+@Perdas WHERE ReservaId=@Id AND ItemReservaId=@Item AND QuantidadeRetirada-QuantidadeDevolvida-QuantidadeDanificada-QuantidadePerdida>=@Qtd;", id))
                    {
                        cmd.Parameters.AddWithValue("@Item", line.ItemId); cmd.Parameters.AddWithValue("@Boas", line.Good); cmd.Parameters.AddWithValue("@Danos", line.Damaged); cmd.Parameters.AddWithValue("@Perdas", line.Lost); cmd.Parameters.AddWithValue("@Qtd", sum);
                        if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("Devolução maior que a quantidade ainda em aberto.");
                    }
                    using (var cmd = Command(c, tx, "UPDATE dbo.Acessorios SET QuantidadeEstoque=QuantidadeEstoque-@Qtd WHERE AcessorioId=(SELECT AcessorioId FROM dbo.ItensReserva WHERE ItemReservaId=@Item) AND QuantidadeEstoque>=@Qtd;", id))
                    {
                        cmd.Parameters.AddWithValue("@Qtd", line.Damaged + line.Lost); cmd.Parameters.AddWithValue("@Item", line.ItemId);
                        if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("Estoque inconsistente para registrar perdas.");
                    }
                    changed++;
                }
                if (changed == 0 && fine == 0) throw new ArgumentException("Informe ao menos uma devolução ou multa.");
                using (var cmd = Command(c, tx, "UPDATE dbo.Reservas SET Multas=Multas+@Multa,ValorTotal=ValorTotal+@Multa,Status=N'AguardandoDevolucao' WHERE ReservaId=@Id;", id))
                { cmd.Parameters.AddWithValue("@Multa", fine); cmd.ExecuteNonQuery(); }
                Log(c, tx, id, "Devolução", "Multa " + fine.ToString("F2") + ". " + reason); return true;
            });
        }
        public Task<bool> FinishAsync(int id)
        {
            return Run(false, (c, tx) =>
            {
                var state = State(c, tx, id);
                if (state != "EmAndamento" && state != "AguardandoDevolucao") throw new InvalidOperationException("Reserva deve estar em andamento.");
                using (var cmd = Command(c, tx, "SELECT COUNT(*) FROM dbo.ItensReserva WHERE ReservaId=@Id AND QuantidadeRetirada<>QuantidadeDevolvida+QuantidadeDanificada+QuantidadePerdida;", id))
                    if (Convert.ToInt32(cmd.ExecuteScalar()) != 0) throw new InvalidOperationException("Ainda existem itens sem devolução.");
                if (Paid(c, tx, id) != Total(c, tx, id)) throw new InvalidOperationException("Quite o saldo antes de finalizar.");
                using (var cmd = Command(c, tx, "UPDATE dbo.Reservas SET Status=N'Finalizada' WHERE ReservaId=@Id;", id)) cmd.ExecuteNonQuery();
                Log(c, tx, id, "Finalização", null); return true;
            });
        }
        public Task<DataTable> RevenueAsync(DateTime start, DateTime end)
        {
            if (end <= start) throw new ArgumentException("Período inválido.");
            return Run(true, (c, tx) =>
            {
                using (var cmd = Command(c, tx, "SELECT CONVERT(date,DataHora) AS Dia,Metodo,COUNT(*) AS Pagamentos,SUM(Valor) AS Recebido FROM dbo.Pagamentos WHERE DataHora>=@Inicio AND DataHora<@Fim GROUP BY CONVERT(date,DataHora),Metodo ORDER BY Dia;"))
                { cmd.Parameters.AddWithValue("@Inicio", start); cmd.Parameters.AddWithValue("@Fim", end); return Table(cmd); }
            });
        }
        public Task<DataTable> MostRentedAsync(DateTime start, DateTime end)
        {
            if (end <= start) throw new ArgumentException("Período inválido.");
            return Run(true, (c, tx) =>
            {
                using (var cmd = Command(c, tx, "SELECT a.Nome,SUM(i.QuantidadeReservada) AS Unidades,COUNT(*) AS Reservas FROM dbo.ItensReserva i JOIN dbo.Acessorios a ON a.AcessorioId=i.AcessorioId JOIN dbo.Reservas r ON r.ReservaId=i.ReservaId WHERE r.Status<>N'Cancelada' AND r.Inicio>=@Inicio AND r.Inicio<@Fim GROUP BY a.AcessorioId,a.Nome ORDER BY Unidades DESC,a.Nome;"))
                { cmd.Parameters.AddWithValue("@Inicio", start); cmd.Parameters.AddWithValue("@Fim", end); return Table(cmd); }
            });
        }
        public Task<DataTable> UsersAsync()
        { return Run(true, (c, tx) => Table(Command(c, tx, "SELECT UsuarioId,Nome,Login,Perfil,Ativo FROM dbo.Usuarios ORDER BY Nome;"))); }
        public Task<bool> AddUserAsync(string name, string login, string password, bool administrator)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 120 || string.IsNullOrWhiteSpace(login) || login.Trim().Length > 60 || password == null || password.Length < 10) throw new ArgumentException("Informe nome, login e senha de pelo menos 10 caracteres.");
            return Run(true, (c, tx) =>
            {
                byte[] salt, hash; PasswordHashing.Create(password, out salt, out hash);
                using (var cmd = Command(c, tx, "INSERT dbo.Usuarios(Nome,Login,Perfil,SenhaSalt,SenhaHash,IteracoesHash) VALUES(@Nome,@Login,@Perfil,@Salt,@Hash,@Iteracoes);"))
                {
                    cmd.Parameters.AddWithValue("@Nome", name.Trim()); cmd.Parameters.AddWithValue("@Login", login.Trim()); cmd.Parameters.AddWithValue("@Perfil", administrator ? "Administrador" : "Atendente");
                    cmd.Parameters.AddWithValue("@Salt", salt); cmd.Parameters.AddWithValue("@Hash", hash); cmd.Parameters.AddWithValue("@Iteracoes", PasswordHashing.Iterations); cmd.ExecuteNonQuery(); return true;
                }
            });
        }
        public Task<bool> SetUserActiveAsync(int id, bool active)
        {
            return Run(true, (c, tx) =>
            {
                if (id == userId) throw new InvalidOperationException("Não é permitido desativar sua própria conta.");
                using (var cmd = Command(c, tx, "UPDATE dbo.Usuarios SET Ativo=@Ativo WHERE UsuarioId=@Id;", id))
                { cmd.Parameters.AddWithValue("@Ativo", active); if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("Usuário não encontrado."); return true; }
            });
        }
    }
}
