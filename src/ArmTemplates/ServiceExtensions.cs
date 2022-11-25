// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Applications;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.ApiOperations;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.ApiSchemas;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.ApiRevision;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.ApiVersionSet;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.AuthorizationServer;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Diagnostics;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Groups;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Product;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Tags;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Gateway;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Loggers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.NamedValues;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Backend;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.IdentityProviders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.ApiManagementService;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Schemas;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.OpenIdConnectProviders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.PolicyFragments;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Registeres services for ARMTemplate in dependency injection container. Allows to use your own logger
        /// </summary>
        /// <param name="logger">end-user logger interface to log application traces to</param>
        public static void AddArmTemplatesServices(this IServiceCollection services, ILogger logger)
        {
            services.AddLogging(builder =>
            {
                builder.AddSerilog(logger);
            });

            services.AddScoped<FileReader>();
            services.AddHttpClient();
            services.RemoveAll<IHttpMessageHandlerBuilderFilter>();
            SetupCommands(services);
            SetupExecutors(services);
            SetupApiClients(services);
            SetupBuilders(services);
            SetupExtractors(services);
            SetupCreators(services);
            SetupDataProcessors(services);
        }

        static void SetupDataProcessors(IServiceCollection services)
        {
            services.AddScoped<IGroupDataProcessor, GroupDataProcessor>();
            services.AddScoped<IProductDataProcessor, ProductDataProcessor>();
            services.AddScoped<IProductApiDataProcessor, ProductApiDataProcessor>();
            services.AddScoped<IApiDataProcessor, ApiDataProcessor>();
            services.AddScoped<IApiOperationDataProcessor, ApiOperationDataProcessor>();
            services.AddScoped<INamedValuesDataProcessor, NamedValuesDataProcessor>();
            services.AddScoped(typeof(ITemplateResourceDataProcessor<>), typeof(TemplateResourceDataProcessor<>));
        }

        static void SetupCommands(IServiceCollection services)
        {
            services.AddScoped(typeof(CreateApplicationCommand));
            services.AddScoped(typeof(ExtractApplicationCommand));
        }

        static void SetupBuilders(IServiceCollection services)
        {
            services.AddSingleton<ITemplateBuilder, TemplateBuilder>();
        }

        static void SetupExecutors(IServiceCollection services)
        {
            services.AddScoped(typeof(ExtractorExecutor));
            services.AddScoped(typeof(CreatorExecutor));
        }

        static void SetupCreators(IServiceCollection services)
        {
            services.AddScoped<IApiTemplateCreator, ApiTemplateCreator>();
            services.AddScoped<IApiVersionSetTemplateCreator, ApiVersionSetTemplateCreator>();
            services.AddScoped<IAuthorizationServerTemplateCreator, AuthorizationServerTemplateCreator>();
            services.AddScoped<IBackendTemplateCreator, BackendTemplateCreator>();
            services.AddScoped<IDiagnosticTemplateCreator, DiagnosticTemplateCreator>();
            services.AddScoped<ILoggerTemplateCreator, LoggerTemplateCreator>();
            services.AddScoped<IMasterTemplateCreator, MasterTemplateCreator>();
            services.AddScoped<IPolicyTemplateCreator, PolicyTemplateCreator>();
            services.AddScoped<IProductApiTemplateCreator, ProductApiTemplateCreator>();
            services.AddScoped<IProductGroupTemplateCreator, ProductGroupTemplateCreator>();
            services.AddScoped<IProductTemplateCreator, ProductTemplateCreator>();
            services.AddScoped<IPropertyTemplateCreator, PropertyTemplateCreator>();
            services.AddScoped<IReleaseTemplateCreator, ReleaseTemplateCreator>();
            services.AddScoped<ISubscriptionTemplateCreator, SubscriptionTemplateCreator>();
            services.AddScoped<ITagApiTemplateCreator, TagApiTemplateCreator>();
            services.AddScoped<ITagTemplateCreator, TagTemplateCreator>();
        }

        static void SetupExtractors(IServiceCollection services)
        {
            services.AddScoped<IApiExtractor, ApiExtractor>();
            services.AddScoped<IApiVersionSetExtractor, ApiVersionSetExtractor>();
            services.AddScoped<IAuthorizationServerExtractor, AuthorizationServerExtractor>();
            services.AddScoped<IBackendExtractor, BackendExtractor>();
            services.AddScoped<ILoggerExtractor, LoggerExtractor>();
            services.AddScoped<IMasterTemplateExtractor, MasterTemplateExtractor>();
            services.AddScoped<IPolicyExtractor, PolicyExtractor>();
            services.AddScoped<IProductApisExtractor, ProductApisExtractor>();
            services.AddScoped<IProductExtractor, ProductExtractor>();
            services.AddScoped<INamedValuesExtractor, NamedValuesExtractor>();
            services.AddScoped<ITagApiExtractor, TagApiExtractor>();
            services.AddScoped<ITagExtractor, TagExtractor>();
            services.AddScoped<IGroupExtractor, GroupExtractor>();
            services.AddScoped<IApiSchemaExtractor, ApiSchemaExtractor>();
            services.AddScoped<IApiOperationExtractor, ApiOperationExtractor>();
            services.AddScoped<IDiagnosticExtractor, DiagnosticExtractor>();
            services.AddScoped<IAuthorizationServerExtractor, AuthorizationServerExtractor>();
            services.AddScoped<IApiRevisionExtractor, ApiRevisionExtractor>();
            services.AddScoped<IGatewayExtractor, GatewayExtractor>();
            services.AddScoped<IGatewayApiExtractor, GatewayApiExtractor>();
            services.AddScoped<IParametersExtractor, ParametersExtractor>();
            services.AddScoped<IIdentityProviderExtractor, IdentityProviderExtractor>();
            services.AddScoped<IApiManagementServiceExtractor, ApiManagementServiceExtractor>();
            services.AddScoped<ISchemaExtractor, SchemaExtractor>();
            services.AddScoped<IOpenIdConnectProviderExtractor, OpenIdConnectProviderExtractor>();
            services.AddScoped<IPolicyFragmentsExtractor, PolicyFragmentsExtractor>();
            services.AddScoped<IApiReleaseExtractor, ApiReleaseExtractor>();
        }

        static void SetupApiClients(IServiceCollection services)
        {
            services.AddScoped<IApiOperationClient, ApiOperationClient>();
            services.AddScoped<IApiSchemaClient, ApiSchemaClient>();
            services.AddScoped<IApiRevisionClient, ApiRevisionClient>();
            services.AddScoped<IAuthorizationServerClient, AuthorizationServerClient>();
            services.AddScoped<IApiVersionSetClient, ApiVersionSetClient>();
            services.AddScoped<IApisClient, ApisClient>();
            services.AddScoped<IGroupsClient, GroupsClient>();
            services.AddScoped<ITagClient, TagClient>();
            services.AddScoped<IPolicyClient, PolicyClient>();
            services.AddScoped<IProductsClient, ProductsClient>();
            services.AddScoped<IDiagnosticClient, DiagnosticClient>();
            services.AddScoped<IGatewayClient, GatewayClient>();
            services.AddScoped<ILoggerClient, LoggerClient>();
            services.AddScoped<INamedValuesClient, NamedValuesClient>();
            services.AddScoped<IBackendClient, BackendClient>();
            services.AddScoped<IIdentityProviderClient, IdentityProviderClient>();
            services.AddScoped<IApiManagementServiceClient, ApiManagementServiceClient>();
            services.AddScoped<ISchemasClient, SchemasClient>();
            services.AddScoped<IOpenIdConnectProvidersClient, OpenIdConnectProviderClient>();
            services.AddScoped<IPolicyFragmentsClient, PolicyFragmentsClient>();
        }
    }
}