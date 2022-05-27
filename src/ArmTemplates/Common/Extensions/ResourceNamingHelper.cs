// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Text.RegularExpressions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions
{
    public static class ResourceNamingHelper
    {
        static readonly Regex ExcludeOtherFromAlphaNumericsAndHyphensRegex = new Regex("[^a-zA-Z0-9-]");

        public static string GenerateValidResourceNameFromDisplayName(string displayName)
        {
            if (displayName.IsNullOrEmpty())
            {
                return null;
            }

            var trimmedDisplayName = displayName.Trim().Replace(" ", "-");
            var resourceName = ExcludeOtherFromAlphaNumericsAndHyphensRegex.Replace(trimmedDisplayName, string.Empty);
            return resourceName;
        }
    }
}
