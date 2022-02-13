using apimtemplate.Common.Templates.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace apimtemplate.Extractor.EntityExtractors.Abstractions
{
    public interface IAuthorizationServerExtractor
    {
        Task<Template> GenerateAuthorizationServersARMTemplateAsync(string apimname, string resourceGroup, string singleApiName, List<TemplateResource> apiTemplateResources);
    }
}
