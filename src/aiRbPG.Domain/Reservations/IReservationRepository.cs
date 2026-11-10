using aiRbPG.Domain.Catalog;

namespace aiRbPG.Domain.Reservations;

public interface IReservationRepository
{
    Task<IReadOnlyList<Reservation>> GetReservationsAsync(CancellationToken cancellationToken = default);
    Task<int> CreateAsync(NewReservation reservation, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(Accessory Accessory, int Available)>> GetAvailableAccessoriesAsync(
        DateTime start, DateTime end, CancellationToken cancellationToken = default);
}
