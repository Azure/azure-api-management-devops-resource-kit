﻿// --------------------------------------------------------------------------
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

            SetupCommands(services);
            SetupExecutors(services);
            SetupApiClients(services);
            SetupBuilders(services);
            SetupExtractors(services);
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
        }
    }
}