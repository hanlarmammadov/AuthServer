using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Common.Messages
{
    public interface IPage<T>
    {
        IEnumerable<T> List { get; }
        long TotalItems { get; }
    }
}
