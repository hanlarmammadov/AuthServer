using System.Collections.Generic;

namespace AuthServer.Common.Messages
{
    public class RestfulErrorResponse
    {
        private IEnumerable<Message> _messages;

        private RestfulErrorResponse(IEnumerable<Message> messages)
        {
            _messages = messages;
        }
        private RestfulErrorResponse(Message message)
        {
           // _messages = messages;
        }

        public IEnumerable<Message> Messages;
    }
}
