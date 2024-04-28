using Org.BouncyCastle.Crypto.Generators;

namespace web_project_BE.Helper
{
    public class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            string pwdHash = BCrypt.Net.BCrypt.HashPassword(password);
            return pwdHash;
        }

        public static bool VerifyPassword(string password, string pwdHash)
        {
            bool verified = BCrypt.Net.BCrypt.Verify(password, pwdHash);
            return verified;
        }
    }
}
