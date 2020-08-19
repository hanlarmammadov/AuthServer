using System;

namespace AuthServer.UserSystem.Domain.Entities
{
    public enum RoleStatus
    {
        NotSet = 0,
        Active = 1,
        Inactive = 2
    }

    public class Role
    {
        public Role() { }
        public Role(string roleId)
        {
            Id = roleId;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Consumer { get; set; }
        public RoleStatus Status { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
