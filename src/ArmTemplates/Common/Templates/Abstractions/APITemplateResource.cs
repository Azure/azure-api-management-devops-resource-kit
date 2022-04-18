// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions
{
    public class APITemplateResource : TemplateResource
    {
        public APITemplateProperties Properties { get; set; }

        public APITemplateSubResource[] Resources { get; set; }
    }   
}