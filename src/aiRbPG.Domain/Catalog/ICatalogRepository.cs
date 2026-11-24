using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace aiRbPG.Domain.Catalog
{
public interface ICatalogRepository
{
    Task<IReadOnlyList<Client>> GetClientsAsync(CancellationToken cancellationToken = default(CancellationToken));
    Task<OperationResult<bool>> SaveClientAsync(Client client, CancellationToken cancellationToken = default(CancellationToken));
    Task<OperationResult<bool>> DeactivateClientAsync(int clientId, CancellationToken cancellationToken = default(CancellationToken));

    Task<IReadOnlyList<RpgSpace>> GetSpacesAsync(CancellationToken cancellationToken = default(CancellationToken));
    Task<OperationResult<bool>> SaveSpaceAsync(RpgSpace space, CancellationToken cancellationToken = default(CancellationToken));
    Task<OperationResult<bool>> DeactivateSpaceAsync(int spaceId, CancellationToken cancellationToken = default(CancellationToken));

    Task<IReadOnlyList<RpgSystem>> GetRpgSystemsAsync(CancellationToken cancellationToken = default(CancellationToken));
    Task<OperationResult<bool>> SaveRpgSystemAsync(RpgSystem system, CancellationToken cancellationToken = default(CancellationToken));
    Task<OperationResult<bool>> DeactivateRpgSystemAsync(int systemId, CancellationToken cancellationToken = default(CancellationToken));

    Task<IReadOnlyList<Accessory>> GetAccessoriesAsync(CancellationToken cancellationToken = default(CancellationToken));
    Task<OperationResult<bool>> SaveAccessoryAsync(Accessory accessory, CancellationToken cancellationToken = default(CancellationToken));
    Task<OperationResult<bool>> DeactivateAccessoryAsync(int accessoryId, CancellationToken cancellationToken = default(CancellationToken));
}
}
