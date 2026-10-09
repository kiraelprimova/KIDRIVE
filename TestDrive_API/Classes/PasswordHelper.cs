using System.Security.Cryptography;
using System.Text;

namespace TestDrive_API.Classes
{
    public static class PasswordHelper
    {
        // Хеширование пароля алгоритмом SHA-256
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }

        // Проверка пароля (поддерживает как новый хэш, так и старый открытый пароль)
        public static bool VerifyPassword(string inputPassword, string storedPassword)
        {
            if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedPassword))
                return false;

            string hashedInput = HashPassword(inputPassword);
            return storedPassword.Equals(hashedInput, StringComparison.OrdinalIgnoreCase)
                || storedPassword == inputPassword;
        }
    }
}
