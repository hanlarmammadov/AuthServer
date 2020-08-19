using AuthServer.UserSystem.Domain.Entities;

namespace AuthServer.UserSystem.Services.Models
{ 
    public class RoleCreateModel
    {
        private string _id;
        private string _name;
        private string _description;
        private string _consumer;

        public string Id
        {
            get { return _id; }
            set { _id = value.ToLower() ?? null; }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value.ToLower() ?? null; }
        }
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
        public string Consumer
        {
            get { return _consumer; }
            set { _consumer = value.ToLower() ?? null; }
        }
        public RoleStatus Status { get; set; } 
    }
}
