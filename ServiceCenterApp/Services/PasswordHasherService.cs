using ServiceCenterApp.Services.Interfaces;
using System.Security.Cryptography; 
using System.Text;                  

namespace ServiceCenterApp.Services
{
    public class PasswordHashService : IPasswordHasher
    {
        public string Hash(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                    sb.Append(hashBytes[i].ToString("x2"));
                return sb.ToString();
            }
        }

        public bool Verify(string password, string hash)
        {
            string hashOfInput = Hash(password);
            return StringComparer.OrdinalIgnoreCase.Compare(hashOfInput, hash) == 0;
        }
    }
}