using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.Common.Messages
{
    public interface IResponse
    {
        //bool Ok { get; }
        ResponseCode Code { get; }
        ResponseType Type { get; }
        IEnumerable<Message> Messages { get; }
    }
}
