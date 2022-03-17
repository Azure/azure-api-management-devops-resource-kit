using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiVersionSet;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.AuthorizationServer;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.TagApi;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions
{
    public interface IMasterTemplateExtractor
    {
        Task<Template> CreateMasterTemplateParameterValues(List<string> apisToExtract, ExtractorParameters extractorParameters,
            Dictionary<string, object> apiLoggerId,
            Dictionary<string, string> loggerResourceIds,
            Dictionary<string, BackendApiParameters> backendParams,
             List<TemplateResource> propertyResources);

        Template GenerateLinkedMasterTemplate(
            Template<ApiTemplateResources> apiTemplate,
            Template<PolicyTemplateResources> globalServicePolicyTemplate,
            Template<ApiVersionSetTemplateResources> apiVersionSetTemplate,
            Template<ProductTemplateResources> productsTemplate,
            Template<ProductApiTemplateResources> productAPIsTemplate,
            Template<TagApiTemplateResources> apiTagsTemplate,
            Template loggersTemplate,
            Template backendsTemplate,
            Template<AuthorizationServerTemplateResources> authorizationServersTemplate,
            Template namedValuesTemplate,
            Template<TagTemplateResources> tagTemplate,
            FileNames fileNames,
            ExtractorParameters extractorParameters);
    }
}
