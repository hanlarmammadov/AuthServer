using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.Common.Messages
{
    public class ValueResponse<TValue> : Response, IValueResponse<TValue>
    {
        public TValue Value { get; set; }

        #region Ctors
        /// <summary>
        /// Default constructor for generating ValueResponse during such processes as deserialization etc. You should not use it
        /// </summary>
        public ValueResponse()
        {

        }

        protected ValueResponse(TValue value, ResponseCode code, ResponseType type, Message message)
            : this(value, code, type, new Message[1] { message })
        {

        }

        protected ValueResponse(TValue value, ResponseCode code, ResponseType type, IEnumerable<Message> messages)
            : base(code, type, messages)
        {
            this.Value = value;
        }
        #endregion

        #region Statics

        public static IValueResponse<TValue> Success(TValue value, string message = null)
        { 
            return new ValueResponse<TValue>(value, ResponseCode.Success, ResponseType.Response, (message != null) ? new Message(message) : null);
        }

        public static new IValueResponse<TValue> ValidationError(IEnumerable<Message> errors)
        {
            return new ValueResponse<TValue>(default(TValue), ResponseCode.ValidationError, ResponseType.Response, errors);
        }

        public static new IValueResponse<TValue> GeneralError(string error)
        {
            return new ValueResponse<TValue>(default(TValue), ResponseCode.GeneralError, ResponseType.Response, new Message(error));
        }

        public static new IValueResponse<TValue> AccessDenied(string error)
        {
            return new ValueResponse<TValue>(default(TValue), ResponseCode.AccessDenied, ResponseType.Response, new Message(error));
        }
        #endregion
    }
}
