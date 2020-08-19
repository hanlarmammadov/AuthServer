using AuthServer.Common.Patterns;

namespace AuthServer.UserSystem.Models
{
    public class AuthValidationRequest : MessageBase
    {
        public string UsernameOrEmail { get; set; }
        public string Password { get; set; }
    }
}
