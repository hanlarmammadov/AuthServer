using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Common.Localization
{
    public class StubCultureProvider : ICultureProvider
    {
        public string CurrentCulture { get; private set; }

        public StubCultureProvider()
        {
            CurrentCulture = "en-US";
        }

        public string Localize(string str)
        {
            return str;
        }

        public string Localize(string str, string cultureCode)
        {
            return str;
        }

        public void SetCurrentCulture(string cultureCode)
        {
            CurrentCulture = cultureCode;
        }
    }
}
