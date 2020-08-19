using AuthServer.Common.Patterns;

namespace AuthServer.UserSystem.Models.Events
{
    public class AccountEmailChangeUndoEvent : EventBase
    {
        public string AccountId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }
}
