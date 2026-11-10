namespace aiRbPG.Domain.Catalog;

public interface ICatalogRepository
{
    Task<IReadOnlyList<Client>> GetClientsAsync(CancellationToken cancellationToken = default);
    Task SaveClientAsync(Client client, CancellationToken cancellationToken = default);
    Task DeactivateClientAsync(int clientId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RpgSpace>> GetSpacesAsync(CancellationToken cancellationToken = default);
    Task SaveSpaceAsync(RpgSpace space, CancellationToken cancellationToken = default);
    Task DeactivateSpaceAsync(int spaceId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RpgSystem>> GetRpgSystemsAsync(CancellationToken cancellationToken = default);
    Task SaveRpgSystemAsync(RpgSystem system, CancellationToken cancellationToken = default);
    Task DeactivateRpgSystemAsync(int systemId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Accessory>> GetAccessoriesAsync(CancellationToken cancellationToken = default);
    Task SaveAccessoryAsync(Accessory accessory, CancellationToken cancellationToken = default);
    Task DeactivateAccessoryAsync(int accessoryId, CancellationToken cancellationToken = default);
}
