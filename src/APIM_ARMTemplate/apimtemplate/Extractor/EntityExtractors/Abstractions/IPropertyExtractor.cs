using apimtemplate.Common.Templates.Abstractions;
using apimtemplate.Extractor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace apimtemplate.Extractor.EntityExtractors.Abstractions
{
    public interface IPropertyExtractor
    {
        Task<Template> GenerateNamedValuesTemplateAsync(string singleApiName, List<TemplateResource> apiTemplateResources, ExtractorParameters extractorParameters, IBackendExtractor backendExtractor, List<TemplateResource> loggerTemplateResources);
    }
}
