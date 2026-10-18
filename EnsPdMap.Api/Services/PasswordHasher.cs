using System.Security.Cryptography;

namespace EnsPdMap.Api.Services;

/// <summary>Hachage PBKDF2-SHA256 avec sel aléatoire (aucune dépendance externe).</summary>
public static class PasswordHasher
{
    const int Iterations = 100_000;
    const int HashSize = 32;
    const int SaltSize = 16;

    public static (string Hash, string Salt) Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public static bool Verify(string password, string hash, string salt)
    {
        byte[] expected = Convert.FromBase64String(hash);
        byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, Convert.FromBase64String(salt), Iterations, HashAlgorithmName.SHA256, HashSize);
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }
}
