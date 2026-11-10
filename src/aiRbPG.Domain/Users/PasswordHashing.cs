using System.Security.Cryptography;

namespace aiRbPG.Domain.Users;

public static class PasswordHashing
{
    public const int Iterations = 210_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public static (byte[] Salt, byte[] Hash) Create(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return (salt, hash);
    }

    public static bool Verify(string password, byte[] salt, byte[] expectedHash, int iterations)
    {
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(salt);
        ArgumentNullException.ThrowIfNull(expectedHash);

        if (iterations < 100_000 || expectedHash.Length != HashSize)
        {
            return false;
        }

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
