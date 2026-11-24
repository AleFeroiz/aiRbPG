using System.Threading;
using System.Threading.Tasks;

namespace aiRbPG.Domain.Users
{

public interface IUserRepository
{
    Task<bool> HasAnyUsersAsync(CancellationToken cancellationToken = default(CancellationToken));

    Task<UserAccount> FindByLoginAsync(
        string login,
        CancellationToken cancellationToken = default(CancellationToken));

    Task<OperationResult<bool>> CreateFirstAdministratorAsync(
        string fullName,
        string login,
        byte[] passwordSalt,
        byte[] passwordHash,
        int hashIterations,
        CancellationToken cancellationToken = default(CancellationToken));
}
}
