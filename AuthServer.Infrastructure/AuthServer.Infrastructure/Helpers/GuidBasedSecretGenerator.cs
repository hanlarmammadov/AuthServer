using AuthServer.Infrastructure.Helpers.Interfaces; 
using System; 

namespace AuthServer.Infrastructure.Helpers
{
    public class GuidBasedSecretGenerator : ISecretGenerator
    {
        private readonly int _length;

        public GuidBasedSecretGenerator(int length = 32)
        { 
            if (length > 32)
                throw new ArgumentException("Length should be less than 32");
            this._length = length;
        }

        public string Generate()
        {
            return Guid.NewGuid().ToString("n").Substring(0, _length); 
        }
    }
}
