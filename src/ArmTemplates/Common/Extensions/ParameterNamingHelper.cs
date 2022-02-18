using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions
{
    internal static class ParameterNamingHelper
    {
        private static readonly Regex _excludeOtherFromLettersAndDigitsRegex = new Regex("[^a-zA-Z0-9]");

        public static string GenerateValidParameterName(string apiName, string prefix)
        {
            var validApiName = _excludeOtherFromLettersAndDigitsRegex.Replace(apiName, string.Empty);

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
