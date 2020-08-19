using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AuthServer.SecurityTokens.Api.UnitTests
{
    public static class AttrHelper
    {
        public static bool ClassHasAttr<TClass, TAttribute>() where TAttribute : Attribute
        {
            return (typeof(TClass).GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute) != null;
        }
        public static bool MethodHasAttr<TClass, TAttribute>(string methodName) where TAttribute : Attribute
        {
            return (typeof(TClass).GetMethod(methodName).GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute) != null;
        }
        public static TValue GetClassAttrValue<TClass, TAttribute, TValue>(Func<TAttribute, TValue> selector) where TAttribute : Attribute
        {
            var attribute = typeof(TClass).GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault();
            return TryExtractValue(attribute, selector);
        }
        public static TValue GetMethodAttrValue<TClass, TAttribute, TValue>(string methodName, Func<TAttribute, TValue> selector) where TAttribute : Attribute
        {
            var attribute = typeof(TClass).GetMethod(methodName).GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault();
            return TryExtractValue(attribute, selector);
        }
        private static TValue TryExtractValue<TAttribute, TValue>(Object attr, Func<TAttribute, TValue> selector)
        {
            TValue result = default;
            if (attr is TAttribute)
                result = selector((TAttribute)attr);
            return result;
        }
    }

  
}
