namespace aiRbPG.Domain.Catalog;

public sealed record RpgSpace(
    int Id,
    string Name,
    string? Description,
    string? Address,
    int Capacity,
    decimal HourlyRate,
    decimal PerPersonHourlyRate,
    bool IsActive = true);
