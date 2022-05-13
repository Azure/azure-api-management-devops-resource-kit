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
        IDictionary<string, string> OverrideRules { get; set; }
        void ProcessData(List<GroupTemplateResource> template, ExtractorParameters extractorParameters);
        void OverrideName(GroupTemplateResource template);

    }
}
