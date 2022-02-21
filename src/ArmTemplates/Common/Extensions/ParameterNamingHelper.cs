using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions
{
    static class ParameterNamingHelper
    {
        static readonly Regex ExcludeOtherFromLettersAndDigitsRegex = new Regex("[^a-zA-Z0-9]");

        public static string GenerateValidParameterName(string apiName, string prefix)
        {
            var validApiName = ExcludeOtherFromLettersAndDigitsRegex.Replace(apiName, string.Empty);

            if (char.IsDigit(validApiName.First()))
            {
                return prefix + validApiName;
            }
            else
            {
                return validApiName;
            }
        }
    }
}
