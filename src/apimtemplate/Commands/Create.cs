using System;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.HelpText;
using Colors.Net;

namespace APIManagement.Template
{
    public class CreateCommand : CommandLineApplication
    {
        public CreateCommand()
        {
            this.Name = Constants.CreateName;
            this.Description = Constants.CreateDescription;

            var filename = this.Option("--filename <filename>", "File name", CommandOptionType.SingleValue).IsRequired();
            this.HelpOption();

            this.OnExecute(() =>
            {
                if (filename.HasValue())
                    Console.WriteLine($"Create command executed with filename {filename.Value()}");
                else
                    ColoredConsole.Error.WriteLine("No file name passed in");
                return 0;
            });
        }
    }
}