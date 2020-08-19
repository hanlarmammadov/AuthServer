using AuthServer.Common.Patterns;

namespace AuthServer.UserSystem.Models.Events
{
    public class RoleCreatedOrEditedEvent : EventBase
    { 
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Consumer { get; set; }
        public bool RoleIsActive { get; set; }
    }
}
