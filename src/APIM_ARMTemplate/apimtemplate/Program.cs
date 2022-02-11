using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Serilog;
using Serilog.Extensions.Logging;
using apimtemplate.Common;
using Microsoft.Extensions.Logging;

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
                app.Commands.Add(new CreateCommand());
                app.Commands.Add(new ExtractCommand());
                
                app.OnExecute(() => {
                    Logger.LogError("No commands specified, please specify a command");
                    app.ShowHelp();
                    return 1;
                });

                return app.Execute(args);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "apimtemplate application finished with an error...");
                return 1;
            }
        }

        private static void SetupApplicationLoggingToConsole()
        {
            var serilogConsoleLogger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger();

            var microsoftLogger = new SerilogLoggerFactory(serilogConsoleLogger)
                .CreateLogger<Microsoft.Extensions.Logging.ILogger>();

            Logger.SetupLogger(microsoftLogger);
        }
    }
}