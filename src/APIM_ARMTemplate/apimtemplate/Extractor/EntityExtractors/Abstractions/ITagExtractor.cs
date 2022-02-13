using apimtemplate.Common.Templates.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace apimtemplate.Extractor.EntityExtractors.Abstractions
{
    public interface ITagExtractor
    {
        Task<Template> GenerateTagsTemplateAsync(string apimname, string resourceGroup, string singleApiName, List<TemplateResource> apiTemplateResources, List<TemplateResource> productTemplateResources, string policyXMLBaseUrl, string policyXMLSasToken);
    }
}
