
namespace AuthServer.UserSystem.Services.Models
{
    public class ListQueryModel
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string Order { get; set; } 
        public bool IsDesc { get; set; }
    }
}
