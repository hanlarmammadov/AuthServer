using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Common.Messages
{
    public class Page<T> : IPage<T>
    {
        public Page(IEnumerable<T> list, long totalItems)
        {
            List = list;
            TotalItems = totalItems;
        }

        public IEnumerable<T> List { get; private set; }
        public long TotalItems { get; private set; }
    }
}
