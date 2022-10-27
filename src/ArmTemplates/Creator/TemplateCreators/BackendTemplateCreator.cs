﻿// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class BackendTemplateCreator : IBackendTemplateCreator
    {
        readonly ITemplateBuilder templateBuilder;

        public BackendTemplateCreator(ITemplateBuilder templateBuilder)
        {
            this.templateBuilder = templateBuilder;
        }

        public Template CreateBackendTemplate(CreatorParameters creatorConfig)
        {
            // create empty template
            Template backendTemplate = this.templateBuilder.GenerateEmptyTemplate().Build();

            // add parameters
            backendTemplate.Parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { ParameterNames.ApimServiceName, new TemplateParameterProperties(){ Type = "string" } }
            };

            List<TemplateResource> resources = new List<TemplateResource>();
            foreach (BackendTemplateProperties backendTemplatePropeties in creatorConfig.Backends)
            {
                if (!string.IsNullOrWhiteSpace(backendTemplatePropeties.Properties?.ServiceFabricCluster?.ClientCertificateId)) {
                    var clientCertId = backendTemplatePropeties.Properties.ServiceFabricCluster.ClientCertificateId;
                    backendTemplatePropeties.Properties.ServiceFabricCluster.ClientCertificateId = $"[resourceId('{ResourceTypeConstants.Certificate}', parameters('{ParameterNames.ApimServiceName}'), '{clientCertId}')]";
                }
                // create backend resource with properties
                BackendTemplateResource backendTemplateResource = new BackendTemplateResource()
                {
                    Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{backendTemplatePropeties.Title}')]",
                    Type = ResourceTypeConstants.Backend,
                    ApiVersion = GlobalConstants.ApiVersion,
                    Properties = backendTemplatePropeties,
                    DependsOn = new string[] { }
                };
                resources.Add(backendTemplateResource);
            }

            backendTemplate.Resources = resources.ToArray();
            return backendTemplate;
        }
    }
}
