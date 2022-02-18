using System;
using Newtonsoft.Json.Linq;

namespace apimtemplate.Extractor.Utilities
{
    static class StringExtensions
    {
        internal static bool TryParseJson(this string str, out JToken result)
        {
            try
            {
                result = JToken.Parse(str);
                return true;
            }
            catch (Exception)
            {
                
            }

            result = null;
            return false;
        }
    }
}