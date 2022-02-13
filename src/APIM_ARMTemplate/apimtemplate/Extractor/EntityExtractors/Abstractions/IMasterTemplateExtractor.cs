using apimtemplate.Common.FileHandlers;
using apimtemplate.Common.TemplateModels;
using apimtemplate.Common.Templates.Abstractions;
using apimtemplate.Extractor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace apimtemplate.Extractor.EntityExtractors.Abstractions
{
    public interface IMasterTemplateExtractor
    {
        Task<Template> CreateMasterTemplateParameterValues(List<string> apisToExtract, ExtractorParameters extractorParameters,
            Dictionary<string, object> apiLoggerId,
            Dictionary<string, string> loggerResourceIds,
            Dictionary<string, BackendApiParameters> backendParams,
             List<TemplateResource> propertyResources);

        Template GenerateLinkedMasterTemplate(Template apiTemplate,
            Template globalServicePolicyTemplate,
            Template apiVersionSetTemplate,
            Template productsTemplate,
            Template productAPIsTemplate,
            Template apiTagsTemplate,
            Template loggersTemplate,
            Template backendsTemplate,
            Template authorizationServersTemplate,
            Template namedValuesTemplate,
            Template tagTemplate,
            FileNames fileNames,
            string apiFileName,
            ExtractorParameters extractorParameters);
    }
}
