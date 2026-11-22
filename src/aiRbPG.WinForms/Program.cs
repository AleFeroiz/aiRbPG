using System;
using System.Windows.Forms;

namespace aiRbPG.WinForms
{
internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var connectionFactory = new aiRbPG.Data.SqlConnectionFactory();
        var userRepository = new aiRbPG.Data.Users.SqlUserRepository(connectionFactory);
        var reservationRepository = new aiRbPG.Data.Reservations.SqlReservationRepository(connectionFactory);
        var authentication = new aiRbPG.Domain.Users.AuthenticationService(userRepository);

        using (var login = new Forms.LoginForm(authentication))
        {
            if (login.ShowDialog() == DialogResult.OK && login.AuthenticatedUser != null)
            {
                var catalogRepository = new aiRbPG.Data.Catalog.SqlCatalogRepository(connectionFactory, login.AuthenticatedUser);
                Application.Run(new Forms.DashboardForm(login.AuthenticatedUser, catalogRepository, reservationRepository));
            }
        }
    }
}
}
