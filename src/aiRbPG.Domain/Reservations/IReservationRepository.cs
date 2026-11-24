using aiRbPG.Domain.Catalog;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace aiRbPG.Domain.Reservations
{
    public interface IReservationRepository
    {
        Task<IReadOnlyList<Reservation>> GetReservationsAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<OperationResult<int>> CreateAsync(NewReservation reservation, CancellationToken cancellationToken = default(CancellationToken));
        Task<OperationResult<int>> UpdateAsync(int id, NewReservation reservation, CancellationToken cancellationToken = default(CancellationToken));
        Task<IReadOnlyList<AccessoryAvailability>> GetAvailableAccessoriesAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default(CancellationToken));
    }
}
