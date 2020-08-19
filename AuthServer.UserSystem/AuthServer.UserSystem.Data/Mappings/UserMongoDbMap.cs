using AuthServer.UserSystem.Domain.Entities;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.UserSystem.Data.Mappings
{ 
    public class UserMongoDbMap
    {
        public UserMongoDbMap()
        {
            BsonClassMap.RegisterClassMap<User>(cm =>
            { 
                cm.SetIgnoreExtraElements(true);

                cm.MapIdMember(c => c.AccountId);
                cm.MapMember(c => c.FirstName);
                cm.MapMember(c => c.LastName);
                cm.MapMember(c => c.Gender);
                cm.MapMember(c => c.CreateDate);
                cm.MapMember(c => c.ModifiedDate);
                cm.MapMember(c => c.Contacts).SetElementName("contacts");
                cm.MapMember(c => c.RoleIds).SetElementName("roles"); 
            });
        }
    }
}
