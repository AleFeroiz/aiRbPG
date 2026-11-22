using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using aiRbPG.Data;
using aiRbPG.Data.Catalog;
using aiRbPG.Data.Reservations;
using aiRbPG.Data.Users;
using aiRbPG.Domain.Catalog;
using aiRbPG.Domain.Reservations;
using aiRbPG.Domain.Users;

namespace aiRbPG.Tests
{
    internal static class SmokeTests
    {
        private static int checks;
        private static void Assert(bool condition, string name)
        { if (!condition) throw new Exception("FALHOU: " + name); checks++; Console.WriteLine("OK " + name); }
        private static async Task Reject(Func<Task> action, string name)
        { try { await action(); } catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is UnauthorizedAccessException || ex is SqlException) { Assert(true,name); return; } throw new Exception("FALHOU: deveria rejeitar " + name); }
        private static int Main()
        {
            try { Run().GetAwaiter().GetResult(); Console.WriteLine("APROVADO: " + checks + " verificações"); return 0; }
            catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
        }
        private static async Task Run()
        {
            var factory = new SqlConnectionFactory();
            using (var c = factory.Create()) { c.Open(); if (!c.Database.StartsWith("aiRbPG_Test_",StringComparison.Ordinal)) throw new InvalidOperationException("Testes exigem banco temporário aiRbPG_Test_."); }
            var auth = new AuthenticationService(new SqlUserRepository(factory));
            Assert(await auth.NeedsInitialAdministratorAsync(),"Primeiro acesso vazio");
            await Reject(() => auth.CreateInitialAdministratorAsync("Teste","admin","curta"),"Senha curta");
            var password = Guid.NewGuid().ToString("N");
            await auth.CreateInitialAdministratorAsync("Administrador Teste","admin",password);
            var result = await auth.AuthenticateAsync("admin",password);
            Assert(result.Succeeded,"Login válido");
            Assert(!(await auth.AuthenticateAsync("admin","incorreta")).Succeeded,"Senha incorreta");
            await Reject(() => auth.CreateInitialAdministratorAsync("Outro","outro",password),"Segundo administrador inicial");
            var user = result.User;
            var ops = new SqlOperationsRepository(factory,user);
            await ops.AddUserAsync("Atendente Teste","atendente",password,false);
            var attendant = (await auth.AuthenticateAsync("atendente",password)).User;
            var attendantOps = new SqlOperationsRepository(factory,attendant);
            var catalog = new SqlCatalogRepository(factory,user);
            var attendantCatalog = new SqlCatalogRepository(factory,attendant);
            Assert((await ops.UsersAsync()).Rows.Count == 2,"Cadastro de atendente");
            await Reject(() => attendantOps.UsersAsync(),"Atendente sem gestão de usuários");
            await Reject(() => attendantOps.RevenueAsync(DateTime.Today,DateTime.Today.AddDays(1)),"Atendente sem faturamento");
            await Reject(() => attendantCatalog.SaveSpaceAsync(new RpgSpace(0,"Negado",null,null,4,10,0)),"Atendente sem alteração de tarifas");
            await Reject(() => ops.SetUserActiveAsync(user.Id,false),"Não inativar conta da sessão");
            await attendantCatalog.SaveClientAsync(new Client(0,"Cliente Teste","123","teste@exemplo.local"));
            var client = (await catalog.GetClientsAsync()).Single(x => x.Name == "Cliente Teste");
            await catalog.SaveClientAsync(new Client(client.Id,"Cliente Alterado","456","alterado@exemplo.local"));
            Assert((await catalog.GetClientsAsync()).Any(x => x.Name == "Cliente Alterado"),"Cliente cadastrado e alterado pelo fluxo real");
            await Reject(() => catalog.SaveClientAsync(new Client(0,"",null,null)),"Nome obrigatório");
            await catalog.SaveSpaceAsync(new RpgSpace(0,"Sala Teste",null,"Local de teste",6,80,0));
            await catalog.SaveSpaceAsync(new RpgSpace(0,"Sala Alternativa",null,null,6,80,0));
            var space = (await catalog.GetSpacesAsync()).Single(x => x.Name == "Sala Teste");
            var other = (await catalog.GetSpacesAsync()).Single(x => x.Name == "Sala Alternativa");
            await catalog.SaveRpgSystemAsync(new RpgSystem(0,"Sistema Teste","Teste"));
            var system = (await catalog.GetRpgSystemsAsync()).Single(x => x.Name == "Sistema Teste");
            await catalog.SaveRpgSystemAsync(new RpgSystem(system.Id,"Sistema Alterado","Alterado"));
            await catalog.DeactivateRpgSystemAsync(system.Id);
            Assert(!(await catalog.GetRpgSystemsAsync()).Any(x => x.Id == system.Id),"Sistema alterado e inativado");
            await catalog.SaveAccessoryAsync(new Accessory(0,"Dados Teste",null,3,2));
            var accessory = (await catalog.GetAccessoriesAsync()).Single(x => x.Name == "Dados Teste");
            var repo = new SqlReservationRepository(factory);
            var start = new DateTime(2026,12,1,14,0,0);
            var items = new[] { new ReservationAccessory(accessory.Id,accessory.Name,2,2) };
            var draft = new NewReservation(client.Id,space.Id,user.Id,start,start.AddHours(2),4,"Teste",items);
            int id = await repo.CreateAsync(draft);
            Assert((await repo.GetReservationsAsync()).Single(x => x.Id == id).Total == 168m,"Preço: 2h espaço + 2 acessórios");
            await Reject(() => repo.CreateAsync(draft),"Conflito de espaço");
            await Reject(() => repo.CreateAsync(new NewReservation(client.Id,other.Id,user.Id,start,start.AddHours(2),4,null,items)),"Conflito de estoque entre espaços");
            await Reject(() => repo.CreateAsync(new NewReservation(client.Id,other.Id,user.Id,start,start.AddHours(2),7,null,new ReservationAccessory[0])),"Capacidade");
            await Reject(() => repo.CreateAsync(new NewReservation(client.Id,other.Id,user.Id,start,start,4,null,new ReservationAccessory[0])),"Período vazio");
            await Reject(() => repo.CreateAsync(new NewReservation(client.Id,other.Id,user.Id,start,start.AddHours(1),4,null,new[] { items[0],items[0] })),"Acessório duplicado");
            Assert(await repo.UpdateAsync(id,draft) == id,"Edição ignora o próprio estoque/agenda");
            Assert((await ops.ItemsAsync(id)).Rows.Count == 1,"Edição não duplica itens");
            await Reject(() => catalog.SaveAccessoryAsync(new Accessory(accessory.Id,accessory.Name,null,1,2)),"Não reduzir estoque comprometido");
            var available = (await repo.GetAvailableAccessoriesAsync(start,start.AddHours(2))).Single(x => x.Accessory.Id == accessory.Id);
            Assert(available.Available == 1,"Estoque disponível desconta reservas");
            int adjacent = await repo.CreateAsync(new NewReservation(client.Id,space.Id,user.Id,start.AddHours(2),start.AddHours(3),4,null,new ReservationAccessory[0]));
            Assert(adjacent != id,"Períodos adjacentes não conflitam");
            await ops.CancelAsync(adjacent,"Teste de fronteira");
            await Reject(() => attendantOps.DiscountAsync(id,10,"Negado"),"Desconto exclusivo de administrador");
            await ops.DiscountAsync(id,8,"Demonstração");
            await Reject(() => ops.DiscountAsync(id,169,"Excessivo"),"Desconto não deixa total negativo");
            await ops.PayAsync(id,60,"Pix","Parcial");
            await Reject(() => repo.UpdateAsync(id,draft),"Não editar reserva paga");
            await Reject(() => ops.CancelAsync(id,"Com pagamento"),"Não cancelar pagamento sem estorno");
            await Reject(() => ops.PayAsync(id,101,"Pix",null),"Não aceitar pagamento maior que saldo");
            await Reject(() => ops.PickupAsync(id),"Retirada exige confirmação");
            await attendantOps.ConfirmAsync(id);
            await Reject(() => ops.ConfirmAsync(id),"Não confirmar duas vezes");
            await ops.PickupAsync(id);
            await Reject(() => ops.PickupAsync(id),"Não retirar duas vezes");
            await Reject(() => ops.FinishAsync(id),"Finalização bloqueia pendência de itens");
            var itemId = Convert.ToInt32((await ops.ItemsAsync(id)).Rows[0]["ItemReservaId"]);
            await Reject(() => ops.ReturnAsync(id,new[] { new ReturnLine { ItemId = itemId,Good = 3 } },0,null),"Devolução excessiva");
            await ops.ReturnAsync(id,new[] { new ReturnLine { ItemId = itemId,Good = 1, Damaged = 1 } },10,"Dano verificado");
            Assert((await catalog.GetAccessoriesAsync()).Single(x => x.Id == accessory.Id).QuantityInStock == 2,"Dano retira item do estoque utilizável");
            Assert((await repo.GetAvailableAccessoriesAsync(start,start.AddHours(2))).Single(x => x.Accessory.Id == accessory.Id).Available == 2,"Itens resolvidos deixam de bloquear estoque");
            await Reject(() => ops.ReturnAsync(id,new[] { new ReturnLine { ItemId = itemId,Good = 1 } },0,null),"Não devolver item duas vezes");
            await Reject(() => ops.FinishAsync(id),"Finalização exige quitação");
            await ops.PayAsync(id,110,"Dinheiro","Saldo e multa");
            await ops.FinishAsync(id);
            Assert((await repo.GetReservationsAsync()).Single(x => x.Id == id).Status == "Finalizada","Ciclo completo finalizado");
            Assert((await ops.HistoryAsync(id)).Rows.Count >= 8,"Histórico de ações preservado");
            var revenue = await ops.RevenueAsync(DateTime.Today,DateTime.Today.AddDays(1));
            Assert(revenue.Rows.Cast<DataRow>().Sum(x => Convert.ToDecimal(x["Recebido"])) == 170m,"Relatório de recebimentos sem duplicação");
            Assert((await ops.MostRentedAsync(start.Date,start.Date.AddDays(1))).Rows.Count == 1,"Relatório de acessórios");
            int cancelled = await repo.CreateAsync(new NewReservation(client.Id,space.Id,user.Id,start,start.AddHours(2),4,null,new ReservationAccessory[0]));
            await ops.CancelAsync(cancelled,"Cliente desistiu");
            Assert((await repo.GetReservationsAsync()).Single(x => x.Id == cancelled).Status == "Cancelada","Cancelamento preserva histórico");
            await Reject(() => ops.PayAsync(cancelled,1,"Pix",null),"Cancelada não recebe pagamento");
            var concurrentDraft = new NewReservation(client.Id,other.Id,user.Id,start.AddDays(1),start.AddDays(1).AddHours(1),4,null,new ReservationAccessory[0]);
            var attempts = await Task.WhenAll(TryCreate(repo,concurrentDraft),TryCreate(repo,concurrentDraft));
            Assert(attempts.Count(x => x > 0) == 1,"Criações simultâneas não duplicam agenda");
            await ops.CancelAsync(attempts.Single(x => x > 0),"Fim do teste concorrente");
            var fractional = aiRbPG.Domain.Pricing.PricingCalculator.Calculate(1m/60m,1,0,0,new[] { new aiRbPG.Domain.Pricing.AccessoryCharge(1,1),new aiRbPG.Domain.Pricing.AccessoryCharge(1,1) });
            Assert(fractional.Total == .04m,"Arredondamento do total soma itens individualmente");
            await catalog.DeactivateClientAsync(client.Id);
            Assert(!(await catalog.GetClientsAsync()).Any(x => x.Id == client.Id),"Inativação lógica de cliente");
            Assert((await repo.GetReservationsAsync()).Any(x => x.Id == id),"Histórico sobrevive à inativação");
            await Reject(() => repo.CreateAsync(draft),"Não reservar para cliente inativo");
            await ops.SetUserActiveAsync(attendant.Id,false);
            Assert(!(await auth.AuthenticateAsync("atendente",password)).Succeeded,"Conta inativa sem login");
            await Reject(() => attendantCatalog.SaveClientAsync(new Client(0,"Negado",null,null)),"Sessão de usuário inativo bloqueada");
        }
        private static async Task<int> TryCreate(SqlReservationRepository repo, NewReservation draft)
        { try { return await repo.CreateAsync(draft); } catch (InvalidOperationException) { return 0; } }
    }
}
