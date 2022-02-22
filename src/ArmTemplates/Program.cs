using System;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Applications;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
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
                    applicationLogger.LogInformation($"Recognized {GlobalConstants.ExtractName} command...");
                    var extractorCommandApplication = serviceProvider.GetRequiredService<ExtractApplicationCommand>();

                    var extractorParameters = await extractorCommandApplication.ParseInputConfigurationAsync(consoleAppConfiguration);
                    await extractorCommandApplication.ExecuteCommandAsync(extractorParameters);
                },

                async (CreateConsoleAppConfiguration consoleAppConfiguration) =>
                {
                    applicationLogger.LogInformation($"Recognized {GlobalConstants.CreateName} command...");
                    var creatorCommandApplication = serviceProvider.GetRequiredService<CreateApplicationCommand>();

                    var creatorConfig = await creatorCommandApplication.ParseInputConfigurationAsync(consoleAppConfiguration);
                    await creatorCommandApplication.ExecuteCommandAsync(creatorConfig);
                },

                async errors =>
                {
                    applicationLogger.LogError("Azure API Management DevOps Resource toolkit failed with parsing arguments errors.");
                    applicationLogger.LogError("Below is full errors list. Please, check your input");

                    var builder = SentenceBuilder.Create();
                    var errorMessages = HelpText.RenderParsingErrorsTextAsLines(parserResult, builder.FormatError, builder.FormatMutuallyExclusiveSetErrors, 1);

                    foreach (var errorMessage in errorMessages)
                    {
                        applicationLogger.LogError(errorMessage);
                    }
                }
            );
        }

        static IServiceProvider CreateServiceProvider(ILogger logger)
        {
            var services = new ServiceCollection();
            var serilogLogger = logger as Serilog.ILogger;

            services.AddArmTemplatesServices(serilogLogger);
            return services.BuildServiceProvider();
        }

        static ILogger SetupApplicationLoggingToConsole()
        {
            var serilogConsoleLogger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger();

            return new SerilogLoggerFactory(serilogConsoleLogger)
                .CreateLogger<ILogger>();
        }
    }
}