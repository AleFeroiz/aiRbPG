namespace aiRbPG.Domain.Reservations;

public sealed record ReservationAccessory(int AccessoryId, string Name, int Quantity, decimal HourlyRate);

public sealed record Reservation(
    int Id,
    string ClientName,
    string SpaceName,
    DateTime Start,
    DateTime End,
    int People,
    string Status,
    decimal Total);

public sealed record NewReservation(
    int ClientId,
    int SpaceId,
    int CreatedByUserId,
    DateTime Start,
    DateTime End,
    int People,
    string? Notes,
    IReadOnlyList<ReservationAccessory> Accessories);
