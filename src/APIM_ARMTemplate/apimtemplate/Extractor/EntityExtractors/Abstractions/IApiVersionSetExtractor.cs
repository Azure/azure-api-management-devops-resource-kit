using apimtemplate.Common.Templates.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace apimtemplate.Extractor.EntityExtractors.Abstractions
{
    public interface IApiVersionSetExtractor
    {
        Task<Template> GenerateAPIVersionSetsARMTemplateAsync(string apimname, string resourceGroup, string singleApiName, List<TemplateResource> apiTemplateResources);
    }
}
