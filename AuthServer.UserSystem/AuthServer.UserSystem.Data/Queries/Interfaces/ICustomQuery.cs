using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.UserSystem.Data.Queries.Interfaces
{
    public interface ICustomQuery<TResult, TUnitOfWork>
    {
        TResult Execute(TUnitOfWork unitOfWork);
    }
}
