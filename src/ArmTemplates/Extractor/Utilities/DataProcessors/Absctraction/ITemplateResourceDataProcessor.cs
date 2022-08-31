// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction
{
    public interface ITemplateResourceDataProcessor<T> where T : TemplateResource
    {
        void ProcessData(List<T> templateResources);
        void ProcessSingleData(T templateResource);
    }
}
