using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions
{
    public interface ILoggerExtractor
    {
        Task<Template> GenerateLoggerTemplateAsync(ExtractorParameters extractorParameters, string singleApiName, List<TemplateResource> apiTemplateResources, Dictionary<string, object> apiLoggerId);
    }
}
