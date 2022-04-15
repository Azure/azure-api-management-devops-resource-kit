// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions
{
    public abstract class TemplateResourcesBase
    {
        protected TemplateResource[] ConcatenateTemplateResourcesCollections(params TemplateResource[][] templateResourcesCollections)
        {
            var allTemplateResources = new List<TemplateResource>();

            foreach (var templateResourceCollection in templateResourcesCollections)
            {
                if (!templateResourceCollection.IsNullOrEmpty())
                {
                    foreach (var templateResource in templateResourceCollection)
                    {
                        if (templateResource is not null)
                        {
                            allTemplateResources.Add(templateResource);
                        }
                    }
                }
            }

            return allTemplateResources.ToArray();
        }
    }
}
