using AuthServer.UserSystem.Domain.Entities;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.UserSystem.Data.Mappings
{
    public class RoleMongoDbMap
    {
        public RoleMongoDbMap()
        {
            BsonClassMap.RegisterClassMap<Role>(cm =>
            {
                cm.SetIgnoreExtraElements(true);

                cm.MapIdMember(c => c.Id);
                cm.MapMember(c => c.Name);
                cm.MapMember(c => c.Description);
                cm.MapMember(c => c.Consumer);
                cm.MapMember(c => c.Status);
                cm.MapMember(c => c.CreateDate);
            }); 
        }
    }
}
