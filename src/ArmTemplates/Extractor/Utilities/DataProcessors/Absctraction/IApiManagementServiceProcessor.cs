// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiManagementService;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction
{
    public interface IApiManagementServiceProcessor
    {
        void ProcessData(List<ApiManagementServiceResource> templates, ExtractorParameters extractorParameters);

        void ProcessSingleInstanceData(ApiManagementServiceResource template, ExtractorParameters extractorParameters);
    }
}
