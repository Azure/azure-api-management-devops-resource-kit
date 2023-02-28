// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------


using System;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models
{
    public class SKUTypes
    {
        public const string Basic = "Basic";
        public const string Consumption = "Consumption";
        public const string Developer = "Developer";
        public const string Isolated = "Isolated";
        public const string Premium = "Premium";
        public const string Standard = "Standard";

        public static bool IsConsumption(string skuValue)
        {
            if (string.IsNullOrEmpty(skuValue))
            {
                return false;
            }

            return skuValue.Equals(Consumption, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
