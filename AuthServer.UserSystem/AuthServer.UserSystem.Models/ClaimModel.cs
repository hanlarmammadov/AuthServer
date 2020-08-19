
namespace AuthServer.UserSystem.Models
{
    public class ClaimModel
    {
        public string Type { get; set; } 
        public string Value { get; set; }

        public ClaimModel() { }
        public ClaimModel(string type, string value)
        {
            Type = type;
            Value = value;
        } 
    }
}
