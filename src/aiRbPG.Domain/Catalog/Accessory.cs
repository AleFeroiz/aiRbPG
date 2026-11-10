namespace aiRbPG.Domain.Catalog;

public sealed record Accessory(
    int Id,
    string Name,
    string? Description,
    int QuantityInStock,
    decimal HourlyRate,
    bool IsActive = true);
