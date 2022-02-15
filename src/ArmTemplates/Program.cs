using System;
using McMaster.Extensions.CommandLineUtils;
using Serilog;
using Serilog.Extensions.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Applications;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class AppArgs
    {
        public bool create { get; set; }
        public string extract { get; set; }
    }

    class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                SetupApplicationLoggingToConsole();

                var app = new CommandLineApplication()
                {
                    Name = GlobalConstants.AppShortName,
                    FullName = GlobalConstants.AppLongName,
                    Description = GlobalConstants.AppDescription
                };

                app.HelpOption(inherited: true);
                app.Commands.Add(new CreateApplicationCommand());
                app.Commands.Add(new ExtractApplicationCommand());

                app.OnExecute(() =>
                {
                    Logger.LogError("No commands specified, please specify a command");
                    app.ShowHelp();
                    return 1;
                });

                return app.Execute(args);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Azure API Management DevOps Resource toolkit finished with an error...");
                return 1;
            }
        }

        private static void SetupApplicationLoggingToConsole()
        {
            var serilogConsoleLogger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger();

            var microsoftLogger = new SerilogLoggerFactory(serilogConsoleLogger)
                .CreateLogger<Extensions.Logging.ILogger>();

            Logger.SetupLogger(microsoftLogger);
        }
    }
}