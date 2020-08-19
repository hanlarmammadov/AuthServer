
namespace AuthServer.Infrastructure.Serialization
{
    public interface ISerializer
    {
        T Deserialize<T>(string json);
        T Deserialize<T>(byte[] jsonBytes);
        string Serialize(object obj);
    }
}
