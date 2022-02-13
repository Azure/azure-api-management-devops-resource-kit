using apimtemplate.Common.Templates.Abstractions;
using apimtemplate.Extractor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace apimtemplate.Extractor.EntityExtractors.Abstractions
{
    public interface IProductExtractor
    {
        Task<Template> GenerateProductsARMTemplateAsync(string apimname, string resourceGroup, string singleApiName, List<TemplateResource> apiTemplateResources, string fileFolder, ExtractorParameters extractorParameters);
    }
}
