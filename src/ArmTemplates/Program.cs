// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Applications;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Configurations;
using CommandLine;
using System.Threading.Tasks;
using CommandLine.Text;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using System.Linq;

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
                parserSettings.HelpWriter = Console.Out;
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

                errors => {
                    applicationLogger.Information("Azure API Management DevOps Resource toolkit finished.");

                    if (!errors.IsNullOrEmpty())
                    {
                        var errorList = errors.ToList();

                        // write descriptive message for specific error-tags
                        if (errorList.Count == 1)
                        {
                            var singleError = errorList.First();

                            switch (singleError.Tag)
                            {
                                case ErrorType.VersionRequestedError:
                                case ErrorType.HelpRequestedError:
                                    return Task.CompletedTask;
                                case ErrorType.HelpVerbRequestedError:
                                    applicationLogger.Error("No verb found. Use \"help\" command to view all supported commands.");
                                    break;

                                default:
                                    applicationLogger.Error("Azure API Management DevOps Resource toolkit non-expected error occured.");
                                    break;
                            }
                        }

                        var builder = SentenceBuilder.Create();
                        var errorMessages = HelpText.RenderParsingErrorsTextAsLines(parserResult, builder.FormatError, builder.FormatMutuallyExclusiveSetErrors, 0).ToList();

                        for (var i = 0; i < errorList.Count; i++)
                        {
                            var errorTag = errorList[i].Tag;
                            var errorMessage = (i < errorMessages.Count) ? errorMessages[i] : string.Empty;

                            applicationLogger.Error("[{0}] {1}", errorList[i].Tag, errorMessage);
                        }
                    }

                    return Task.CompletedTask;
                });
        }

        public static IServiceProvider CreateServiceProvider(ILogger logger)
        {
            var services = new ServiceCollection();

            services.AddArmTemplatesServices(logger);
            return services.BuildServiceProvider();
        }

        public static ILogger SetupApplicationLoggingToConsole() 
            => new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
    }
}