using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Applications;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var applicationLogger = SetupApplicationLoggingToConsole();
            var serviceProvider = CreateServiceProvider(applicationLogger);

            try
            {
                var app = new RootCommandLineApplication(serviceProvider)
                {
                    Name = GlobalConstants.AppShortName,
                    FullName = GlobalConstants.AppLongName,
                    Description = GlobalConstants.AppDescription
                };

                app.HelpOption(inherited: true);
                app.Conventions
                    .UseConstructorInjection(serviceProvider);

                app.OnExecute(() =>
                {
                    applicationLogger.LogInformation("No commands specified, please specify a command...");
                    app.ShowHelp();
                    return 1;
                });

                return app.Execute(args);
            }
            catch (Exception e)
            {
                applicationLogger.LogError(e, "Azure API Management DevOps Resource toolkit finished with an error...");
                return 1;
            }
        }

        static IServiceProvider CreateServiceProvider(ILogger logger)
        {
            var services = new ServiceCollection();
            services.AddArmTemplatesServices(logger);
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