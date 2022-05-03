// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators.Abstractions
{
    public interface IMasterTemplateCreator
    {
        string GetDependsOnPreviousApiVersion(ApiConfig api, IDictionary<string, string[]> apiVersions);

        Task<bool> DetermineIfAPIDependsOnLoggerAsync(ApiConfig api, FileReader fileReader);

        Task<bool> DetermineIfAPIDependsOnBackendAsync(ApiConfig api, FileReader fileReader);

        Template CreateMasterTemplateParameterValues(CreatorParameters creatorConfig);

        Template CreateLinkedMasterTemplate(CreatorParameters creatorConfig,
            Template globalServicePolicyTemplate,
            Template apiVersionSetTemplate,
            Template productsTemplate,
            Template productAPIsTemplate,
            Template propertyTemplate,
            Template loggersTemplate,
            Template backendsTemplate,
            Template authorizationServersTemplate,
            Template tagTemplate,
            List<LinkedMasterTemplateAPIInformation> apiInformation,
            FileNames fileNames,
            string apimServiceName);
    }
}
