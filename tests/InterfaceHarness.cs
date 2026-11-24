// Visual QA entry point. It only runs against an isolated aiRbPG_UI_* database.
using System;
using System.Linq;
using System.Windows.Forms;
using aiRbPG.Data;
using aiRbPG.Data.Users;
using aiRbPG.Data.Catalog;
using aiRbPG.Data.Reservations;
using aiRbPG.Domain.Users;
using aiRbPG.Domain.Reservations;
class InterfaceHarness
{
    static void Check<T>(aiRbPG.Domain.OperationResult<T> result)
    { if (!result.Succeeded) throw new Exception(result.Error); }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
        var factory = new SqlConnectionFactory();
        using (var connection = factory.Create()) { connection.Open(); if (!connection.Database.StartsWith("aiRbPG_UI_",StringComparison.Ordinal)) throw new Exception("Banco de QA obrigatório."); }
        var users = new SqlUserRepository(factory); var auth = new AuthenticationService(users);
        if (auth.NeedsInitialAdministratorAsync().GetAwaiter().GetResult())
            Check(auth.CreateInitialAdministratorAsync("Operador de Demonstração","qa",Guid.NewGuid().ToString("N")).GetAwaiter().GetResult());
        var user = users.FindByLoginAsync("qa").GetAwaiter().GetResult();
        var catalog = new SqlCatalogRepository(factory,user); var reservations = new SqlReservationRepository(factory);
        if (reservations.GetReservationsAsync().GetAwaiter().GetResult().Count == 0)
        {
            var client = catalog.GetClientsAsync().GetAwaiter().GetResult().First();
            var space = catalog.GetSpacesAsync().GetAwaiter().GetResult().First();
            var accessory = catalog.GetAccessoriesAsync().GetAwaiter().GetResult().First();
            var start = new DateTime(2026,12,1,14,0,0);
            Check(reservations.CreateAsync(new NewReservation(client.Id,space.Id,user.Id,start,start.AddHours(2),4,"Reserva de demonstração",new[] { new ReservationAccessory(accessory.Id,accessory.Name,2,accessory.HourlyRate) })).GetAwaiter().GetResult());
        }
        Application.Run(new aiRbPG.WinForms.Forms.DashboardForm(user,catalog,reservations));
    }
}
