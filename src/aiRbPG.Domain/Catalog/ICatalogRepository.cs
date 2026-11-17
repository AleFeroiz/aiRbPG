using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace aiRbPG.Domain.Catalog
{
public interface ICatalogRepository
{
    Task<IReadOnlyList<Client>> GetClientsAsync(CancellationToken cancellationToken = default(CancellationToken));
    Task SaveClientAsync(Client client, CancellationToken cancellationToken = default(CancellationToken));
    Task DeactivateClientAsync(int clientId, CancellationToken cancellationToken = default(CancellationToken));

    Task<IReadOnlyList<RpgSpace>> GetSpacesAsync(CancellationToken cancellationToken = default(CancellationToken));
    Task SaveSpaceAsync(RpgSpace space, CancellationToken cancellationToken = default(CancellationToken));
    Task DeactivateSpaceAsync(int spaceId, CancellationToken cancellationToken = default(CancellationToken));

    Task<IReadOnlyList<RpgSystem>> GetRpgSystemsAsync(CancellationToken cancellationToken = default(CancellationToken));
    Task SaveRpgSystemAsync(RpgSystem system, CancellationToken cancellationToken = default(CancellationToken));
    Task DeactivateRpgSystemAsync(int systemId, CancellationToken cancellationToken = default(CancellationToken));

    Task<IReadOnlyList<Accessory>> GetAccessoriesAsync(CancellationToken cancellationToken = default(CancellationToken));
    Task SaveAccessoryAsync(Accessory accessory, CancellationToken cancellationToken = default(CancellationToken));
    Task DeactivateAccessoryAsync(int accessoryId, CancellationToken cancellationToken = default(CancellationToken));
}
}
