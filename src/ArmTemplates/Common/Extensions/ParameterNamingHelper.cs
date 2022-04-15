// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions
{
    static class ParameterNamingHelper
    {
        static readonly Regex ExcludeOtherFromLettersAndDigitsRegex = new Regex("[^a-zA-Z0-9]");

        public static string GetSubstringBetweenTwoCharacters(char left, char right, string fullString)
        {
            var regex = new Regex($"(?<={left})(.*?)(?={right})");
            var matchString = regex.Match(fullString);
            return matchString.Captures.FirstOrDefault()?.Value ?? string.Empty;
        }

        public static string GenerateValidParameterName(string apiName, string prefix)
        {
            if (string.IsNullOrEmpty(apiName))
            {
                return string.Empty;
            }

            var validApiName = ExcludeOtherFromLettersAndDigitsRegex.Replace(apiName, string.Empty);

            if (string.IsNullOrEmpty(validApiName))
            {
                return string.Empty;
            }

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
