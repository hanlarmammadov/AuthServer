using AuthServer.Common.Patterns;

namespace AuthServer.UserSystem.Models.MQ
{
    public class UserClaimsMQRequest : MessageBase
    {
        public string AccountId { get; set; }
        public string[] ClaimsConsumers { get; set; }
    }
}
