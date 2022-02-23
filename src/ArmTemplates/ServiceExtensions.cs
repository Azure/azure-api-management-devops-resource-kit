// --------------------------------------------------------------------------
//  <copyright file="Startup.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Applications;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Registeres services for ARMTemplate in dependency injection container. Allows to use your own logger
        /// </summary>
        /// <param name="logger">end-user logger interface to log application traces to</param>
        public static void AddArmTemplatesServices(this IServiceCollection services, Serilog.ILogger logger)
        {
            services.AddLogging(builder =>
            {
                builder.AddSerilog(logger);
            });

            SetupCommands(services);
            SetupExecutors(services);
            SetupApiClients(services);

            SetupExtractors(services);
        }

        static void SetupCommands(IServiceCollection services)
        {
            services.AddScoped(typeof(CreateApplicationCommand));
            services.AddScoped(typeof(ExtractApplicationCommand));
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
            services.AddScoped<IProductApiExtractor, ProductApiExtractor>();
            services.AddScoped<IProductExtractor, ProductExtractor>();
            services.AddScoped<IPropertyExtractor, PropertyExtractor>();
            services.AddScoped<ITagApiExtractor, TagApiExtractor>();
            services.AddScoped<ITagExtractor, TagExtractor>();
        }

        static void SetupApiClients(IServiceCollection services)
        {
            services.AddScoped<IPolicyApiClient, PolicyApiClient>();
        }
    }
}