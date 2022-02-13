using apimtemplate.Common.Templates.Abstractions;
using apimtemplate.Extractor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace apimtemplate.Extractor.EntityExtractors.Abstractions
{
    public interface ILoggerExtractor
    {
        Task<Template> GenerateLoggerTemplateAsync(ExtractorParameters extractorParameters, string singleApiName, List<TemplateResource> apiTemplateResources, Dictionary<string, object> apiLoggerId);
    }
}
