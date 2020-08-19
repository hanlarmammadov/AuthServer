
namespace AuthServer.SecurityTokens.Services.Entities
{
    public enum CredStatus
    {
        NotSet = 0,
        Ok = 999
    }

    public class Credential
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public CredStatus Status { get; set; }

        public Credential() { }

        public Credential(string username, string password, CredStatus status = CredStatus.Ok)
        {
            Username = username;
            Password = password;
            Status = status;
        }
    }
}
