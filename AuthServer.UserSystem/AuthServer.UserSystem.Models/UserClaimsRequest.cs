using AuthServer.Common.Patterns;

namespace AuthServer.UserSystem.Models
{
    public class UserClaimsRequest : MessageBase
    {
        public string AccountId { get; set; }
        public string[] ClaimsConsumers { get; set; }
    }
}
