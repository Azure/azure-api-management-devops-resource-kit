// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities
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