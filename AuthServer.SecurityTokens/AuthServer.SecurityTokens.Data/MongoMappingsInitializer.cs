using AuthServer.SecurityTokens.Entities;
using MongoDB.Bson.Serialization;

namespace AuthServer.SecurityTokens.Data
{
    public static class MongoMappingsInitializer
    {
        public static void Init()
        {
            #region AccountRToken maps

            BsonClassMap.RegisterClassMap<AccountRTokenInfo>(cm =>
                {
                    cm.SetIgnoreExtraElements(true);

                    cm.MapIdMember(c => c.TokenId);
                    cm.MapMember(c => c.AccountId);
                    cm.MapMember(c => c.Status);
                    cm.MapMember(c => c.DeviceInfo);
                    cm.MapMember(c => c.RequesterIPv4);
                    cm.MapMember(c => c.RequesterIPv6);
                    cm.MapMember(c => c.CreateDate);
                    cm.MapMember(c => c.ExpireDate);
                    cm.MapMember(c => c.ModifiedDate);
                });

            #endregion
        }
    }
}
