using AuthServer.Infrastructure.Helpers.Interfaces;  
using System.Security.Cryptography;
using System.Text; 

namespace AuthServer.Infrastructure.Helpers
{
    public class SecretSha256Helper : ISecretHashHelper
    {
        private static string HexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }

        public string GenerateHash(string secretPlain)
        {
            if (secretPlain == null) { return null; }

            byte[] bytes = Encoding.UTF8.GetBytes(secretPlain);

            var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(bytes);

            return HexStringFromBytes(hashBytes);

        }
        public bool ValidateSecret(string secretPlain, string secretHashed)
        {
            string generatedHashedPassword = GenerateHash(secretPlain);
            return (generatedHashedPassword == secretHashed);
        }
    }
}
