using ServiceCenterApp.Services.Interfaces;
using System.Security.Cryptography; 
using System.Text;                  

namespace ServiceCenterApp.Services
{
    public class PasswordHashService : IPasswordHasher
    {
        public string Hash(string password)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes = SHA256.HashData(inputBytes);

            StringBuilder sb = new();
            for (int i = 0; i < hashBytes.Length; i++)
                sb.Append(hashBytes[i].ToString("x2"));
            return sb.ToString();
        }

        public bool Verify(string password, string hash)
        {
            string hashOfInput = Hash(password);
            return StringComparer.OrdinalIgnoreCase.Compare(hashOfInput, hash) == 0;
        }
    }
}