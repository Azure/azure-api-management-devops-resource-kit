using apimtemplate.Common.Templates.Abstractions;
using System.Threading.Tasks;

namespace apimtemplate.Extractor.EntityExtractors.Abstractions
{
    public interface IPolicyExtractor
    {
        Task<Template> GenerateGlobalServicePolicyTemplateAsync(string apimname, string resourceGroup, string policyXMLBaseUrl, string policyXMLSasToken, string fileFolder);
    }
}
