using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.Common.Messages
{
    public interface IValueResponse<TValue> : IResponse
    {
        TValue Value { get; }
    }
}
