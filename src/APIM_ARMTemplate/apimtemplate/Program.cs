using System;
using Colors.Net;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract;

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
                var app = new CommandLineApplication() 
                {
                    Name = Constants.AppShortName,
                    FullName = Constants.AppLongName,
                    Description = Constants.AppDescription
                };
                
                app.HelpOption(inherited: true);
                app.Commands.Add(new CreateCommand());
                app.Commands.Add(new ExtractCommand());
                
                app.OnExecute(() => {
                    ColoredConsole.Error.WriteLine("No commands specified, please specify a command");
                    app.ShowHelp();
                    return 1;
                });
                return app.Execute(args);
            }
            catch (Exception e)
            {
                ColoredConsole.Error.WriteLine(e.Message);
                return 1;
            }
        }
    }
}