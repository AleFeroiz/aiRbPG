using System;
using System.Security.Cryptography;

namespace aiRbPG.Domain.Users
{
public static class PasswordHashing
{
    public const int Iterations = 210000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public static void Create(string password, out byte[] salt, out byte[] hash)
    {
        if (password == null) throw new ArgumentNullException("password");
        salt = new byte[SaltSize];
        using (var random = RandomNumberGenerator.Create()) random.GetBytes(salt);
        hash = DeriveSha256(password, salt, Iterations, HashSize);
    }

    public static bool Verify(string password, byte[] salt, byte[] expectedHash, int iterations)
    {
        if (password == null || salt == null || expectedHash == null) return false;

        if (iterations < 100000 || expectedHash.Length != HashSize)
        {
            return false;
        }

        var actualHash = DeriveSha256(password, salt, iterations, expectedHash.Length);
        var difference = 0;
        for (var index = 0; index < actualHash.Length; index++) difference |= actualHash[index] ^ expectedHash[index];
        return difference == 0;
    }

    private static byte[] DeriveSha256(string password, byte[] salt, int iterations, int outputLength)
    {
        var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
        var output = new byte[outputLength];
        var blockCount = (outputLength + 31) / 32;
        using (var hmac = new HMACSHA256(passwordBytes))
        {
            for (var block = 1; block <= blockCount; block++)
            {
                var input = new byte[salt.Length + 4];
                Buffer.BlockCopy(salt, 0, input, 0, salt.Length);
                input[input.Length - 1] = (byte)block;
                var current = hmac.ComputeHash(input);
                var aggregate = (byte[])current.Clone();
                for (var round = 1; round < iterations; round++)
                {
                    current = hmac.ComputeHash(current);
                    for (var index = 0; index < aggregate.Length; index++) aggregate[index] ^= current[index];
                }
                var copyLength = Math.Min(32, outputLength - ((block - 1) * 32));
                Buffer.BlockCopy(aggregate, 0, output, (block - 1) * 32, copyLength);
            }
        }
        Array.Clear(passwordBytes, 0, passwordBytes.Length);
        return output;
    }
}
}
