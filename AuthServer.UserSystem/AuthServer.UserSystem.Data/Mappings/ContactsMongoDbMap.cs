using AuthServer.UserSystem.Domain.Entities;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.UserSystem.Data.Mappings
{
    public class ContactsMongoDbMap
    { 
        public ContactsMongoDbMap()
        {
            BsonClassMap.RegisterClassMap<Contact>(cm =>
            {
                cm.SetIgnoreExtraElements(true);
                 
                cm.MapMember(c => c.Value);
                cm.MapMember(c => c.Type); 
            });
        }
    }
}
