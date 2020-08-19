
namespace AuthServer.UserSystem.Services.Models
{
    public class CompletePasswordRecoveryModel
    {
        public string Code { get; set; }
        public string UsernameOrEmail { get; set; }
        public string NewPassword { get; set; }
    }
}
