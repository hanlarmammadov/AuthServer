
namespace AuthServer.Common.Patterns
{
    public interface ICommand
    {
        void Execute();
        bool IsFaulted { get; }
    }
}
