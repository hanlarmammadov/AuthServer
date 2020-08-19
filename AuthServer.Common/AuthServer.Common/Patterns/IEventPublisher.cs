
namespace AuthServer.Common.Patterns
{
    public interface IEventPublisher<TEvent>
    {
        IEventPublisher<TEvent> AddSubsciber(IEventSubscriber<TEvent> subscriber);
    }
}

