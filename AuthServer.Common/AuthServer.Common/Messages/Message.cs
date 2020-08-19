using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.Common.Messages
{
    public class Message
    {
        public string Field { get; set; }
        public string Text { get; set; }
        public string Code { get; set; }

        public Message(string text, string field, string code)
        {
            Text = text;
            Field = field;
            Code = code;
        }

        public Message(string text, string field)
            : this(text, field, null)
        {
            Field = field;
            Text = text;
        }

        public Message(string text)
            : this(text, null)
        {

        }

        public Message()
        {

        }
    }
}
