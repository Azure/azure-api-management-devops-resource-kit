// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class PolicyTemplateCreator
    {
        readonly ITemplateBuilder templateBuilder;
        FileReader fileReader;

        public PolicyTemplateCreator(
            FileReader fileReader,
            ITemplateBuilder templateBuilder)
        {
            this.fileReader = fileReader;
            this.templateBuilder = templateBuilder;
        }

        public Template CreateGlobalServicePolicyTemplate(CreatorConfig creatorConfig)
        {
            // create empty template
            Template policyTemplate = this.templateBuilder.GenerateEmptyTemplate().Build();

            // add parameters
            policyTemplate.Parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { ParameterNames.ApimServiceName, new TemplateParameterProperties(){ Type = "string" } }
            };

            List<TemplateResource> resources = new List<TemplateResource>();

            // create global service policy resource with properties
            string globalServicePolicy = creatorConfig.policy;
            Uri uriResult;
            bool isUrl = Uri.TryCreate(globalServicePolicy, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            // create policy resource with properties
            PolicyTemplateResource policyTemplateResource = new PolicyTemplateResource()
            {
                Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/policy')]",
                Type = ResourceTypeConstants.GlobalServicePolicy,
                ApiVersion = GlobalConstants.ApiVersion,
                Properties = new PolicyTemplateProperties()
                {
                    // if policy is a url inline the url, if it is a local file inline the file contents
                    Format = isUrl ? "rawxml-link" : "rawxml",
                    PolicyContent = isUrl ? globalServicePolicy : this.fileReader.RetrieveLocalFileContents(globalServicePolicy)
                },
                DependsOn = new string[] { }
            };
            resources.Add(policyTemplateResource);

            policyTemplate.Resources = resources.ToArray();
            return policyTemplate;
        }

        public PolicyTemplateResource CreateAPIPolicyTemplateResource(APIConfig api, string[] dependsOn)
        {
            Uri uriResult;
            bool isUrl = Uri.TryCreate(api.policy, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            // create policy resource with properties
            PolicyTemplateResource policyTemplateResource = new PolicyTemplateResource()
            {
                Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.name}/policy')]",
                Type = ResourceTypeConstants.APIPolicy,
                ApiVersion = GlobalConstants.ApiVersion,
                Properties = new PolicyTemplateProperties()
                {
                    // if policy is a url inline the url, if it is a local file inline the file contents
                    Format = isUrl ? "rawxml-link" : "rawxml",
                    PolicyContent = isUrl ? api.policy : this.fileReader.RetrieveLocalFileContents(api.policy)
                },
                DependsOn = dependsOn
            };
            return policyTemplateResource;
        }

        public PolicyTemplateResource CreateProductPolicyTemplateResource(ProductConfig product, string[] dependsOn)
        {
            if (string.IsNullOrEmpty(product.Name))
            {
                product.Name = product.DisplayName;
            }

            Uri uriResult;
            bool isUrl = Uri.TryCreate(product.policy, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            // create policy resource with properties
            PolicyTemplateResource policyTemplateResource = new PolicyTemplateResource()
            {
                Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{product.Name}/policy')]",
                Type = ResourceTypeConstants.ProductPolicy,
                ApiVersion = GlobalConstants.ApiVersion,
                Properties = new PolicyTemplateProperties()
                {
                    // if policy is a url inline the url, if it is a local file inline the file contents
                    Format = isUrl ? "rawxml-link" : "rawxml",
                    PolicyContent = isUrl ? product.policy : this.fileReader.RetrieveLocalFileContents(product.policy)
                },
                DependsOn = dependsOn
            };
            return policyTemplateResource;
        }

        public PolicyTemplateResource CreateOperationPolicyTemplateResource(KeyValuePair<string, OperationsConfig> policyPair, string apiName, string[] dependsOn)
        {
            Uri uriResult;
            bool isUrl = Uri.TryCreate(policyPair.Value.policy, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            // create policy resource with properties
            PolicyTemplateResource policyTemplateResource = new PolicyTemplateResource()
            {
                Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{policyPair.Key}/policy')]",
                Type = ResourceTypeConstants.APIOperationPolicy,
                ApiVersion = GlobalConstants.ApiVersion,
                Properties = new PolicyTemplateProperties()
                {
                    // if policy is a url inline the url, if it is a local file inline the file contents
                    Format = isUrl ? "rawxml-link" : "rawxml",
                    PolicyContent = isUrl ? policyPair.Value.policy : this.fileReader.RetrieveLocalFileContents(policyPair.Value.policy)
                },
                DependsOn = dependsOn
            };
            return policyTemplateResource;
        }

        public List<PolicyTemplateResource> CreateOperationPolicyTemplateResources(APIConfig api, string[] dependsOn)
        {
            // create a policy resource for each policy listed in the config file and its associated provided xml file
            List<PolicyTemplateResource> policyTemplateResources = new List<PolicyTemplateResource>();
            foreach (KeyValuePair<string, OperationsConfig> pair in api.operations)
            {
                policyTemplateResources.Add(this.CreateOperationPolicyTemplateResource(pair, api.name, dependsOn));
            }
            return policyTemplateResources;
        }
    }
}
