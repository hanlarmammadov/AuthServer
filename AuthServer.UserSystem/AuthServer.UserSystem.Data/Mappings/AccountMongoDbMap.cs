using AuthServer.UserSystem.Domain.Entities;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.UserSystem.Data.Mappings
{
    public class AccountMongoDbMap
    {
        public AccountMongoDbMap()
        {
            BsonClassMap.RegisterClassMap<Account>(cm =>
            {
                cm.SetIgnoreExtraElements(true);

                cm.MapIdMember(c => c.AccountId);
                cm.MapMember(c => c.Username);
                cm.MapMember(c => c.Password);
                cm.MapMember(c => c.Salt);
                cm.MapMember(c => c.Email);
                cm.MapMember(c => c.PasswordStatus);
                cm.MapMember(c => c.EmailStatus);
                cm.MapMember(c => c.AccountStatus);
                cm.MapMember(c => c.AccountDataStatus);
                cm.MapMember(c => c.PasswordLastChanged);
                cm.MapMember(c => c.AccountCreated);
            });
        }
    }
}
