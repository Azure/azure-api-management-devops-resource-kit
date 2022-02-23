using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions
{
    public interface ITagApiExtractor
    {
        Task<Template> GenerateAPITagsARMTemplateAsync(string singleApiName, List<string> multipleApiNames, ExtractorParameters extractorParameters);
    }
}
