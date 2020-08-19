using System.Collections.Generic;

namespace AuthServer.Common.Messages
{
    public interface IListResponse<TListElem> : IResponse
    {
        IEnumerable<TListElem> Value { get; }
        long TotalItems { get; }
    }
}
