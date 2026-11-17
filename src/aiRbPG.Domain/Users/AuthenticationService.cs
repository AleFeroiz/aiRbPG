using System;
using System.Threading;
using System.Threading.Tasks;

namespace aiRbPG.Domain.Users
{
public sealed class AuthenticationService
{
    private readonly IUserRepository users;
    public AuthenticationService(IUserRepository users) { this.users = users; }
    public const int MinimumPasswordLength = 10;

    public async Task<bool> NeedsInitialAdministratorAsync(CancellationToken cancellationToken = default(CancellationToken)) =>
        !await users.HasAnyUsersAsync(cancellationToken);

    public async Task CreateInitialAdministratorAsync(
        string fullName,
        string login,
        string password,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        fullName = fullName.Trim();
        login = login.Trim();

        if (fullName.Length == 0 || login.Length == 0)
        {
            throw new ArgumentException("Informe o nome completo e o usuário.");
        }

        if (password.Length < MinimumPasswordLength)
        {
            throw new ArgumentException($"A senha deve ter pelo menos {MinimumPasswordLength} caracteres.");
        }

        if (await users.HasAnyUsersAsync(cancellationToken))
        {
            throw new InvalidOperationException("O Administrador inicial já foi criado.");
        }

        byte[] salt;
        byte[] hash;
        PasswordHashing.Create(password, out salt, out hash);
        await users.CreateFirstAdministratorAsync(
            fullName,
            login,
            salt,
            hash,
            PasswordHashing.Iterations,
            cancellationToken);
    }

    public async Task<AuthenticationResult> AuthenticateAsync(
        string login,
        string password,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrEmpty(password))
        {
            return AuthenticationResult.Failure("Informe usuário e senha.");
        }

        var user = await users.FindByLoginAsync(login.Trim(), cancellationToken);
        if (user == null || !user.IsActive ||
            !PasswordHashing.Verify(password, user.PasswordSalt, user.PasswordHash, user.HashIterations))
        {
            return AuthenticationResult.Failure("Usuário ou senha inválidos.");
        }

        return AuthenticationResult.Success(user);
    }
}
}
