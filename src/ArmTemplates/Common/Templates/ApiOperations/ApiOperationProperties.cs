// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiOperations
{
    public class ApiOperationProperties
    {
        public string DisplayName { get; set; }

        public string Method { get; set; }

        public string UrlTemplate { get; set; }

        public string Description { get; set; }

        public ApiOperationTemplateParameter[] TemplateParameters { get; set; }

        public ApiOperationRequest Request { get; set; }

        public ApiOperationResponse[] Responses { get; set; }
    }
}
