namespace aiRbPG.Domain.Users;

public interface IUserRepository
{
    Task<bool> HasAnyUsersAsync(CancellationToken cancellationToken = default);

    Task<UserAccount?> FindByLoginAsync(
        string login,
        CancellationToken cancellationToken = default);

    Task CreateFirstAdministratorAsync(
        string fullName,
        string login,
        byte[] passwordSalt,
        byte[] passwordHash,
        int hashIterations,
        CancellationToken cancellationToken = default);
}
