using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions
{
    public interface IApiExtractor
    {
        Task<JToken[]> GetAllApiObjsAsync(string apiManagementName, string resourceGroupName);

        Task<string> GetServiceDiagnosticsAsync(string apiManagementName, string resourceGroupName);

        Task<string> GetApiDiagnosticsAsync(string apiManagementName, string resourceGroupName, string apiName);

        Task<Template<ApiTemplateResources>> GenerateApiTemplateAsync(string singleApiName, List<string> multipleApiNames, string baseFilesGenerationDirectory, ExtractorParameters extractorParameters);

        Task<ApiTemplateResources> GenerateSingleApiTemplateResourcesAsync(string singleApiName, string baseFilesGenerationDirectory, ExtractorParameters extractorParameters);

        Task<ApiTemplateResources> GetApiRelatedTemplateResourcesAsync(string apiName, string baseFilesGenerationDirectory, ExtractorParameters extractorParameters);
    }
}
