using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System.Linq;
using System;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    static class ExtractorUtils
    {
        public static List<TemplateResource> removeResourceType(string resourceType, List<TemplateResource> resources)
        {
            List<TemplateResource> newResourcesList = new List<TemplateResource>();
            foreach (TemplateResource resource in resources)
            {
                if (!resource.type.Equals(resourceType))
                {
                    newResourcesList.Add(resource);
                }
            }
            return newResourcesList;
        }
    }
}