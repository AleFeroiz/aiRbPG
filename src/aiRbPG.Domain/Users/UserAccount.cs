namespace aiRbPG.Domain.Users;

public sealed record UserAccount(
    int Id,
    string FullName,
    string Login,
    UserRole Role,
    byte[] PasswordSalt,
    byte[] PasswordHash,
    int HashIterations,
    bool IsActive);
