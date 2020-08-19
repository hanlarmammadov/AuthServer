using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.Common.Messages
{
    public class ListResponse<TListElem> : Response, IListResponse<TListElem>
    {
        public IEnumerable<TListElem> Value { get; set; }
        public long TotalItems { get; set; }

        #region Ctors

        /// <summary>
        /// Default constructor for generating ValueResponse during such processes as deserialization etc. You should not use it
        /// </summary>
        public ListResponse()
        {

        }

        protected ListResponse(IEnumerable<TListElem> value, long totalItems, ResponseCode code, ResponseType type, Message message)
            : this(value, totalItems, code, type, new Message[1] { message })
        {

        }

        protected ListResponse(IEnumerable<TListElem> value, long totalItems, ResponseCode code, ResponseType type, IEnumerable<Message> messages)
            : base(code, type, messages)
        {
            this.Value = value;
            this.TotalItems = totalItems;
        } 
        #endregion

        #region Statics

        public static IListResponse<TListElem> Success(IEnumerable<TListElem> value, long totalItems, string message = null)
        {
            return new ListResponse<TListElem>(value, totalItems, ResponseCode.Success, ResponseType.Response, new Message(message));
        }

        public static new IListResponse<TListElem> ValidationError(IEnumerable<Message> errors)
        {
            return new ListResponse<TListElem>(null, 0, ResponseCode.ValidationError, ResponseType.Response, errors);
        }

        public static new IListResponse<TListElem> GeneralError(string error)
        {
            return new ListResponse<TListElem>(null, 0, ResponseCode.GeneralError, ResponseType.Response, new Message(error));
        }

        public static new IListResponse<TListElem> AccessDenied(string error)
        {
            return new ListResponse<TListElem>(null, 0, ResponseCode.AccessDenied, ResponseType.Response, new Message(error));
        }
        #endregion
    }
}
