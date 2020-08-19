using AuthServer.UserSystem.Domain.Entities;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.UserSystem.Data.Mappings
{
    public class EmailChangeRecordMap
    { 
        public EmailChangeRecordMap()
        {
            BsonClassMap.RegisterClassMap<EmailChangeRecord>(cm =>
            {
                cm.SetIgnoreExtraElements(true);

                cm.MapIdMember(c => c.RecordId);
                cm.MapMember(c => c.AccountId);
                cm.MapMember(c => c.OldEmail);
                cm.MapMember(c => c.NewEmail);
                cm.MapMember(c => c.CreateDate);
                cm.MapMember(c => c.UpdateDate);
                cm.MapMember(c => c.Status);  
            });
        }
    }
}
