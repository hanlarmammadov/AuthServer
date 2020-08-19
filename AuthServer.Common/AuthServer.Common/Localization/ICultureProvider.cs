using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.Common.Localization
{
    public interface ICultureProvider
    {
        string CurrentCulture { get; }
        void SetCurrentCulture(string cultureCode);
        string Localize(string str);
        string Localize(string str, string cultureCode);
    }
}
