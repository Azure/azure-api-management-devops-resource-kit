// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors
{
    public class TemplateResourceDataProcessor<T>: ITemplateResourceDataProcessor<T> where T : TemplateResource
    {
        public void ProcessData(List<T> templateResources)
        {
            if (templateResources.IsNullOrEmpty())
            {
                return;
            }

            foreach (var templateResource in templateResources)
            {
                this.ProcessSingleData(templateResource);
            }
        }

        public void ProcessSingleData(T templateResource)
        {
            if (templateResource == null)
            {
                return;
            }
            
            templateResource.OriginalName = templateResource.Name;
        }
    }
}
