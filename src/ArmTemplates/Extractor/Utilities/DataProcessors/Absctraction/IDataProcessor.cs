// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction
{
    public interface IDataProcessor<TTemplateResource>
    {
        IDictionary<string, string> OverrideRules { get; }

        void ProcessData(List<TTemplateResource> templates, ExtractorParameters extractorParameters);

        void OverrideName(TTemplateResource template);
    }
}
