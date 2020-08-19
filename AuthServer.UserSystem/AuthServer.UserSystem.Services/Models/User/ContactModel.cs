using AuthServer.UserSystem.Domain.Entities;

namespace AuthServer.UserSystem.Services.Models
{
    public class ContactModel
    {
        private string _value;

        public string Value { get { return _value; } set { _value = value.ToLower() ?? null; } }
        public UserContactType Type { get; set; }
    }
}
