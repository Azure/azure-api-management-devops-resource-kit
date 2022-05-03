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
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Utilities;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.TagApi;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class ApiTemplateCreator : IApiTemplateCreator
    {
        readonly ITemplateBuilder templateBuilder;

        FileReader fileReader;
        readonly IPolicyTemplateCreator policyTemplateCreator;
        readonly IProductApiTemplateCreator productAPITemplateCreator;
        readonly ITagApiTemplateCreator tagAPITemplateCreator;
        readonly IDiagnosticTemplateCreator diagnosticTemplateCreator;
        readonly IReleaseTemplateCreator releaseTemplateCreator;

        public ApiTemplateCreator(
            FileReader fileReader, 
            IPolicyTemplateCreator policyTemplateCreator, 
            IProductApiTemplateCreator productAPITemplateCreator, 
            ITagApiTemplateCreator tagAPITemplateCreator, 
            IDiagnosticTemplateCreator diagnosticTemplateCreator, 
            IReleaseTemplateCreator releaseTemplateCreator,
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

        public async Task<List<Template>> CreateAPITemplatesAsync(ApiConfig api)
        {
            // determine if api needs to be split into multiple templates
            bool isSplit = this.IsSplitAPI(api);

            // update api name if necessary (apiRevision > 1 and isCurrent = true) 
            int revisionNumber = 0;
            if (int.TryParse(api.ApiRevision, out revisionNumber))
            {
                if (revisionNumber > 1 && api.IsCurrent == false)
                {
                    string currentAPIName = api.Name;
                    api.Name += $";rev={revisionNumber}";
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

        public async Task<Template> CreateAPITemplateAsync(ApiConfig api, bool isSplit, bool isInitial)
        {
            // create empty template
            Template apiTemplate = this.templateBuilder.GenerateEmptyTemplate().Build();

            // add parameters
            apiTemplate.Parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { ParameterNames.ApimServiceName, new TemplateParameterProperties(){ Type = "string" } }
            };

            if (!string.IsNullOrEmpty(api.ServiceUrl))
            {
                apiTemplate.Parameters.Add(api.Name + "-ServiceUrl", new TemplateParameterProperties() { Type = "string" });
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

        public List<TemplateResource> CreateChildResourceTemplates(ApiConfig api)
        {
            List<TemplateResource> resources = new List<TemplateResource>();
            // all child resources will depend on the api
            string[] dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{api.Name}')]" };

            PolicyTemplateResource apiPolicyResource = api.Policy != null ? this.policyTemplateCreator.CreateAPIPolicyTemplateResource(api, dependsOn) : null;
            List<PolicyTemplateResource> operationPolicyResources = api.Operations != null ? this.policyTemplateCreator.CreateOperationPolicyTemplateResources(api, dependsOn) : null;
            List<ProductApiTemplateResource> productAPIResources = api.Products != null ? this.productAPITemplateCreator.CreateProductAPITemplateResources(api, dependsOn) : null;
            List<TagApiTemplateResource> tagAPIResources = api.Tags != null ? this.tagAPITemplateCreator.CreateTagAPITemplateResources(api, dependsOn) : null;
            DiagnosticTemplateResource diagnosticTemplateResource = api.Diagnostic != null ? this.diagnosticTemplateCreator.CreateAPIDiagnosticTemplateResource(api, dependsOn) : null;
            // add release resource if the name has been appended with ;rev{revisionNumber}
            ReleaseTemplateResource releaseTemplateResource = api.Name.Contains(";rev") == true ? this.releaseTemplateCreator.CreateAPIReleaseTemplateResource(api, dependsOn) : null;

            // add resources if not null
            if (apiPolicyResource != null) resources.Add(apiPolicyResource);
            if (operationPolicyResources != null) resources.AddRange(operationPolicyResources);
            if (productAPIResources != null) resources.AddRange(productAPIResources);
            if (tagAPIResources != null) resources.AddRange(tagAPIResources);
            if (diagnosticTemplateResource != null) resources.Add(diagnosticTemplateResource);
            if (releaseTemplateResource != null) resources.Add(releaseTemplateResource);

            return resources;
        }

        public async Task<APITemplateResource> CreateAPITemplateResourceAsync(ApiConfig api, bool isSplit, bool isInitial)
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
                apiTemplateResource.Properties.ApiVersion = api.ApiVersion;
                apiTemplateResource.Properties.ServiceUrl = this.MakeServiceUrl(api);
                apiTemplateResource.Properties.Type = api.Type;
                apiTemplateResource.Properties.ApiType = api.Type;
                apiTemplateResource.Properties.Description = api.Description;
                apiTemplateResource.Properties.SubscriptionRequired = api.SubscriptionRequired;
                apiTemplateResource.Properties.ApiRevision = api.ApiRevision;
                apiTemplateResource.Properties.ApiRevisionDescription = api.ApiRevisionDescription;
                apiTemplateResource.Properties.ApiVersionDescription = api.ApiVersionDescription;
                apiTemplateResource.Properties.AuthenticationSettings = api.AuthenticationSettings;
                apiTemplateResource.Properties.SubscriptionKeyParameterNames = api.SubscriptionKeyParameterNames;
                apiTemplateResource.Properties.Path = api.Suffix;
                apiTemplateResource.Properties.IsCurrent = api.IsCurrent;
                apiTemplateResource.Properties.DisplayName = string.IsNullOrEmpty(api.DisplayName) ? api.Name : api.DisplayName;
                apiTemplateResource.Properties.Protocols = this.CreateProtocols(api);
                // set the version set id
                if (api.ApiVersionSetId != null)
                {
                    // point to the supplied version set if the apiVersionSetId is provided
                    apiTemplateResource.Properties.ApiVersionSetId = $"[resourceId('Microsoft.ApiManagement/service/apiVersionSets', parameters('{ParameterNames.ApimServiceName}'), '{api.ApiVersionSetId}')]";
                }
                // set the authorization server id
                if (api.AuthenticationSettings != null && api.AuthenticationSettings.OAuth2 != null && api.AuthenticationSettings.OAuth2.AuthorizationServerId != null
                    && apiTemplateResource.Properties.AuthenticationSettings != null && apiTemplateResource.Properties.AuthenticationSettings.OAuth2 != null && apiTemplateResource.Properties.AuthenticationSettings.OAuth2.AuthorizationServerId != null)
                {
                    apiTemplateResource.Properties.AuthenticationSettings.OAuth2.AuthorizationServerId = api.AuthenticationSettings.OAuth2.AuthorizationServerId;
                }
                // set the subscriptionKey Parameter Names
                if (api.SubscriptionKeyParameterNames != null)
                {
                    if (api.SubscriptionKeyParameterNames.Header != null)
                    {
                        apiTemplateResource.Properties.SubscriptionKeyParameterNames.Header = api.SubscriptionKeyParameterNames.Header;
                    }
                    if (api.SubscriptionKeyParameterNames.Query != null)
                    {
                        apiTemplateResource.Properties.SubscriptionKeyParameterNames.Query = api.SubscriptionKeyParameterNames.Query;
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
                if (!isUrl || api.OpenApiSpecFormat == OpenApiSpecFormat.Unspecified)
                    fileContents = await this.fileReader.RetrieveFileContentsAsync(api.OpenApiSpec);

                value = isUrl
                    ? api.OpenApiSpec
                    : fileContents
                    ;

                bool isVersionThree = false;
                if (api.OpenApiSpecFormat == OpenApiSpecFormat.Unspecified)
                {
                    isJSON = this.fileReader.IsJSON(fileContents);

                    if (isJSON == true)
                    {
                        var openAPISpecReader = new OpenAPISpecReader();
                        isVersionThree = await openAPISpecReader.IsJSONOpenAPISpecVersionThreeAsync(api.OpenApiSpec);
                    }
                    format = GetOpenApiSpecFormat(isUrl, isJSON, isVersionThree);
                }

                else
                {
                    isJSON = IsOpenApiSpecJson(api.OpenApiSpecFormat);
                    format = GetOpenApiSpecFormat(isUrl, api.OpenApiSpecFormat);
                }

                // if the title needs to be modified
                // we need to embed the OpenAPI definition

                if (!string.IsNullOrEmpty(api.DisplayName))
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
                        .SetTitle(api.DisplayName)
                        .GetDefinition()
                        ;
                }

                // set the version set id
                if (api.ApiVersionSetId != null)
                {
                    // point to the supplied version set if the apiVersionSetId is provided
                    apiTemplateResource.Properties.ApiVersionSetId = $"[resourceId('Microsoft.ApiManagement/service/apiVersionSets', parameters('{ParameterNames.ApimServiceName}'), '{api.ApiVersionSetId}')]";
                }
                apiTemplateResource.Properties.Format = format;
                apiTemplateResource.Properties.Value = value;

                // #562: deploying multiple versions of an API may fail because while trying to deploy the initial template
                // overwrite the initial template’s path property to a dummy value
                // this value will be restored when the subsequent template is deployed

                if (isSplit && isInitial)
                {
                    apiTemplateResource.Properties.Path = api.Suffix + $"/{Guid.NewGuid():n}";
                }
                else
                {
                    apiTemplateResource.Properties.Path = api.Suffix;
                }

                if (!string.IsNullOrEmpty(api.ServiceUrl))
                {
                    apiTemplateResource.Properties.ServiceUrl = this.MakeServiceUrl(api);
                }
            }
            return apiTemplateResource;
        }

        internal static IDictionary<string, string[]> GetApiVersionSets(CreatorParameters creatorConfig)
        {
            var apiVersions = (creatorConfig.ApiVersionSets ?? new List<ApiVersionSetConfig>())
                .ToDictionary(v => v.Id, v => new List<string>())
                ;

            foreach (var api in creatorConfig.Apis.Where(a => !string.IsNullOrEmpty(a.ApiVersionSetId)))
                apiVersions[api.ApiVersionSetId].Add(api.Name)
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

        static bool IsUri(ApiConfig api, out Uri uriResult)
        {
            return
                Uri.TryCreate(api.OpenApiSpec, UriKind.Absolute, out uriResult) &&
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)
                ;
        }

        public static string MakeResourceName(ApiConfig api)
        {
            return $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.Name}')]";
        }

        public string[] CreateProtocols(ApiConfig api)
        {
            string[] protocols;
            if (api.Protocols != null)
            {
                protocols = api.Protocols.Split(", ");
            }
            else
            {
                protocols = new string[1] { "https" };
            }
            return protocols;
        }

        public bool IsSplitAPI(ApiConfig apiConfig)
        {
            // the api needs to be split into multiple templates if the user has supplied a version or version set - deploying swagger related properties at the same time as api version related properties fails, so they must be written and deployed separately
            return apiConfig.ApiVersion != null || apiConfig.ApiVersionSetId != null || apiConfig.AuthenticationSettings != null && apiConfig.AuthenticationSettings.OAuth2 != null && apiConfig.AuthenticationSettings.OAuth2.AuthorizationServerId != null;
        }

        string MakeServiceUrl(ApiConfig api)
        {
            return !string.IsNullOrEmpty(api.ServiceUrl) ? $"[parameters('{api.Name + "-ServiceUrl"}')]" : null;
        }
    }
}
