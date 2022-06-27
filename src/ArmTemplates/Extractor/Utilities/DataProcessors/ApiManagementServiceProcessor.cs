// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiManagementService;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors
{
    public class ApiManagementServiceProcessor : IApiManagementServiceProcessor
    {
        public void ProcessData(List<ApiManagementServiceResource> templates, ExtractorParameters extractorParameters)
        {
            foreach (var template in templates)
            {
                template.OriginalName = template.Name;
            }
        }

        public void ProcessSingleInstanceData(ApiManagementServiceResource template, ExtractorParameters extractorParameters)
        {
            template.OriginalName = template.Name;
        }
    }
}
