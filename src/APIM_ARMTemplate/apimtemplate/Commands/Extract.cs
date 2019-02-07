using System;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.HelpText;
using Colors.Net;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class ExtractCommand : CommandLineApplication
    {
        public ExtractCommand()
        {
            this.Name = Constants.ExtractName;
            this.Description = Constants.ExtractDescription;

            var apimname = this.Option("--name <apimname>", "API Management name", CommandOptionType.SingleValue).IsRequired();
            this.HelpOption();

            this.OnExecute(() =>
            {
                if (apimname.HasValue())
                    Console.WriteLine($"Create command executed with name {apimname.Value()}");
                else
                    ColoredConsole.Error.WriteLine("API Management name passed in");
                return 0;
            });
        }
    }
}