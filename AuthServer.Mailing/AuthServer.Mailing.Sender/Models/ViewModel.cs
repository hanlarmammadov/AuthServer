using System;
using System.Collections.Generic;

namespace AuthServer.Mailing.Sender.Models
{
    public class ViewModel   
    { 
        public readonly Dictionary<string, string> Dictionary;
         
        public ViewModel()
        {
            this.Dictionary = new Dictionary<string, string>();
        }

        public ViewModel Add(string key, string value)
        {
            if (Dictionary.ContainsKey(key))
                throw new ArgumentException($"Model already contains this key: {key}");
            Dictionary.Add(key, value);
            return this;
        }

        public bool Exists(string key)
        {
            return Dictionary.ContainsKey(key);
        }

        public string Get(string key)
        {
            Dictionary.TryGetValue(key, out string value);
            return value;
        }

    }
}
