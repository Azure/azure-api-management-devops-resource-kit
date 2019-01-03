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
                // convert config file to CreatorConfig class
                YAMLReader yamlReader = new YAMLReader();
                CreatorConfig creatorConfig = yamlReader.ConvertConfigYAMLToCreatorConfig(configFile.Value());

                // ensure required parameters have been passed in
                if (creatorConfig.outputLocation == null)
                {
                    throw new CommandParsingException(this, "Output location is required");
                }
                else if (creatorConfig.version == null)
                {
                    throw new CommandParsingException(this, "Version is required");
                }
                else if (creatorConfig.api == null)
                {
                    throw new CommandParsingException(this, "API configuration is required");
                }
                else if (creatorConfig.api.openApiSpec == null)
                {
                    throw new CommandParsingException(this, "Open API Spec is required");
                }
                else if (creatorConfig.api.suffix == null)
                {
                    throw new CommandParsingException(this, "API suffix is required");
                }
                else
                {
                    // required parameters have been supplied

                    // initialize helper classes
                    APIVersionSetTemplateCreator apiVersionSetTemplateCreator = new APIVersionSetTemplateCreator();
                    APITemplateCreator apiTemplateCreator = new APITemplateCreator();
                    ARMTemplateWriter armTemplateWriter = new ARMTemplateWriter();

                    // create templates from provided configuration
                    APIVersionSetTemplate apiVersionSetTemplate = creatorConfig.apiVersionSet != null ? apiVersionSetTemplateCreator.CreateAPIVersionSetTemplate(creatorConfig) : null;
                    APITemplate apiTemplate = await apiTemplateCreator.CreateAPITemplateAsync(creatorConfig);

                    // write templates to outputLocation
                    armTemplateWriter.WriteAPIVersionSetTemplateToFile(apiVersionSetTemplate, creatorConfig.outputLocation);
                    armTemplateWriter.WriteAPITemplateToFile(apiTemplate, creatorConfig.outputLocation);
                    ColoredConsole.WriteLine("Templates written to output location");
                }
                return 0;
            });
        }
    }
}