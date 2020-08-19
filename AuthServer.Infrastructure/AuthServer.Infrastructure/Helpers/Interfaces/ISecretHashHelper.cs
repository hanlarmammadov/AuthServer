using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Infrastructure.Helpers.Interfaces
{
    public interface ISecretHashHelper
    {
        string GenerateHash(string secretPlain);
        bool ValidateSecret(string secretPlain, string secretHashed);
    }
}
