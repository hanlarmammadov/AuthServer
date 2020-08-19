
namespace AuthServer.UserSystem.Models
{
    public class ChangeEmailModel
    {
        private string _newEmail; 
        public string NewEmail { get { return _newEmail; } set { _newEmail = value?.ToLower(); } }
    }
}
