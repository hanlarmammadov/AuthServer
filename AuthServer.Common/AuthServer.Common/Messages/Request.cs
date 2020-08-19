using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.Common.Messages
{
    public class Request<T>
    {
        public T Data { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string Order { get; set; }
        public bool IsDesc { get; set; }
        public string Context { get; set; }
        public string Captcha { get; set; }

        #region Ctors

        /// <summary>
        /// For automatic data binding
        /// </summary>
        public Request() { }

        /// <summary>
        /// For basic CRUD-like operations
        /// </summary>
        /// <param name="data"></param>
        /// <param name="context"></param>
        public Request(T data, string context = null)
        {
            Data = data;
            Context = context;
        }

        /// <summary>
        /// For list retriving operations with adv search
        /// </summary>
        /// <param name="data">Filter object</param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="order"></param>
        /// <param name="isDesc"></param>
        /// <param name="context">Used in some operations when additional operation context information is required</param>
        public Request(T data, int pageNumber, int pageSize, string order, bool isDesc, string context = null)
        {
            Data = data;
            PageNumber = pageNumber;
            PageSize = pageSize;
            Order = order;
            IsDesc = isDesc;
            Context = context;
        }
        #endregion
    }
}
