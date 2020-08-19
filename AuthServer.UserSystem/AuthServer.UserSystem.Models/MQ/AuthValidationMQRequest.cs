using AuthServer.Common.Patterns;

namespace AuthServer.UserSystem.Models.MQ
{
    public class AuthValidationMQRequest : MessageBase
    {
        public string UsernameOrEmail { get; set; }
        public string Password { get; set; }
    }
}
