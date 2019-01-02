using McMaster.Extensions.CommandLineUtils;
using Colors.Net;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class CreateCommand : CommandLineApplication
    {
        public CreateCommand()
        {
            this.Name = Constants.CreateName;
            this.Description = Constants.CreateDescription;

            // list command options
            CommandOption configFile = this.Option("--configFile <configFile>", "Config YAML file location", CommandOptionType.SingleValue).IsRequired();

            this.HelpOption();

            this.OnExecute(async () =>
            {
                if (configFile.HasValue())
                {
                    // convert config file to CreatorConfig class
                    YAMLReader yamlReader = new YAMLReader();
                    CreatorConfig creatorConfig = yamlReader.ConvertConfigYAMLToCreatorConfig(configFile.Value());

                    // ensure required parameters have been passed in
                    if (creatorConfig.outputLocation == null)
                    {
                        ColoredConsole.Error.WriteLine("Output location is required");
                    }
                    else if (creatorConfig.version == null)
                    {
                        ColoredConsole.Error.WriteLine("Version is required");
                    }
                    else if (creatorConfig.api == null)
                    {
                        ColoredConsole.Error.WriteLine("API configuration is required");
                    }
                    else if (creatorConfig.api.openApiSpec == null)
                    {
                        ColoredConsole.Error.WriteLine("Open API Spec is required");
                    }
                    else if (creatorConfig.api.suffix == null)
                    {
                        ColoredConsole.Error.WriteLine("API suffix is required");
                    }
                    else
                    {
                        // required parameters have been supplied

                        // initialize helper classes
                        APITemplateCreator apiTemplateCreator = new APITemplateCreator();
                        ARMTemplateWriter armTemplateWriter = new ARMTemplateWriter();

                        // create templates from provided configuration
                        APITemplate apiTemplate = await apiTemplateCreator.CreateAPITemplateAsync(creatorConfig);

                        // write templates to outputLocation
                        armTemplateWriter.WriteAPITemplateToFile(apiTemplate, creatorConfig.outputLocation);
                        ColoredConsole.WriteLine("Templates written to output location");
                    }
                }
                else
                {
                    ColoredConsole.Error.WriteLine("Config file is required");
                }
                return 0;
            });
        }
    }
}