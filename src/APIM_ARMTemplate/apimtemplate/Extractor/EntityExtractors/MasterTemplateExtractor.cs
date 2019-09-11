using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System.Linq;

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
            FileNames fileNames,
            string apiFileName,
            string linkedTemplatesUrlQueryString,
            string policyXMLBaseUrl)
        {
            // create empty template
            Template masterTemplate = GenerateEmptyTemplate();

            // add parameters
            masterTemplate.parameters = this.CreateMasterTemplateParameters(true, linkedTemplatesUrlQueryString, policyXMLBaseUrl);

            // add deployment resources that links to all resource files
            List<TemplateResource> resources = new List<TemplateResource>();

            // namedValue
            string namedValueDeploymentResourceName = "namedValuesTemplate";
            // all other deployment resources will depend on named values
            string[] dependsOnNamedValues = new string[] { $"[resourceId('Microsoft.Resources/deployments', '{namedValueDeploymentResourceName}')]" };
            if (namedValuesTemplate != null)
            {
                string namedValuesUri = GenerateLinkedTemplateUri(linkedTemplatesUrlQueryString, fileNames.namedValues);
                resources.Add(this.CreateLinkedMasterTemplateResource(namedValueDeploymentResourceName, namedValuesUri, new string[] { }));
            }

            // globalServicePolicy
            if (globalServicePolicyTemplate != null)
            {
                string globalServicePolicyUri = GenerateLinkedTemplateUri(linkedTemplatesUrlQueryString, fileNames.globalServicePolicy);
                resources.Add(this.CreateLinkedMasterTemplateResource("globalServicePolicyTemplate", globalServicePolicyUri, dependsOnNamedValues));
            }

            // apiVersionSet
            if (apiVersionSetTemplate != null)
            {
                string apiVersionSetUri = GenerateLinkedTemplateUri(linkedTemplatesUrlQueryString, fileNames.apiVersionSets);
                resources.Add(this.CreateLinkedMasterTemplateResource("versionSetTemplate", apiVersionSetUri, dependsOnNamedValues));
            }

            // product
            if (productsTemplate != null)
            {
                string productsUri = GenerateLinkedTemplateUri(linkedTemplatesUrlQueryString, fileNames.products);
                resources.Add(this.CreateLinkedMasterTemplateResource("productsTemplate", productsUri, dependsOnNamedValues));
            }

            // logger
            if (loggersTemplate != null)
            {
                string loggersUri = GenerateLinkedTemplateUri(linkedTemplatesUrlQueryString, fileNames.loggers);
                resources.Add(this.CreateLinkedMasterTemplateResource("loggersTemplate", loggersUri, dependsOnNamedValues));
            }

            // backend
            if (backendsTemplate != null)
            {
                string backendsUri = GenerateLinkedTemplateUri(linkedTemplatesUrlQueryString, fileNames.backends);
                resources.Add(this.CreateLinkedMasterTemplateResource("backendsTemplate", backendsUri, dependsOnNamedValues));
            }

            // authorizationServer
            if (authorizationServersTemplate != null)
            {
                string authorizationServersUri = GenerateLinkedTemplateUri(linkedTemplatesUrlQueryString, fileNames.authorizationServers);
                resources.Add(this.CreateLinkedMasterTemplateResource("authorizationServersTemplate", authorizationServersUri, dependsOnNamedValues));
            }

            // api
            if (apiTemplate != null)
            {
                string apisUri = GenerateLinkedTemplateUri(linkedTemplatesUrlQueryString, apiFileName);
                resources.Add(this.CreateLinkedMasterTemplateResource("apisTemplate", apisUri, GenerateAPIResourceDependencies(apiTemplate, globalServicePolicyTemplate, apiVersionSetTemplate, productsTemplate, loggersTemplate, backendsTemplate, authorizationServersTemplate, namedValueDeploymentResourceName)));
            }

            masterTemplate.resources = resources.ToArray();
            return masterTemplate;
        }

        public string[] GenerateAPIResourceDependencies(Template apiTemplate,
            Template globalServicePolicyTemplate,
            Template apiVersionSetTemplate,
            Template productsTemplate,
            Template loggersTemplate,
            Template backendsTemplate,
            Template authorizationServersTemplate,
            string namedValueDeploymentResourceName)
        {
            List<string> apiDependsOn = new List<string>();
            var apiResources = apiTemplate.resources.Where(resource => resource.type == ResourceTypeConstants.API);

            // add dependency on all other template files by default for now
            apiDependsOn.Add($"[resourceId('Microsoft.Resources/deployments', '{namedValueDeploymentResourceName}')]");
            apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'globalServicePolicyTemplate')]");
            apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'versionSetTemplate')]");
            apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'productsTemplate')]");
            apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'loggersTemplate')]");
            apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'backendsTemplate')]");
            apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'authorizationServersTemplate')]");

            return apiDependsOn.ToArray();
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
                        { "ApimServiceName", new TemplateParameterProperties(){ value = "[parameters('ApimServiceName')]" } }
                    }
                },
                dependsOn = dependsOn
            };
            return masterTemplateResource;
        }

        public Dictionary<string, TemplateParameterProperties> CreateMasterTemplateParameters(bool linked, string linkedTemplatesUrlQueryString, string policyXMLBaseUrl)
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
            }
            return parameters;
        }

        public Template CreateMasterTemplateParameterValues(string apimServiceName, string linkedTemplatesBaseUrl, string linkedTemplatesUrlQueryString, string policyXMLBaseUrl)
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
            }
            masterTemplate.parameters = parameters;
            return masterTemplate;
        }

        public string GenerateLinkedTemplateUri(string linkedTemplatesUrlQueryString, string fileName)
        {
            return linkedTemplatesUrlQueryString != null ? $"[concat(parameters('LinkedTemplatesBaseUrl'), '{fileName}', parameters('LinkedTemplatesUrlQueryString'))]" : $"[concat(parameters('LinkedTemplatesBaseUrl'), '{fileName}')]";
        }
    }
}
