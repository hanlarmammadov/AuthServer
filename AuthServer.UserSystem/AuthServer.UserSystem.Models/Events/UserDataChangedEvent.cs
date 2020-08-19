using AuthServer.Common.Patterns;

namespace AuthServer.UserSystem.Models.Events
{
    public class UserDataChangedEvent : EventBase
    {
        public string AccountId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
