// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend
{
    /// <summary>
    /// Backend Parameters for a single API
    /// </summary>
    public class BackendApiParameters
    {
        public string ResourceId { get; set; }
        public string Url { get; set; }
        public string Protocol { get; set; }
    }

}
