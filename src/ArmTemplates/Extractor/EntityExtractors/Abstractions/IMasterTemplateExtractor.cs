// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiVersionSet;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.AuthorizationServer;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Master;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.TagApi;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions
{
    public interface IMasterTemplateExtractor
    {
        Template<MasterTemplateResources> GenerateLinkedMasterTemplate(
           ExtractorParameters extractorParameters,
           ApiTemplateResources apiTemplateResources = null,
           PolicyTemplateResources policyTemplateResources = null,
           ApiVersionSetTemplateResources apiVersionSetTemplateResources = null,
           ProductTemplateResources productsTemplateResources = null,
           ProductApiTemplateResources productAPIsTemplateResources = null,
           TagApiTemplateResources apiTagsTemplateResources = null,
           LoggerTemplateResources loggersTemplateResources = null,
           BackendTemplateResources backendsTemplateResources = null,
           AuthorizationServerTemplateResources authorizationServersTemplateResources = null,
           NamedValuesResources namedValuesTemplateResources = null,
           TagTemplateResources tagTemplateResources = null);
    }
}
