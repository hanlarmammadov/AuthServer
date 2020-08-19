using AuthServer.Common.Patterns;

namespace AuthServer.UserSystem.Models.Events
{
    public class AccountEmailChangedEvent : EventBase
    {
        public string EmailChangeRecordId { get; set; }
        public string AccountId { get; set; }
        public string Username { get; set; }
        public bool OldEmailIsConfirmed { get; set; }
        public string OldEmail { get; set; }
        public string NewEmail { get; set; }
    }
}
