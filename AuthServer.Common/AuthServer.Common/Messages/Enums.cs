using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.Common.Messages
{
    public enum ResponseCode
    {
        Success = 0,
        ValidationError = 1,
        GeneralError = 2,
        AccessDenied = 3,
    }

    public enum ResponseType
    {
        Response = 1,
        ValueResponse = 2,
        ListResponse = 3
    }
}
