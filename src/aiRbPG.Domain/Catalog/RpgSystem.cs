namespace aiRbPG.Domain.Catalog;

public sealed record RpgSystem(int Id, string Name, string? Description, bool IsActive = true);
