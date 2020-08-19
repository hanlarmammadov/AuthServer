using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.UserSystem.Data.Mappings
{
    public static class MappingsInitializer
    {
        public static void InitMappings()
        {
            new AccountMongoDbMap();
            new ConfirmEmailRequestMongoDbMap();
            new ContactsMongoDbMap();
            new UserMongoDbMap();
            new RoleMongoDbMap();
            new EmailChangeRecordMap();
        }
    }
}
