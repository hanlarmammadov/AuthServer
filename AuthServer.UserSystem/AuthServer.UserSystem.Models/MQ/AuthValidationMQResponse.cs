
using AuthServer.Common.Patterns;

namespace AuthServer.UserSystem.Models.MQ
{
    public class AuthValidationMQResponse : MessageBase
    {
        public bool OpSuccess { get; set; }
        public bool IsValid { get; set; }
        public string AccountId { get; set; }
    }
}
