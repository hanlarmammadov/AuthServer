using AuthServer.Common.Patterns;

namespace AuthServer.UserSystem.Models.Event
{
    public class EmailConfirmationByLinkRequestEvent : EventBase
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string ConfirmationUrl { get; set; }
    }
}
