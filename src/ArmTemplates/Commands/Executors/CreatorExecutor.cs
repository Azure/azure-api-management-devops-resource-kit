// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators.Abstractions;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors
{
    public class CreatorExecutor
    {
        ILogger<CreatorExecutor> logger;
        CreatorParameters creatorParameters;
        FileReader fileReader;

        readonly IApiTemplateCreator apiTemplateCreator;
        readonly IApiVersionSetTemplateCreator apiVersionSetTemplateCreator;
        readonly IAuthorizationServerTemplateCreator authorizationServerTemplateCreator;
        readonly IBackendTemplateCreator backendTemplateCreator;
        readonly IDiagnosticTemplateCreator diagnosticTemplateCreator;
        readonly ILoggerTemplateCreator loggerTemplateCreator;
        readonly IMasterTemplateCreator masterTemplateCreator;
        readonly IPolicyTemplateCreator policyTemplateCreator;
        readonly IProductApiTemplateCreator productAPITemplateCreator;
        readonly IProductGroupTemplateCreator productGroupTemplateCreator;
        readonly IProductTemplateCreator productTemplateCreator;
        readonly IPropertyTemplateCreator propertyTemplateCreator;
        readonly IReleaseTemplateCreator releaseTemplateCreator;
        readonly ISubscriptionTemplateCreator subscriptionTemplateCreator;
        readonly ITagApiTemplateCreator tagAPITemplateCreator;
        readonly ITagTemplateCreator tagTemplateCreator;

        public CreatorExecutor(
            ILogger<CreatorExecutor> logger,
            FileReader fileReader,
            IApiTemplateCreator apiTemplateCreator,
            IApiVersionSetTemplateCreator apiVersionSetTemplateCreator,
            IAuthorizationServerTemplateCreator authorizationServerTemplateCreator,
            IBackendTemplateCreator backendTemplateCreator,
            IDiagnosticTemplateCreator diagnosticTemplateCreator,
            ILoggerTemplateCreator loggerTemplateCreator,
            IMasterTemplateCreator masterTemplateCreator,
            IPolicyTemplateCreator policyTemplateCreator,
            IProductApiTemplateCreator productAPITemplateCreator,
            IProductGroupTemplateCreator productGroupTemplateCreator,
            IProductTemplateCreator productTemplateCreator,
            IPropertyTemplateCreator propertyTemplateCreator,
            IReleaseTemplateCreator releaseTemplateCreator,
            ISubscriptionTemplateCreator subscriptionTemplateCreator,
            ITagApiTemplateCreator tagAPITemplateCreator,
            ITagTemplateCreator tagTemplateCreator)
        {
            this.logger = logger;
            this.fileReader = fileReader;

            this.apiTemplateCreator = apiTemplateCreator;
            this.apiVersionSetTemplateCreator = apiVersionSetTemplateCreator;
            this.authorizationServerTemplateCreator = authorizationServerTemplateCreator;
            this.backendTemplateCreator = backendTemplateCreator;
            this.diagnosticTemplateCreator = diagnosticTemplateCreator;
            this.loggerTemplateCreator = loggerTemplateCreator;
            this.masterTemplateCreator = masterTemplateCreator;
            this.policyTemplateCreator = policyTemplateCreator;
            this.productAPITemplateCreator = productAPITemplateCreator;
            this.productGroupTemplateCreator = productGroupTemplateCreator;
            this.productTemplateCreator = productTemplateCreator;
            this.propertyTemplateCreator = propertyTemplateCreator;
            this.releaseTemplateCreator = releaseTemplateCreator;
            this.subscriptionTemplateCreator = subscriptionTemplateCreator;
            this.tagAPITemplateCreator = tagAPITemplateCreator;
            this.tagTemplateCreator = tagTemplateCreator;
        }

        public static CreatorExecutor BuildCreatorExecutor(
            ILogger<CreatorExecutor> logger,
            FileReader fileReader,
            IApiTemplateCreator apiTemplateCreator = null,
            IApiVersionSetTemplateCreator apiVersionSetTemplateCreator = null,
            IAuthorizationServerTemplateCreator authorizationServerTemplateCreator = null,
            IBackendTemplateCreator backendTemplateCreator = null,
            IDiagnosticTemplateCreator diagnosticTemplateCreator = null,
            ILoggerTemplateCreator loggerTemplateCreator = null,
            IMasterTemplateCreator masterTemplateCreator = null,
            IPolicyTemplateCreator policyTemplateCreator = null,
            IProductApiTemplateCreator productAPITemplateCreator = null,
            IProductGroupTemplateCreator productGroupTemplateCreator = null,
            IProductTemplateCreator productTemplateCreator = null,
            IPropertyTemplateCreator propertyTemplateCreator = null,
            IReleaseTemplateCreator releaseTemplateCreator = null,
            ISubscriptionTemplateCreator subscriptionTemplateCreator = null,
            ITagApiTemplateCreator tagAPITemplateCreator = null,
            ITagTemplateCreator tagTemplateCreator = null)
        => new CreatorExecutor(
            logger,
            fileReader,
            apiTemplateCreator,
            apiVersionSetTemplateCreator,
            authorizationServerTemplateCreator,
            backendTemplateCreator,
            diagnosticTemplateCreator,
            loggerTemplateCreator,
            masterTemplateCreator,
            policyTemplateCreator,
            productAPITemplateCreator,
            productGroupTemplateCreator,
            productTemplateCreator,
            propertyTemplateCreator,
            releaseTemplateCreator,
            subscriptionTemplateCreator,
            tagAPITemplateCreator,
            tagTemplateCreator);

        public void SetCreatorParameters(CreatorParameters creatorParameters)
        {
            this.creatorParameters = creatorParameters;
        }

        public async Task ExecuteGenerationBasedOnConfiguration()
        {
            if (this.creatorParameters is null)
            {
                throw new System.Exception();
            }

            if (!Directory.Exists(this.creatorParameters.OutputLocation))
            {
                Directory.CreateDirectory(this.creatorParameters.OutputLocation);
            }

            // create templates from provided configuration
            this.logger.LogInformation("Creating global service policy template");
            this.logger.LogInformation("------------------------------------------");
            var globalServicePolicyTemplate = this.creatorParameters.Policy != null ? this.policyTemplateCreator.CreateGlobalServicePolicyTemplate(this.creatorParameters) : null;
            this.logger.LogInformation("Creating API version set template");
            this.logger.LogInformation("------------------------------------------");
            var apiVersionSetsTemplate = this.creatorParameters.ApiVersionSets != null ? this.apiVersionSetTemplateCreator.CreateAPIVersionSetTemplate(this.creatorParameters) : null;
            this.logger.LogInformation("Creating product template");
            this.logger.LogInformation("------------------------------------------");
            var productsTemplate = this.creatorParameters.Products != null ? this.productTemplateCreator.CreateProductTemplate(this.creatorParameters) : null;
            this.logger.LogInformation("Creating product/APIs template");
            this.logger.LogInformation("------------------------------------------");
            var productAPIsTemplate = this.creatorParameters.Products != null && this.creatorParameters.Apis != null ? this.productAPITemplateCreator.CreateProductAPITemplate(this.creatorParameters) : null;
            this.logger.LogInformation("Creating named values template");
            this.logger.LogInformation("------------------------------------------");
            var propertyTemplate = this.creatorParameters.NamedValues != null ? this.propertyTemplateCreator.CreatePropertyTemplate(this.creatorParameters) : null;
            this.logger.LogInformation("Creating logger template");
            this.logger.LogInformation("------------------------------------------");
            var loggersTemplate = this.creatorParameters.Loggers != null ? this.loggerTemplateCreator.CreateLoggerTemplate(this.creatorParameters) : null;
            this.logger.LogInformation("Creating backend template");
            this.logger.LogInformation("------------------------------------------");
            var backendsTemplate = this.creatorParameters.Backends != null ? this.backendTemplateCreator.CreateBackendTemplate(this.creatorParameters) : null;
            this.logger.LogInformation("Creating authorization server template");
            this.logger.LogInformation("------------------------------------------");
            var authorizationServersTemplate = this.creatorParameters.AuthorizationServers != null ? this.authorizationServerTemplateCreator.CreateAuthorizationServerTemplate(this.creatorParameters) : null;

            // store name and whether the api will depend on the version set template each api necessary to build linked templates
            var apiInformation = new List<LinkedMasterTemplateAPIInformation>();
            var apiTemplates = new List<Template>();
            this.logger.LogInformation("Creating API templates");
            this.logger.LogInformation("------------------------------------------");

            IDictionary<string, string[]> apiVersions = ApiTemplateCreator.GetApiVersionSets(this.creatorParameters);

            foreach (ApiConfig api in this.creatorParameters.Apis)
            {
                if (this.creatorParameters.ConsiderAllApiForDeployments || this.creatorParameters.PreferredApis.Contains(api.Name))
                {
                    bool isServiceUrlParameterizeInYml = false;
                    if (this.creatorParameters.ServiceUrlParameters != null && this.creatorParameters.ServiceUrlParameters.Count > 0)
                    {
                        isServiceUrlParameterizeInYml = this.creatorParameters.ServiceUrlParameters.Any(s => s.ApiName.Equals(api.Name));
                        api.ServiceUrl = isServiceUrlParameterizeInYml ?
                            this.creatorParameters.ServiceUrlParameters.Where(s => s.ApiName.Equals(api.Name)).FirstOrDefault().ServiceUrl : api.ServiceUrl;
                    }
                    // create api templates from provided api config - if the api config contains a supplied apiVersion, split the templates into 2 for metadata and swagger content, otherwise create a unified template
                    List<Template> apiTemplateSet = await this.apiTemplateCreator.CreateAPITemplatesAsync(api);
                    apiTemplates.AddRange(apiTemplateSet);
                    // create the relevant info that will be needed to properly link to the api template(s) from the master template
                    apiInformation.Add(new LinkedMasterTemplateAPIInformation()
                    {
                        name = api.Name,
                        isSplit = this.apiTemplateCreator.IsSplitAPI(api),
                        dependsOnGlobalServicePolicies = this.creatorParameters.Policy != null,
                        dependsOnVersionSets = api.ApiVersionSetId != null,
                        dependsOnVersion = this.masterTemplateCreator.GetDependsOnPreviousApiVersion(api, apiVersions),
                        dependsOnProducts = api.Products != null,
                        dependsOnTags = api.Tags != null,
                        dependsOnLoggers = await this.masterTemplateCreator.DetermineIfAPIDependsOnLoggerAsync(api, this.fileReader),
                        dependsOnAuthorizationServers = api.AuthenticationSettings != null && api.AuthenticationSettings.OAuth2 != null && api.AuthenticationSettings.OAuth2.AuthorizationServerId != null,
                        dependsOnBackends = await this.masterTemplateCreator.DetermineIfAPIDependsOnBackendAsync(api, this.fileReader),
                        isServiceUrlParameterize = isServiceUrlParameterizeInYml
                    });
                }
            }

            var tagTemplate = await this.GenerateTagsTemplateAsync();

            // create parameters file
            var templateParameters = this.masterTemplateCreator.CreateMasterTemplateParameterValues(this.creatorParameters);

            // write templates to outputLocation
            if (this.creatorParameters.Linked == true)
            {
                // create linked master template
                var masterTemplate = this.masterTemplateCreator.CreateLinkedMasterTemplate(
                    this.creatorParameters, 
                    globalServicePolicyTemplate, 
                    apiVersionSetsTemplate, 
                    productsTemplate, 
                    productAPIsTemplate, 
                    propertyTemplate, 
                    loggersTemplate, 
                    backendsTemplate, 
                    authorizationServersTemplate, 
                    tagTemplate, 
                    apiInformation, 
                    this.creatorParameters.FileNames, 
                    this.creatorParameters.ApimServiceName);

                FileWriter.WriteJSONToFile(masterTemplate, Path.Combine(this.creatorParameters.OutputLocation, this.creatorParameters.FileNames.LinkedMaster));
            }

            foreach (var apiTemplate in apiTemplates)
            {
                var apiResource = apiTemplate.Resources.FirstOrDefault(resource => resource.Type == ResourceTypeConstants.API) as APITemplateResource;
                ApiConfig providedAPIConfiguration = this.creatorParameters.Apis.FirstOrDefault(api => string.Compare(apiResource.Name, ApiTemplateCreator.MakeResourceName(api), true) == 0);
                // if the api version is not null the api is split into multiple templates. If the template is split and the content value has been set, then the template is for a subsequent api
                string apiFileName = FileNameGenerator.GenerateCreatorAPIFileName(providedAPIConfiguration.Name, this.apiTemplateCreator.IsSplitAPI(providedAPIConfiguration), apiResource.Properties.Value != null);
                FileWriter.WriteJSONToFile(apiTemplate, Path.Combine(this.creatorParameters.OutputLocation, apiFileName));
            }
            if (globalServicePolicyTemplate != null)
            {
                FileWriter.WriteJSONToFile(globalServicePolicyTemplate, Path.Combine(this.creatorParameters.OutputLocation, this.creatorParameters.FileNames.GlobalServicePolicy));
            }
            if (apiVersionSetsTemplate != null)
            {
                FileWriter.WriteJSONToFile(apiVersionSetsTemplate, Path.Combine(this.creatorParameters.OutputLocation, this.creatorParameters.FileNames.ApiVersionSets));
            }
            if (productsTemplate != null)
            {
                FileWriter.WriteJSONToFile(productsTemplate, Path.Combine(this.creatorParameters.OutputLocation, this.creatorParameters.FileNames.Products));
            }
            if (productAPIsTemplate != null)
            {
                FileWriter.WriteJSONToFile(productAPIsTemplate, Path.Combine(this.creatorParameters.OutputLocation, this.creatorParameters.FileNames.ProductAPIs));
            }
            if (propertyTemplate != null)
            {
                FileWriter.WriteJSONToFile(propertyTemplate, Path.Combine(this.creatorParameters.OutputLocation, this.creatorParameters.FileNames.NamedValues));
            }
            if (loggersTemplate != null)
            {
                FileWriter.WriteJSONToFile(loggersTemplate, Path.Combine(this.creatorParameters.OutputLocation, this.creatorParameters.FileNames.Loggers));
            }
            if (backendsTemplate != null)
            {
                FileWriter.WriteJSONToFile(backendsTemplate, Path.Combine(this.creatorParameters.OutputLocation, this.creatorParameters.FileNames.Backends));
            }
            if (authorizationServersTemplate != null)
            {
                FileWriter.WriteJSONToFile(authorizationServersTemplate, Path.Combine(this.creatorParameters.OutputLocation, this.creatorParameters.FileNames.AuthorizationServers));
            }
            if (tagTemplate != null)
            {
                FileWriter.WriteJSONToFile(tagTemplate, Path.Combine(this.creatorParameters.OutputLocation, this.creatorParameters.FileNames.Tags));
            }

            // write parameters to outputLocation
            FileWriter.WriteJSONToFile(templateParameters, Path.Combine(this.creatorParameters.OutputLocation, this.creatorParameters.FileNames.Parameters));
            this.logger.LogInformation("Templates written to output location");
        }

        public async Task<Template> GenerateTagsTemplateAsync()
        {
            if (this.creatorParameters.Tags.IsNullOrEmpty() && this.creatorParameters.Apis.All(x => x.Tags.IsNullOrEmpty()))
            {
                return null;    
            }

            this.logger.LogInformation("Creating tag template");
            
            var tagTemplate = this.tagTemplateCreator.CreateTagTemplate(this.creatorParameters);
            await FileWriter.SaveAsJsonAsync(tagTemplate, this.creatorParameters.OutputLocation, this.creatorParameters.FileNames.Tags);

            return tagTemplate;
        }
    }
}
