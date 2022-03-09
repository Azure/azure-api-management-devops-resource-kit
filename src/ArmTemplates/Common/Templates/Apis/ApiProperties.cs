// --------------------------------------------------------------------------
//  <copyright file="ServiceApisProperties.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis
{
    public class ApiProperties
    {
        public string DisplayName { get; set; }

        public string ApiRevision { get; set; }

        public string Description { get; set; }

        public bool SubscriptionRequired { get; set; }

        public string ServiceUrl { get; set; }

        public string Path { get; set; }

        public string[] Protocols { get; set; }

        public bool IsCurrent { get; set; }
    }
}
