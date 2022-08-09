// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Groups;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction
{
    public interface IGroupDataProcessor
    {
        void ProcessDataProductLinkedGroups(List<GroupTemplateResource> templates, ExtractorParameters extractorParameters);

        void ProcessDataAllGroups(List<GroupTemplateResource> templates, ExtractorParameters extractorParameters);
    }
}
