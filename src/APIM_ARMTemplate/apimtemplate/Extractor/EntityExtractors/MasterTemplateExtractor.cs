using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System.Linq;
using System;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class MasterTemplateExtractor : EntityExtractor
    {
        public Template GenerateLinkedMasterTemplate(Template apiTemplate,
            Template globalServicePolicyTemplate,
            Template apiVersionSetTemplate,
            Template productsTemplate,
            Template loggersTemplate,
            Template backendsTemplate,
            Template authorizationServersTemplate,
            Template namedValuesTemplate,
            Template tagTemplate,
            FileNames fileNames,
            string apiFileName,
            string linkedTemplatesUrlQueryString,
            string linkedTemplatesSasToken,
            string policyXMLBaseUrl,
            string policyXMLSasToken)
        {
            // create empty template
            Template masterTemplate = GenerateEmptyTemplate();

            // add parameters
            masterTemplate.parameters = this.CreateMasterTemplateParameters(true, linkedTemplatesSasToken, linkedTemplatesUrlQueryString, policyXMLBaseUrl, policyXMLSasToken);

            // add deployment resources that links to all resource files
            List<TemplateResource> resources = new List<TemplateResource>();

            // namedValue
            string namedValueDeploymentResourceName = "namedValuesTemplate";
            // all other deployment resources will depend on named values
            string[] dependsOnNamedValues = new string[] { };

            // api dependsOn
            List<string> apiDependsOn = new List<string>();

            if (namedValuesTemplate != null && namedValuesTemplate.resources.Count() != 0)
            {
                dependsOnNamedValues = new string[] { $"[resourceId('Microsoft.Resources/deployments', '{namedValueDeploymentResourceName}')]" };
                apiDependsOn.Add($"[resourceId('Microsoft.Resources/deployments', '{namedValueDeploymentResourceName}')]");
                string namedValuesUri = GenerateLinkedTemplateUri(linkedTemplatesUrlQueryString, linkedTemplatesSasToken, fileNames.namedValues);
                resources.Add(this.CreateLinkedMasterTemplateResourceWithPolicyToken(namedValueDeploymentResourceName, namedValuesUri, new string[] { }, policyXMLSasToken));
            }

            // globalServicePolicy
            if (globalServicePolicyTemplate != null && globalServicePolicyTemplate.resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'globalServicePolicyTemplate')]");
                string globalServicePolicyUri = GenerateLinkedTemplateUri(linkedTemplatesUrlQueryString, linkedTemplatesSasToken, fileNames.globalServicePolicy);
                resources.Add(this.CreateLinkedMasterTemplateResourceWithPolicyToken("globalServicePolicyTemplate", globalServicePolicyUri, dependsOnNamedValues, policyXMLSasToken));
            }

            // apiVersionSet
            if (apiVersionSetTemplate != null && apiVersionSetTemplate.resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'versionSetTemplate')]");
                string apiVersionSetUri = GenerateLinkedTemplateUri(linkedTemplatesUrlQueryString, linkedTemplatesSasToken, fileNames.apiVersionSets);
                resources.Add(this.CreateLinkedMasterTemplateResource("versionSetTemplate", apiVersionSetUri, dependsOnNamedValues));
            }

            // product
            if (productsTemplate != null && productsTemplate.resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'productsTemplate')]");
                string productsUri = GenerateLinkedTemplateUri(linkedTemplatesUrlQueryString, linkedTemplatesSasToken, fileNames.products);
                resources.Add(this.CreateLinkedMasterTemplateResource("productsTemplate", productsUri, dependsOnNamedValues));
            }

            if (tagTemplate != null && tagTemplate.resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'tagTemplate')]");
                string tagUri = GenerateLinkedTemplateUri(linkedTemplatesUrlQueryString, linkedTemplatesSasToken, fileNames.tags);
                resources.Add(this.CreateLinkedMasterTemplateResource("tagTemplate", tagUri, dependsOnNamedValues));
            }

            // logger
            if (loggersTemplate != null && loggersTemplate.resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'loggersTemplate')]");
                string loggersUri = GenerateLinkedTemplateUri(linkedTemplatesUrlQueryString, linkedTemplatesSasToken, fileNames.loggers);
                resources.Add(this.CreateLinkedMasterTemplateResource("loggersTemplate", loggersUri, dependsOnNamedValues));
            }

            // backend
            if (backendsTemplate != null && backendsTemplate.resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'backendsTemplate')]");
                string backendsUri = GenerateLinkedTemplateUri(linkedTemplatesUrlQueryString, linkedTemplatesSasToken, fileNames.backends);
                resources.Add(this.CreateLinkedMasterTemplateResource("backendsTemplate", backendsUri, dependsOnNamedValues));
            }

            // authorizationServer
            if (authorizationServersTemplate != null && authorizationServersTemplate.resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'authorizationServersTemplate')]");
                string authorizationServersUri = GenerateLinkedTemplateUri(linkedTemplatesUrlQueryString, linkedTemplatesSasToken, fileNames.authorizationServers);
                resources.Add(this.CreateLinkedMasterTemplateResource("authorizationServersTemplate", authorizationServersUri, dependsOnNamedValues));
            }

            // api
            if (apiTemplate != null && apiTemplate.resources.Count() != 0)
            {
                string apisUri = GenerateLinkedTemplateUri(linkedTemplatesUrlQueryString, linkedTemplatesSasToken, apiFileName);
                resources.Add(this.CreateLinkedMasterTemplateResourceWithPolicyToken("apisTemplate", apisUri, apiDependsOn.ToArray(), policyXMLSasToken));
            }

            masterTemplate.resources = resources.ToArray();
            return masterTemplate;
        }

        public MasterTemplateResource CreateLinkedMasterTemplateResourceWithPolicyToken(string name, string uriLink, string[] dependsOn, string sasToken)
        {
            if (sasToken == null)
            {
                return this.CreateLinkedMasterTemplateResource(name, uriLink, dependsOn);
            }
            else
            {
                MasterTemplateResource masterResourceTemplate = this.CreateLinkedMasterTemplateResource(name, uriLink, dependsOn);
                masterResourceTemplate.properties.parameters.Add("PolicyXMLSasToken", new TemplateParameterProperties() { value = "[parameters('PolicyXMLSasToken')]" });
                return masterResourceTemplate;
            }
        }
        public MasterTemplateResource CreateLinkedMasterTemplateResource(string name, string uriLink, string[] dependsOn)
        {
            // create deployment resource with provided arguments
            MasterTemplateResource masterTemplateResource = new MasterTemplateResource()
            {
                name = name,
                type = "Microsoft.Resources/deployments",
                apiVersion = GlobalConstants.LinkedAPIVersion,
                properties = new MasterTemplateProperties()
                {
                    mode = "Incremental",
                    templateLink = new MasterTemplateLink()
                    {
                        uri = uriLink,
                        contentVersion = "1.0.0.0"
                    },
                    parameters = new Dictionary<string, TemplateParameterProperties>
                    {
                        { "ApimServiceName", new TemplateParameterProperties(){ value = "[parameters('ApimServiceName')]" } },
                        { "PolicyXMLBaseUrl", new TemplateParameterProperties(){ value = "[parameters('PolicyXMLBaseUrl')]" } }
                    }
                },
                dependsOn = dependsOn
            };
            return masterTemplateResource;
        }

        public Dictionary<string, TemplateParameterProperties> CreateMasterTemplateParameters(bool linked, string linkedTemplatesSasToken, string linkedTemplatesUrlQueryString, string policyXMLBaseUrl, string policyXMLSasToken)
        {
            // used to create the parameter metatadata, etc (not value) for use in file with resources
            // add parameters with metatdata properties
            Dictionary<string, TemplateParameterProperties> parameters = new Dictionary<string, TemplateParameterProperties>();
            TemplateParameterProperties apimServiceNameProperties = new TemplateParameterProperties()
            {
                metadata = new TemplateParameterMetadata()
                {
                    description = "Name of the API Management"
                },
                type = "string"
            };
            parameters.Add("ApimServiceName", apimServiceNameProperties);
            // add remote location of template files for linked option
            if (linked == true)
            {
                TemplateParameterProperties linkedTemplatesBaseUrlProperties = new TemplateParameterProperties()
                {
                    metadata = new TemplateParameterMetadata()
                    {
                        description = "Base URL of the repository that contains the generated templates"
                    },
                    type = "string"
                };
                parameters.Add("LinkedTemplatesBaseUrl", linkedTemplatesBaseUrlProperties);
                // add linkedTemplatesSasToken parameter if provided and if the templates are linked
                if (linkedTemplatesSasToken != null)
                {
                    TemplateParameterProperties linkedTemplatesSasTokenProperties = new TemplateParameterProperties()
                    {
                        metadata = new TemplateParameterMetadata()
                        {
                            description = "The Shared Access Signature for the URL of the repository"
                        },
                        type = "string"
                    };
                    parameters.Add("LinkedTemplatesSasToken", linkedTemplatesSasTokenProperties);
                }
                // add linkedTemplatesUrlQueryString parameter if provided and if the templates are linked
                if (linkedTemplatesUrlQueryString != null)
                {
                    TemplateParameterProperties linkedTemplatesUrlQueryStringProperties = new TemplateParameterProperties()
                    {
                        metadata = new TemplateParameterMetadata()
                        {
                            description = "Query string for the URL of the repository"
                        },
                        type = "string"
                    };
                    parameters.Add("LinkedTemplatesUrlQueryString", linkedTemplatesUrlQueryStringProperties);
                }
            }
            if (policyXMLBaseUrl != null)
            {
                TemplateParameterProperties policyTemplateBaseUrlProperties = new TemplateParameterProperties()
                {
                    metadata = new TemplateParameterMetadata()
                    {
                        description = "Base URL of the repository that contains the generated policy files"
                    },
                    type = "string"
                };
                parameters.Add("PolicyXMLBaseUrl", policyTemplateBaseUrlProperties);
                if (policyXMLSasToken != null)
                {
                    TemplateParameterProperties policyXMLSasTokenProperties = new TemplateParameterProperties()
                    {
                        metadata = new TemplateParameterMetadata()
                        {
                            description = "The SAS token for the URL of the policy container"
                        },
                        type = "string"
                    };
                    parameters.Add("PolicyXMLSasToken", policyXMLSasTokenProperties);
                }
            }
            return parameters;
        }

        // this function will create master / parameter templates for deploying API revisions
        public Template CreateSingleAPIRevisionsMasterTemplate(List<string> revList, string currentRev, string linkedTemplatesUrlQueryString, string linkedTemplatesSasToken, string policyXMLBaseUrl, string policyXMLSasToken, FileNames fileNames)
        {
            // create empty template
            Template masterTemplate = GenerateEmptyTemplate();

            // add parameters
            masterTemplate.parameters = this.CreateMasterTemplateParameters(true, linkedTemplatesSasToken, linkedTemplatesUrlQueryString, policyXMLBaseUrl, policyXMLSasToken);

            // add deployment resources that links to all resource files
            List<TemplateResource> resources = new List<TemplateResource>();

            string curRevTemplate = String.Concat(currentRev, "MasterTemplate");
            int masterCnt = 0;

            foreach (string apiName in revList)
            {
                string revMasterPath = String.Concat("/", apiName, fileNames.linkedMaster);
                string revUri = GenerateLinkedTemplateUri(linkedTemplatesUrlQueryString, linkedTemplatesSasToken, revMasterPath);
                string templatename = String.Concat("masterTemplate", masterCnt++);
                if (!apiName.Equals(currentRev))
                {
                    resources.Add(this.CreateLinkedMasterTemplateResource(templatename, revUri, GenerateAPIRevisionDependencies(curRevTemplate)));
                }
                else
                {
                    resources.Add(this.CreateLinkedMasterTemplateResource(templatename, revUri, new string[] { }));
                }
            }

            masterTemplate.resources = resources.ToArray();
            return masterTemplate;
        }

        public string[] GenerateAPIRevisionDependencies(string curRevTemplate)
        {
            List<string> revDependsOn = new List<string>();
            revDependsOn.Add($"[resourceId('Microsoft.Resources/deployments', '{curRevTemplate}')]");
            return revDependsOn.ToArray();
        }

        public Template CreateMasterTemplateParameterValues(string apimServiceName, string linkedTemplatesBaseUrl, string linkedTemplatesSasToken, string linkedTemplatesUrlQueryString, string policyXMLBaseUrl, string policyXMLSasToken)
        {
            // used to create the parameter values for use in parameters file
            // create empty template
            Template masterTemplate = GenerateEmptyTemplate();

            // add parameters with value property
            Dictionary<string, TemplateParameterProperties> parameters = new Dictionary<string, TemplateParameterProperties>();
            TemplateParameterProperties apimServiceNameProperties = new TemplateParameterProperties()
            {
                value = apimServiceName
            };
            parameters.Add("ApimServiceName", apimServiceNameProperties);
            if (linkedTemplatesBaseUrl != null)
            {
                TemplateParameterProperties linkedTemplatesBaseUrlProperties = new TemplateParameterProperties()
                {
                    value = linkedTemplatesBaseUrl
                };
                parameters.Add("LinkedTemplatesBaseUrl", linkedTemplatesBaseUrlProperties);
                // add linkedTemplatesSasToken parameter if provided and if the user has provided a linkedTemplatesBaseUrl
                if (linkedTemplatesSasToken != null)
                {
                    TemplateParameterProperties linkedTemplatesSasTokenProperties = new TemplateParameterProperties()
                    {
                        value = linkedTemplatesSasToken
                    };
                    parameters.Add("LinkedTemplatesSasToken", linkedTemplatesSasTokenProperties);
                }
                // add linkedTemplatesUrlQueryString parameter if provided and if the user has provided a linkedTemplatesBaseUrl
                if (linkedTemplatesUrlQueryString != null)
                {
                    TemplateParameterProperties linkedTemplatesUrlQueryStringProperties = new TemplateParameterProperties()
                    {
                        value = linkedTemplatesUrlQueryString
                    };
                    parameters.Add("LinkedTemplatesUrlQueryString", linkedTemplatesUrlQueryStringProperties);
                }
            }
            if (policyXMLBaseUrl != null)
            {
                TemplateParameterProperties policyTemplateBaseUrlProperties = new TemplateParameterProperties()
                {
                    value = policyXMLBaseUrl
                };
                parameters.Add("PolicyXMLBaseUrl", policyTemplateBaseUrlProperties);
                // add policyXMLSasToken parameter if provided and if the user has provided a policyXMLBaseUrl
                if (policyXMLSasToken != null)
                {
                    TemplateParameterProperties policyTemplateSasTokenProperties = new TemplateParameterProperties()
                    {
                        value = policyXMLSasToken
                    };
                    parameters.Add("PolicyXMLSasToken", policyTemplateSasTokenProperties);
                }
            }
            masterTemplate.parameters = parameters;
            return masterTemplate;
        }

        public string GenerateLinkedTemplateUri(string linkedTemplatesUrlQueryString, string linkedTemplatesSasToken, string fileName)
        {
            string linkedTemplateUri = linkedTemplatesSasToken != null ? $"parameters('LinkedTemplatesBaseUrl'), '{fileName}', parameters('LinkedTemplatesSasToken')" : $"parameters('LinkedTemplatesBaseUrl'), '{fileName}'";
            return linkedTemplatesUrlQueryString != null ? $"[concat({linkedTemplateUri}, parameters('LinkedTemplatesUrlQueryString'))]" : $"[concat({linkedTemplateUri})]";
        }
    }
}
