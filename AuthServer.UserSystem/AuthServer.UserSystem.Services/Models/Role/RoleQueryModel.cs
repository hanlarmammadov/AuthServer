using AuthServer.UserSystem.Domain.Entities;

namespace AuthServer.UserSystem.Services.Models
{
    public class RoleQueryModel : ListQueryModel
    {
        public string Name { get; set; }
        public string Consumer { get; set; }
        public RoleStatus Status { get; set; }
    }
}
