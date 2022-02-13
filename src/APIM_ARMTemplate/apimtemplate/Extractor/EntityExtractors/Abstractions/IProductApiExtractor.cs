using apimtemplate.Common.Templates.Abstractions;
using apimtemplate.Extractor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace apimtemplate.Extractor.EntityExtractors.Abstractions
{
    public interface IProductApiExtractor
    {
        Task<Template> GenerateAPIProductsARMTemplateAsync(string singleApiName, List<string> multipleApiNames, ExtractorParameters extractorParameters);
    }
}
