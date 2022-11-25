// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction
{
    public interface INamedValuesDataProcessor
    {
        void ProcessData(List<NamedValueTemplateResource> templateResources);
        void ProcessSingleData(NamedValueTemplateResource templateResource);
    }
}
