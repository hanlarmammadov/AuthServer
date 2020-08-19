using System.Threading.Tasks;

namespace AuthServer.Common.Patterns
{
    public interface IEventSubscriber<TEvent>
    {
        Task HandleEvent(TEvent evnt);
    } 
}
