using System;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Applications;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Configurations;
using CommandLine;
using System.Threading.Tasks;
using CommandLine.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var applicationLogger = SetupApplicationLoggingToConsole();
            var serviceProvider = CreateServiceProvider(applicationLogger);

            var commandLineParser = new Parser(parserSettings =>
            {
                parserSettings.CaseSensitive = true;
            });

            var parserResult = commandLineParser.ParseArguments<ExtractorConsoleAppConfiguration, CreateConsoleAppConfiguration>(args);
            
            await parserResult.MapResult(
                async (ExtractorConsoleAppConfiguration consoleAppConfiguration) =>
                {
                    applicationLogger.Information($"Recognized {GlobalConstants.ExtractName} command...");
                    var extractorCommandApplication = serviceProvider.GetRequiredService<ExtractApplicationCommand>();

                    var extractorParameters = await extractorCommandApplication.ParseInputConfigurationAsync(consoleAppConfiguration);
                    await extractorCommandApplication.ExecuteCommandAsync(extractorParameters);
                },

                async (CreateConsoleAppConfiguration consoleAppConfiguration) =>
                {
                    applicationLogger.Information($"Recognized {GlobalConstants.CreateName} command...");
                    var creatorCommandApplication = serviceProvider.GetRequiredService<CreateApplicationCommand>();

                    var creatorConfig = await creatorCommandApplication.ParseInputConfigurationAsync(consoleAppConfiguration);
                    await creatorCommandApplication.ExecuteCommandAsync(creatorConfig);
                },

                async errors =>
                {
                    applicationLogger.Error("Azure API Management DevOps Resource toolkit failed with parsing arguments errors.");
                    applicationLogger.Error("Below is full errors list. Please, check your input");

                    var builder = SentenceBuilder.Create();
                    var errorMessages = HelpText.RenderParsingErrorsTextAsLines(parserResult, builder.FormatError, builder.FormatMutuallyExclusiveSetErrors, 1);

                    foreach (var errorMessage in errorMessages)
                    {
                        applicationLogger.Error(errorMessage);
                    }
                }
            );
        }

        static IServiceProvider CreateServiceProvider(ILogger logger)
        {
            var services = new ServiceCollection();

            services.AddArmTemplatesServices(logger);
            return services.BuildServiceProvider();
        }

        static ILogger SetupApplicationLoggingToConsole() 
            => new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
    }
}