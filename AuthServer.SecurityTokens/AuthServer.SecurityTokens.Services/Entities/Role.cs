
namespace AuthServer.SecurityTokens.Services.Entities
{
    public class Role
    {
        public string Name { get; set; }

        public Role() { }

        public Role(string name)
        {
            Name = name;
        }
    }
}
