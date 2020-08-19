using AuthServer.Common.Patterns;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuthServer.Infrastructure.Eventing
{
    public abstract class EventPublisher<TEvent> : IEventPublisher<TEvent>
    {
        protected List<IEventSubscriber<TEvent>> _subscribers;

        public EventPublisher()
        {
            _subscribers = new List<IEventSubscriber<TEvent>>();
        }

        protected async Task Publish(TEvent evnt)
        {
            foreach (var subscriber in _subscribers)
                await subscriber.HandleEvent(evnt);
        }

        public IEventPublisher<TEvent> AddSubsciber(IEventSubscriber<TEvent> subscriber)
        {
            _subscribers.Add(subscriber);
            return this;
        }
    }
}
