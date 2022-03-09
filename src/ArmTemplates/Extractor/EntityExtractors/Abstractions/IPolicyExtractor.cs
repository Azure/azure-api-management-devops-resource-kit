using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions
{
    public interface IPolicyExtractor
    {
        Task<PolicyTemplateResource> GenerateProductPolicyTemplateAsync(
            ExtractorParameters extractorParameters,
            string productName,
            string[] productResourceId,
            string baseFilesGenerationDirectory);

        Task<Template> GenerateGlobalServicePolicyTemplateAsync(ExtractorParameters extractorParameters, string baseFilesGenerationDirectory);
    }
}
