
namespace AuthServer.UserSystem.Models
{
    public class ResetLinkPasswordRecoveryMailModel
    { 
        public string Username { get; set; }
        public string Email { get; set; }
        public string Url { get; set; }

        public override string ToString()
        {
            return "Password reset link";
        }
    }
}
