namespace aiRbPG.Domain.Users
{
    public sealed class UserAccount
    {
        public UserAccount(int id, string fullName, string login, UserRole role, byte[] passwordSalt, byte[] passwordHash, int hashIterations, bool isActive)
        { Id = id; FullName = fullName; Login = login; Role = role; PasswordSalt = passwordSalt; PasswordHash = passwordHash; HashIterations = hashIterations; IsActive = isActive; }
        public int Id { get; private set; }
        public string FullName { get; private set; }
        public string Login { get; private set; }
        public UserRole Role { get; private set; }
        public byte[] PasswordSalt { get; private set; }
        public byte[] PasswordHash { get; private set; }
        public int HashIterations { get; private set; }
        public bool IsActive { get; private set; }
    }
}
