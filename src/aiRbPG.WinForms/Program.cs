namespace aiRbPG.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var connectionFactory = new aiRbPG.Data.SqlConnectionFactory();
        var userRepository = new aiRbPG.Data.Users.SqlUserRepository(connectionFactory);
        var catalogRepository = new aiRbPG.Data.Catalog.SqlCatalogRepository(connectionFactory);
        var reservationRepository = new aiRbPG.Data.Reservations.SqlReservationRepository(connectionFactory);
        var authentication = new aiRbPG.Domain.Users.AuthenticationService(userRepository);

        using var login = new Forms.LoginForm(authentication);
        if (login.ShowDialog() == DialogResult.OK && login.AuthenticatedUser is { } user)
        {
            Application.Run(new Forms.DashboardForm(user, catalogRepository, reservationRepository));
        }
    }
}
