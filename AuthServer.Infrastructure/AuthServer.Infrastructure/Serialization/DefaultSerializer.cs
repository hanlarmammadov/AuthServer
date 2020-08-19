using Newtonsoft.Json;
using System;
using System.Text;

namespace AuthServer.Infrastructure.Serialization
{
    public class DefaultSerializer : ISerializer
    {
        protected JsonSerializerSettings _serializerSettings;

        public DefaultSerializer()
        {
            _serializerSettings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };
        }

        public T Deserialize<T>(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, _serializerSettings); 
        }

        public T Deserialize<T>(byte[] jsonBytes)
        {  
            return Deserialize<T>(Encoding.UTF8.GetString(jsonBytes));
        }

        public string Serialize(Object obj)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj, _serializerSettings);
            return json;
        }
    }
}
