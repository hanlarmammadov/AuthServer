
namespace AuthServer.SecurityTokens.Models
{
    public class RTokenRenewModel
    {
        public string OldRToken { get; set; }
        public string[] ClaimsConsumers { get; set; }
    }
}
