using AuthServer.UserSystem.Domain.Entities;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.UserSystem.Data.Mappings
{
    public class ConfirmEmailRequestMongoDbMap
    {
        public ConfirmEmailRequestMongoDbMap()
        {
            BsonClassMap.RegisterClassMap<ConfirmEmailRequest>(cm =>
            {
                cm.SetIgnoreExtraElements(true);

                cm.MapIdMember(c => c.Id);
                cm.MapMember(c => c.AccountId).SetElementName("AccountId");
                cm.MapMember(c => c.Email).SetElementName("Email");
                cm.MapMember(c => c.Code).SetElementName("Code");
                cm.MapMember(c => c.CreateDate).SetElementName("CreateDate");
                cm.MapMember(c => c.Status).SetElementName("Status");
                cm.MapMember(c => c.ResolveDate).SetElementName("ResolveDate");
            });
        }
    }
}
