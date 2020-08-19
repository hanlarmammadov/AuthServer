
namespace AuthServer.UserSystem.Domain.Entities
{
    public class Contact
    {
        public Contact() { } 

        public string Value { get; set; }
        public UserContactType Type { get; set; }
    }
}
