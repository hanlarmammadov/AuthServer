
namespace AuthServer.SecurityTokens.Models
{
    public class CredModel
    {
        public string Username { get; set; } 
        public string Password { get; set; }
        public string[] ClaimsConsumers { get; set; }
    }
}
