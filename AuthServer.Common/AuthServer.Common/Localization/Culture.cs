using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.Common.Localization
{
    //https://en.wikipedia.org/wiki/ISO_3166-1 
    //https://msdn.microsoft.com/en-us/library/hh441729.aspx

    public class Culture
    {
        public string Code { get; set; }
        public string Language { get; set; }
        public string CountryIsoAlpha3Code { get; set; } 
    }
}
