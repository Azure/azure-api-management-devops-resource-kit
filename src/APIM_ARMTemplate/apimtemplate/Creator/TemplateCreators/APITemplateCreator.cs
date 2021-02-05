﻿using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using apimtemplate.Creator.Utilities;
using System.Net;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class APITemplateCreator : TemplateCreator
    {
        private FileReader fileReader;
        private PolicyTemplateCreator policyTemplateCreator;
        private TagAPITemplateCreator tagAPITemplateCreator;
        private DiagnosticTemplateCreator diagnosticTemplateCreator;
        private ReleaseTemplateCreator releaseTemplateCreator;

        public APITemplateCreator(FileReader fileReader, PolicyTemplateCreator policyTemplateCreator, TagAPITemplateCreator tagAPITemplateCreator, DiagnosticTemplateCreator diagnosticTemplateCreator, ReleaseTemplateCreator releaseTemplateCreator)
        {
            this.fileReader = fileReader;
            this.policyTemplateCreator = policyTemplateCreator;
            this.tagAPITemplateCreator = tagAPITemplateCreator;
            this.diagnosticTemplateCreator = diagnosticTemplateCreator;
            this.releaseTemplateCreator = releaseTemplateCreator;
        }

        public async Task<List<Template>> CreateAPITemplatesAsync(APIConfig api)
        {
            // determine if api needs to be split into multiple templates
            bool isSplit = isSplitAPI(api);

            // update api name if necessary (apiRevision > 1 and isCurrent = true) 
            int revisionNumber = 0;
            if (Int32.TryParse(api.apiRevision, out revisionNumber))
            {
                if (revisionNumber > 1 && api.isCurrent == true)
                {
                    string currentAPIName = api.name;
                    api.name += $";rev={revisionNumber}";
                }
            }

            List<Template> apiTemplates = new List<Template>();
            if (isSplit == true)
            {
                // create 2 templates, an initial template with metadata and a subsequent template with the swagger content
                apiTemplates.Add(await CreateAPITemplateAsync(api, isSplit, true));
                apiTemplates.Add(await CreateAPITemplateAsync(api, isSplit, false));
            }
            else
            {
                // create a unified template that includes both the metadata and swagger content 
                apiTemplates.Add(await CreateAPITemplateAsync(api, isSplit, false));
            }
            return apiTemplates;
        }

        public async Task<Template> CreateAPITemplateAsync(APIConfig api, bool isSplit, bool isInitial)
        {
            // create empty template
            Template apiTemplate = CreateEmptyTemplate();

            // add parameters
            apiTemplate.parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { ParameterNames.ApimServiceName, new TemplateParameterProperties(){ type = "string" } }
            };

            if (String.IsNullOrEmpty(api.serviceUrl))
            {
                apiTemplate.parameters.Add(ParameterNames.ServiceUrl, new TemplateParameterProperties() { type = "object" });
            }

            List<TemplateResource> resources = new List<TemplateResource>();
            // create api resource 
            APITemplateResource apiTemplateResource = await this.CreateAPITemplateResourceAsync(api, isSplit, isInitial);
            resources.Add(apiTemplateResource);
            // add the api child resources (api policies, diagnostics, etc) if this is the unified or subsequent template
            if (!isSplit || !isInitial)
            {
                resources.AddRange(CreateChildResourceTemplates(api));
            }
            apiTemplate.resources = resources.ToArray();

            return apiTemplate;
        }

        public List<TemplateResource> CreateChildResourceTemplates(APIConfig api)
        {
            List<TemplateResource> resources = new List<TemplateResource>();
            // all child resources will depend on the api
            string[] dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{api.name}')]" };

            PolicyTemplateResource apiPolicyResource = api.policy != null ? this.policyTemplateCreator.CreateAPIPolicyTemplateResource(api, dependsOn) : null;
            List<PolicyTemplateResource> operationPolicyResources = api.operations != null ? this.policyTemplateCreator.CreateOperationPolicyTemplateResources(api, dependsOn) : null;
            List<TagAPITemplateResource> tagAPIResources = api.tags != null ? this.tagAPITemplateCreator.CreateTagAPITemplateResources(api,dependsOn) : null;
            DiagnosticTemplateResource diagnosticTemplateResource = api.diagnostic != null ? this.diagnosticTemplateCreator.CreateAPIDiagnosticTemplateResource(api, dependsOn) : null;
            // add release resource if the name has been appended with ;rev{revisionNumber}
            ReleaseTemplateResource releaseTemplateResource = api.name.Contains(";rev") == true ? this.releaseTemplateCreator.CreateAPIReleaseTemplateResource(api, dependsOn) : null;

            // add resources if not null
            if (apiPolicyResource != null) resources.Add(apiPolicyResource);
            if (operationPolicyResources != null) resources.AddRange(operationPolicyResources);
            if (tagAPIResources != null) resources.AddRange(tagAPIResources);
            if (diagnosticTemplateResource != null) resources.Add(diagnosticTemplateResource);
            if (releaseTemplateResource != null) resources.Add(releaseTemplateResource);

            return resources;
        }

        public async Task<APITemplateResource> CreateAPITemplateResourceAsync(APIConfig api, bool isSplit, bool isInitial)
        {
            // create api resource
            APITemplateResource apiTemplateResource = new APITemplateResource()
            {
                name = MakeResourceName(api),
                type = ResourceTypeConstants.API,
                apiVersion = GlobalConstants.APIVersion,
                properties = new APITemplateProperties(),
                dependsOn = new string[] { }
            };

            // add properties depending on whether the template is the initial, subsequent, or unified 
            if (!isSplit || !isInitial)
            {
                // add metadata properties for initial and unified templates
                apiTemplateResource.properties.apiVersion = api.apiVersion;
                apiTemplateResource.properties.serviceUrl = MakeServiceUrl(api);
                apiTemplateResource.properties.type = api.type;
                apiTemplateResource.properties.apiType = api.type;
                apiTemplateResource.properties.description = api.description;
                apiTemplateResource.properties.subscriptionRequired = api.subscriptionRequired;
                apiTemplateResource.properties.apiRevision = api.apiRevision;
                apiTemplateResource.properties.apiRevisionDescription = api.apiRevisionDescription;
                apiTemplateResource.properties.apiVersionDescription = api.apiVersionDescription;
                apiTemplateResource.properties.authenticationSettings = api.authenticationSettings;
                apiTemplateResource.properties.subscriptionKeyParameterNames = api.subscriptionKeyParameterNames;
                apiTemplateResource.properties.path = api.suffix;
                apiTemplateResource.properties.isCurrent = api.isCurrent;
                apiTemplateResource.properties.displayName = string.IsNullOrEmpty(api.displayName) ? api.name : api.displayName;
                apiTemplateResource.properties.protocols = this.CreateProtocols(api);
                // set the version set id
                if (api.apiVersionSetId != null)
                {
                    // point to the supplied version set if the apiVersionSetId is provided
                    apiTemplateResource.properties.apiVersionSetId = $"[resourceId('Microsoft.ApiManagement/service/apiVersionSets', parameters('{ParameterNames.ApimServiceName}'), '{api.apiVersionSetId}')]";
                }
                // set the authorization server id
                if (api.authenticationSettings != null && api.authenticationSettings.oAuth2 != null && api.authenticationSettings.oAuth2.authorizationServerId != null
                    && apiTemplateResource.properties.authenticationSettings != null && apiTemplateResource.properties.authenticationSettings.oAuth2 != null && apiTemplateResource.properties.authenticationSettings.oAuth2.authorizationServerId != null)
                {
                    apiTemplateResource.properties.authenticationSettings.oAuth2.authorizationServerId = api.authenticationSettings.oAuth2.authorizationServerId;
                }
                // set the subscriptionKey Parameter Names
                if (api.subscriptionKeyParameterNames != null)
                {
                    if (api.subscriptionKeyParameterNames.header != null)
                    {
                        apiTemplateResource.properties.subscriptionKeyParameterNames.header = api.subscriptionKeyParameterNames.header;
                    }
                    if (api.subscriptionKeyParameterNames.query != null)
                    {
                        apiTemplateResource.properties.subscriptionKeyParameterNames.query = api.subscriptionKeyParameterNames.query;
                    }
                }
            }
            if (!isSplit || isInitial)
            {
                // add open api spec properties for subsequent and unified templates
                string format;
                string value;

                // determine if the open api spec is remote or local, yaml or json
                Uri uriResult;
                string fileContents = await this.fileReader.RetrieveFileContentsAsync(api.openApiSpec);
                bool isJSON = this.fileReader.isJSON(fileContents);
                bool isUrl = Uri.TryCreate(api.openApiSpec, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                value = isUrl
                    ? api.openApiSpec
                    : fileContents
                    ;

                bool isVersionThree = false;
                if (isJSON == true)
                {
                    // open api spec is remote json file, use swagger-link-json for v2 and openapi-link for v3
                    OpenAPISpecReader openAPISpecReader = new OpenAPISpecReader();
                    isVersionThree = await openAPISpecReader.isJSONOpenAPISpecVersionThreeAsync(api.openApiSpec);
                }

                format = isUrl
                    ? (isJSON ? (isVersionThree ? "openapi-link" : "swagger-link-json") : "openapi-link")
                    : (isJSON ? (isVersionThree ? "openapi+json" : "swagger-json") : "openapi")
                    ;

                // if the title needs to be modified
                // we need to embed the OpenAPI definition

                if (!string.IsNullOrEmpty(api.displayName))
                {
                    format = (isJSON ? (isVersionThree ? "openapi+json" : "swagger-json") : "openapi");

                    // download definition

                    if (isUrl)
                    {
                        using (var client = new WebClient())
                            value = client.DownloadString(value);
                    }

                    // update title

                    value = new OpenApi(value, format)
                        .SetTitle(api.displayName)
                        .GetDefinition()
                        ;
                }
                // set the version set id
                if (api.apiVersionSetId != null)
                {
                    // point to the supplied version set if the apiVersionSetId is provided
                    apiTemplateResource.properties.apiVersionSetId = $"[resourceId('Microsoft.ApiManagement/service/apiVersionSets', parameters('{ParameterNames.ApimServiceName}'), '{api.apiVersionSetId}')]";
                }
                apiTemplateResource.properties.format = format;
                apiTemplateResource.properties.value = value;
                apiTemplateResource.properties.path = api.suffix;
                apiTemplateResource.properties.serviceUrl = MakeServiceUrl(api);

            }
            return apiTemplateResource;
        }

        public static string MakeResourceName(APIConfig api)
        {
            return $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.name}')]";
        }

        public string[] CreateProtocols(APIConfig api)
        {
            string[] protocols;
            if (api.protocols != null)
            {
                protocols = api.protocols.Split(", ");
            }
            else
            {
                protocols = new string[1] { "https" };
            }
            return protocols;
        }

        public bool isSplitAPI(APIConfig apiConfig)
        {
            // the api needs to be split into multiple templates if the user has supplied a version or version set - deploying swagger related properties at the same time as api version related properties fails, so they must be written and deployed separately
            return apiConfig.apiVersion != null || apiConfig.apiVersionSetId != null || (apiConfig.authenticationSettings != null && apiConfig.authenticationSettings.oAuth2 != null && apiConfig.authenticationSettings.oAuth2.authorizationServerId != null);
        }

        private string MakeServiceUrl(APIConfig api)
        {
            return api.serviceUrl ?? $"[parameters('{ParameterNames.ServiceUrl}').{ExtractorUtils.GenValidParamName(api.name, ParameterPrefix.Api)}]";
        }
    }
}
