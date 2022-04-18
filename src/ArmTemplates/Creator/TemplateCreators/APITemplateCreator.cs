// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Net;
using System.Linq;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Utilities;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.TagApi;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class APITemplateCreator
    {
        readonly ITemplateBuilder templateBuilder;

        FileReader fileReader;
        PolicyTemplateCreator policyTemplateCreator;
        ProductAPITemplateCreator productAPITemplateCreator;
        TagAPITemplateCreator tagAPITemplateCreator;
        DiagnosticTemplateCreator diagnosticTemplateCreator;
        ReleaseTemplateCreator releaseTemplateCreator;

        public APITemplateCreator(
            FileReader fileReader, 
            PolicyTemplateCreator policyTemplateCreator, 
            ProductAPITemplateCreator productAPITemplateCreator, 
            TagAPITemplateCreator tagAPITemplateCreator, 
            DiagnosticTemplateCreator diagnosticTemplateCreator, 
            ReleaseTemplateCreator releaseTemplateCreator,
            ITemplateBuilder templateBuilder)
        {
            this.fileReader = fileReader;
            this.policyTemplateCreator = policyTemplateCreator;
            this.productAPITemplateCreator = productAPITemplateCreator;
            this.tagAPITemplateCreator = tagAPITemplateCreator;
            this.diagnosticTemplateCreator = diagnosticTemplateCreator;
            this.releaseTemplateCreator = releaseTemplateCreator;
            this.templateBuilder = templateBuilder;
        }

        public async Task<List<Template>> CreateAPITemplatesAsync(APIConfig api)
        {
            // determine if api needs to be split into multiple templates
            bool isSplit = this.IsSplitAPI(api);

            // update api name if necessary (apiRevision > 1 and isCurrent = true) 
            int revisionNumber = 0;
            if (int.TryParse(api.apiRevision, out revisionNumber))
            {
                if (revisionNumber > 1 && api.isCurrent == false)
                {
                    string currentAPIName = api.name;
                    api.name += $";rev={revisionNumber}";
                }
            }

            List<Template> apiTemplates = new List<Template>();
            if (isSplit == true)
            {
                // create 2 templates, an initial template with metadata and a subsequent template with the swagger content
                apiTemplates.Add(await this.CreateAPITemplateAsync(api, isSplit, true));
                apiTemplates.Add(await this.CreateAPITemplateAsync(api, isSplit, false));
            }
            else
            {
                // create a unified template that includes both the metadata and swagger content 
                apiTemplates.Add(await this.CreateAPITemplateAsync(api, isSplit, false));
            }
            return apiTemplates;
        }

        public async Task<Template> CreateAPITemplateAsync(APIConfig api, bool isSplit, bool isInitial)
        {
            // create empty template
            Template apiTemplate = this.templateBuilder.GenerateEmptyTemplate().Build();

            // add parameters
            apiTemplate.Parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { ParameterNames.ApimServiceName, new TemplateParameterProperties(){ Type = "string" } }
            };

            if (!string.IsNullOrEmpty(api.serviceUrl))
            {
                apiTemplate.Parameters.Add(api.name + "-ServiceUrl", new TemplateParameterProperties() { Type = "string" });
            }

            List<TemplateResource> resources = new List<TemplateResource>();
            // create api resource 
            APITemplateResource apiTemplateResource = await this.CreateAPITemplateResourceAsync(api, isSplit, isInitial);
            resources.Add(apiTemplateResource);
            // add the api child resources (api policies, diagnostics, etc) if this is the unified or subsequent template
            if (!isSplit || !isInitial)
            {
                resources.AddRange(this.CreateChildResourceTemplates(api));
            }
            apiTemplate.Resources = resources.ToArray();

            return apiTemplate;
        }

        public List<TemplateResource> CreateChildResourceTemplates(APIConfig api)
        {
            List<TemplateResource> resources = new List<TemplateResource>();
            // all child resources will depend on the api
            string[] dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{api.name}')]" };

            PolicyTemplateResource apiPolicyResource = api.policy != null ? this.policyTemplateCreator.CreateAPIPolicyTemplateResource(api, dependsOn) : null;
            List<PolicyTemplateResource> operationPolicyResources = api.operations != null ? this.policyTemplateCreator.CreateOperationPolicyTemplateResources(api, dependsOn) : null;
            List<ProductApiTemplateResource> productAPIResources = api.products != null ? this.productAPITemplateCreator.CreateProductAPITemplateResources(api, dependsOn) : null;
            List<TagApiTemplateResource> tagAPIResources = api.tags != null ? this.tagAPITemplateCreator.CreateTagAPITemplateResources(api, dependsOn) : null;
            DiagnosticTemplateResource diagnosticTemplateResource = api.diagnostic != null ? this.diagnosticTemplateCreator.CreateAPIDiagnosticTemplateResource(api, dependsOn) : null;
            // add release resource if the name has been appended with ;rev{revisionNumber}
            ReleaseTemplateResource releaseTemplateResource = api.name.Contains(";rev") == true ? this.releaseTemplateCreator.CreateAPIReleaseTemplateResource(api, dependsOn) : null;

            // add resources if not null
            if (apiPolicyResource != null) resources.Add(apiPolicyResource);
            if (operationPolicyResources != null) resources.AddRange(operationPolicyResources);
            if (productAPIResources != null) resources.AddRange(productAPIResources);
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
                Name = MakeResourceName(api),
                Type = ResourceTypeConstants.API,
                ApiVersion = GlobalConstants.ApiVersion,
                Properties = new APITemplateProperties(),
                DependsOn = new string[] { }
            };

            // add properties depending on whether the template is the initial, subsequent, or unified 
            if (!isSplit || !isInitial)
            {
                // add metadata properties for initial and unified templates
                apiTemplateResource.Properties.ApiVersion = api.apiVersion;
                apiTemplateResource.Properties.ServiceUrl = this.MakeServiceUrl(api);
                apiTemplateResource.Properties.Type = api.type;
                apiTemplateResource.Properties.ApiType = api.type;
                apiTemplateResource.Properties.Description = api.description;
                apiTemplateResource.Properties.SubscriptionRequired = api.subscriptionRequired;
                apiTemplateResource.Properties.ApiRevision = api.apiRevision;
                apiTemplateResource.Properties.ApiRevisionDescription = api.apiRevisionDescription;
                apiTemplateResource.Properties.ApiVersionDescription = api.apiVersionDescription;
                apiTemplateResource.Properties.AuthenticationSettings = api.authenticationSettings;
                apiTemplateResource.Properties.SubscriptionKeyParameterNames = api.subscriptionKeyParameterNames;
                apiTemplateResource.Properties.Path = api.suffix;
                apiTemplateResource.Properties.IsCurrent = api.isCurrent;
                apiTemplateResource.Properties.DisplayName = string.IsNullOrEmpty(api.displayName) ? api.name : api.displayName;
                apiTemplateResource.Properties.Protocols = this.CreateProtocols(api);
                // set the version set id
                if (api.apiVersionSetId != null)
                {
                    // point to the supplied version set if the apiVersionSetId is provided
                    apiTemplateResource.Properties.ApiVersionSetId = $"[resourceId('Microsoft.ApiManagement/service/apiVersionSets', parameters('{ParameterNames.ApimServiceName}'), '{api.apiVersionSetId}')]";
                }
                // set the authorization server id
                if (api.authenticationSettings != null && api.authenticationSettings.OAuth2 != null && api.authenticationSettings.OAuth2.AuthorizationServerId != null
                    && apiTemplateResource.Properties.AuthenticationSettings != null && apiTemplateResource.Properties.AuthenticationSettings.OAuth2 != null && apiTemplateResource.Properties.AuthenticationSettings.OAuth2.AuthorizationServerId != null)
                {
                    apiTemplateResource.Properties.AuthenticationSettings.OAuth2.AuthorizationServerId = api.authenticationSettings.OAuth2.AuthorizationServerId;
                }
                // set the subscriptionKey Parameter Names
                if (api.subscriptionKeyParameterNames != null)
                {
                    if (api.subscriptionKeyParameterNames.Header != null)
                    {
                        apiTemplateResource.Properties.SubscriptionKeyParameterNames.Header = api.subscriptionKeyParameterNames.Header;
                    }
                    if (api.subscriptionKeyParameterNames.Query != null)
                    {
                        apiTemplateResource.Properties.SubscriptionKeyParameterNames.Query = api.subscriptionKeyParameterNames.Query;
                    }
                }
            }
            if (!isSplit || isInitial)
            {
                // add open api spec properties for subsequent and unified templates
                string format;
                string value;

                // determine if the open api spec is remote or local, yaml or json
                bool isJSON = false;
                bool isUrl = IsUri(api, out var _);

                string fileContents = null;
                if (!isUrl || api.openApiSpecFormat == OpenApiSpecFormat.Unspecified)
                    fileContents = await this.fileReader.RetrieveFileContentsAsync(api.openApiSpec);

                value = isUrl
                    ? api.openApiSpec
                    : fileContents
                    ;

                bool isVersionThree = false;
                if (api.openApiSpecFormat == OpenApiSpecFormat.Unspecified)
                {
                    isJSON = this.fileReader.IsJSON(fileContents);

                    if (isJSON == true)
                    {
                        var openAPISpecReader = new OpenAPISpecReader();
                        isVersionThree = await openAPISpecReader.IsJSONOpenAPISpecVersionThreeAsync(api.openApiSpec);
                    }
                    format = GetOpenApiSpecFormat(isUrl, isJSON, isVersionThree);
                }

                else
                {
                    isJSON = IsOpenApiSpecJson(api.openApiSpecFormat);
                    format = GetOpenApiSpecFormat(isUrl, api.openApiSpecFormat);
                }

                // if the title needs to be modified
                // we need to embed the OpenAPI definition

                if (!string.IsNullOrEmpty(api.displayName))
                {
                    format = GetOpenApiSpecFormat(false, isJSON, isVersionThree);

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
                    apiTemplateResource.Properties.ApiVersionSetId = $"[resourceId('Microsoft.ApiManagement/service/apiVersionSets', parameters('{ParameterNames.ApimServiceName}'), '{api.apiVersionSetId}')]";
                }
                apiTemplateResource.Properties.Format = format;
                apiTemplateResource.Properties.Value = value;

                // #562: deploying multiple versions of an API may fail because while trying to deploy the initial template
                // overwrite the initial template’s path property to a dummy value
                // this value will be restored when the subsequent template is deployed

                if (isSplit && isInitial)
                {
                    apiTemplateResource.Properties.Path = api.suffix + $"/{Guid.NewGuid():n}";
                }
                else
                {
                    apiTemplateResource.Properties.Path = api.suffix;
                }

                if (!string.IsNullOrEmpty(api.serviceUrl))
                {
                    apiTemplateResource.Properties.ServiceUrl = this.MakeServiceUrl(api);
                }
            }
            return apiTemplateResource;
        }

        internal static IDictionary<string, string[]> GetApiVersionSets(CreatorConfig creatorConfig)
        {
            var apiVersions = (creatorConfig.apiVersionSets ?? new List<APIVersionSetConfig>())
                .ToDictionary(v => v.id, v => new List<string>())
                ;

            foreach (var api in creatorConfig.apis.Where(a => !string.IsNullOrEmpty(a.apiVersionSetId)))
                apiVersions[api.apiVersionSetId].Add(api.name)
                    ;

            return apiVersions.ToDictionary(
                kv => kv.Key,
                kv => kv.Value.OrderBy(v => v).ToArray()
                );
        }

        static string GetOpenApiSpecFormat(bool isUrl, bool isJSON, bool isVersionThree)
        {
            return isUrl
                ? isJSON ? isVersionThree ? "openapi-link" : "swagger-link-json" : "openapi-link"
                : isJSON ? isVersionThree ? "openapi+json" : "swagger-json" : "openapi";
        }

        static string GetOpenApiSpecFormat(bool isUrl, OpenApiSpecFormat openApiSpecFormat)
        {
            switch (openApiSpecFormat)
            {
                case OpenApiSpecFormat.Swagger_Json:
                    return isUrl ? "swagger-link-json" : "swagger-json";

                case OpenApiSpecFormat.OpenApi20_Yaml:
                    return isUrl ? "openapi-link" : "openapi";

                case OpenApiSpecFormat.OpenApi20_Json:
                    return isUrl ? "openapi-link" : "swagger-json";

                case OpenApiSpecFormat.OpenApi30_Yaml:
                    return isUrl ? "openapi-link" : "openapi";

                case OpenApiSpecFormat.OpenApi30_Json:
                    return isUrl ? "openapi-link" : "openapi+json";

                default:
                    throw new NotSupportedException();
            }
        }
        static bool IsOpenApiSpecJson(OpenApiSpecFormat openApiSpecFormat)
        {
            switch (openApiSpecFormat)
            {
                case OpenApiSpecFormat.Swagger_Json:
                case OpenApiSpecFormat.OpenApi20_Json:
                case OpenApiSpecFormat.OpenApi30_Json:
                    return true;

                case OpenApiSpecFormat.OpenApi20_Yaml:
                case OpenApiSpecFormat.OpenApi30_Yaml:
                    return false;

                default:
                    throw new NotSupportedException();
            }
        }

        static bool IsUri(APIConfig api, out Uri uriResult)
        {
            return
                Uri.TryCreate(api.openApiSpec, UriKind.Absolute, out uriResult) &&
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)
                ;
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

        public bool IsSplitAPI(APIConfig apiConfig)
        {
            // the api needs to be split into multiple templates if the user has supplied a version or version set - deploying swagger related properties at the same time as api version related properties fails, so they must be written and deployed separately
            return apiConfig.apiVersion != null || apiConfig.apiVersionSetId != null || apiConfig.authenticationSettings != null && apiConfig.authenticationSettings.OAuth2 != null && apiConfig.authenticationSettings.OAuth2.AuthorizationServerId != null;
        }

        string MakeServiceUrl(APIConfig api)
        {
            return !string.IsNullOrEmpty(api.serviceUrl) ? $"[parameters('{api.name + "-ServiceUrl"}')]" : null;
        }
    }
}
