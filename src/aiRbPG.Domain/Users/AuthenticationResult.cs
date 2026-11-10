namespace aiRbPG.Domain.Users;

public sealed record AuthenticationResult(bool Succeeded, UserAccount? User, string Message)
{
    public static AuthenticationResult Success(UserAccount user) => new(true, user, "Acesso autorizado.");

    public static AuthenticationResult Failure(string message) => new(false, null, message);
}
