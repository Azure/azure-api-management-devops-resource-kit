using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions
{
    public interface IPolicyExtractor
    {
        Task<Template> GenerateGlobalServicePolicyTemplateAsync(string apimname, string resourceGroup, string policyXMLBaseUrl, string policyXMLSasToken, string fileFolder);
    }
}
