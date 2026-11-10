namespace aiRbPG.Domain.Catalog;

public sealed record Client(int Id, string Name, string? Phone, string? Email, bool IsActive = true);
