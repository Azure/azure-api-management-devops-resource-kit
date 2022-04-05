using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions
{
    public interface IBackendExtractor
    {
        Task<Tuple<Template, Dictionary<string, BackendApiParameters>>> GenerateBackendsARMTemplateAsync(
            string singleApiName, 
            List<PolicyTemplateResource> policyTemplateResources,
            List<TemplateResource> propertyResources, 
            ExtractorParameters extractorParameters,
            string baseFilesGenerationDirectory);

        Task<bool> IsNamedValueUsedInBackends(
            string apimname,
            string resourceGroup,
            string singleApiName,
            List<PolicyTemplateResource> apiPolicies,
            ExtractorParameters extractorParameters,
            string propertyName,
            string propertyDisplayName,
            string baseFilesGenerationDirectory);
    }
}
