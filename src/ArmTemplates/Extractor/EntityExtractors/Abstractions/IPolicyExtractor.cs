using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions
{
    public interface IPolicyExtractor
    {
        Task<Template> GenerateGlobalServicePolicyTemplateAsync(ExtractorParameters extractorParameters, string baseFilesGenerationDirectory);
    }
}
