using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Common.Validation
{
    public interface IValidatorFactory
    {
        IValidator Create();
    }
}
