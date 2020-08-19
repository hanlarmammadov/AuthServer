
namespace AuthServer.Common.Patterns
{
    public class NullCommand : ICommand
    {
        public bool IsFaulted => false;

        public void Execute()
        {

        }
    }
}
