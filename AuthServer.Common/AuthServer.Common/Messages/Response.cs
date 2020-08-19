using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.Common.Messages
{
    public class Response : IResponse
    {
        //public bool Ok { get { return this.Code == ResponseCode.Success; } }
        public ResponseCode Code { get; set; }
        public ResponseType Type { get; set; }
        public IEnumerable<Message> Messages { get; set; }

        #region Ctors
        /// <summary>
        /// Default constructor for generating ValueResponse during such processes as deserialization etc. You should not use it
        /// </summary>
        public Response()
        {

        }

        protected Response(ResponseCode code, ResponseType type, Message message)
            : this(code, type, new Message[1] { message })
        {

        }

        protected Response(ResponseCode code, ResponseType type, IEnumerable<Message> messages)
        {
            this.Code = code;
            this.Type = type;
            this.Messages = messages;
        }
        #endregion

        #region Statics
        public static IResponse Success(string message = null)
        {
            return new Response(ResponseCode.Success, ResponseType.Response, new Message(message));
        }

        public static IResponse ValidationError(IEnumerable<Message> errors)
        {
            return new Response(ResponseCode.ValidationError, ResponseType.Response, errors);
        }

        public static IResponse GeneralError(string error)
        {
            return new Response(ResponseCode.GeneralError, ResponseType.Response, new Message(error));
        }

        public static IResponse AccessDenied(string error)
        {
            return new Response(ResponseCode.AccessDenied, ResponseType.Response, new Message(error));
        }
        #endregion
    }
}
