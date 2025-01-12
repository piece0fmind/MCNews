using System.Security.Cryptography;
using System.Text;

namespace Application.Infrastructure
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int hashWorkFactor = 10;
        
        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, hashWorkFactor);
        }

    }
}
