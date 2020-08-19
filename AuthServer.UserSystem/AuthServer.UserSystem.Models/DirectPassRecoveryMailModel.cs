
namespace AuthServer.UserSystem.Models
{
    public class DirectPasswordSetEvent
    {
        public string Username { get; set; }
        public string Email { get; set; } 
        public string PasswordPlain { get; set; }
    }
}
