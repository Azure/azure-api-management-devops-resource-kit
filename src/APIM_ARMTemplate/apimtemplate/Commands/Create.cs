using McMaster.Extensions.CommandLineUtils;
using Colors.Net;
using System;
using System.Collections.Generic;

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
                FileReader fileReader = new FileReader();
                CreatorConfig creatorConfig = await fileReader.ConvertConfigYAMLToCreatorConfigAsync(configFile.Value());

                // ensure required parameters have been passed in
                if (creatorConfig.outputLocation == null)
                {
                    throw new CommandParsingException(this, "Output location is required");
                }
                else if (creatorConfig.version == null)
                {
                    throw new CommandParsingException(this, "Version is required");
                }
                else if (creatorConfig.apimServiceName == null)
                {
                    throw new CommandParsingException(this, "APIM service name is required");
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
                    FileNameGenerator fileNameGenerator = new FileNameGenerator();
                    TemplateCreator templateCreator = new TemplateCreator();
                    APIVersionSetTemplateCreator apiVersionSetTemplateCreator = new APIVersionSetTemplateCreator(templateCreator);
                    ProductAPITemplateCreator productAPITemplateCreator = new ProductAPITemplateCreator();
                    PolicyTemplateCreator policyTemplateCreator = new PolicyTemplateCreator(fileReader);
                    APITemplateCreator apiTemplateCreator = new APITemplateCreator(fileReader, templateCreator, policyTemplateCreator, productAPITemplateCreator);
                    MasterTemplateCreator masterTemplateCreator = new MasterTemplateCreator(templateCreator, fileNameGenerator);
                    ARMTemplateWriter armTemplateWriter = new ARMTemplateWriter();

                    // create templates from provided configuration
                    CreatorFileNames creatorFileNames = fileNameGenerator.GenerateCreatorFileNames();
                    Template apiVersionSetTemplate = creatorConfig.apiVersionSet != null ? apiVersionSetTemplateCreator.CreateAPIVersionSetTemplate(creatorConfig) : null;
                    Template apiTemplate = await apiTemplateCreator.CreateAPITemplateAsync(creatorConfig);
                    if(creatorConfig.linked == true)
                    {
                        Template masterTemplate = masterTemplateCreator.CreateLinkedMasterTemplate(apiVersionSetTemplate, apiTemplate, creatorFileNames);
                        Template masterTemplateParameters = masterTemplateCreator.CreateMasterTemplateParameterValues(creatorConfig);

                        // write templates to outputLocation
                        if (apiVersionSetTemplate != null)
                        {
                            armTemplateWriter.WriteJSONToFile(apiVersionSetTemplate, String.Concat(creatorConfig.outputLocation, @"/linked/", creatorFileNames.apiVersionSet));
                        }
                        armTemplateWriter.WriteJSONToFile(apiTemplate, String.Concat(creatorConfig.outputLocation, @"/linked/", creatorFileNames.api));
                        armTemplateWriter.WriteJSONToFile(masterTemplate, String.Concat(creatorConfig.outputLocation, @"/linked/", "master.template.json"));
                        armTemplateWriter.WriteJSONToFile(masterTemplateParameters, String.Concat(creatorConfig.outputLocation, @"/linked/", "master.parameters.json"));
                    } else
                    {
                        Template masterTemplate = masterTemplateCreator.CreateUnlinkedMasterTemplate(apiVersionSetTemplate, apiTemplate, creatorFileNames);
                        Template masterTemplateParameters = masterTemplateCreator.CreateMasterTemplateParameterValues(creatorConfig);
                        armTemplateWriter.WriteJSONToFile(masterTemplate, String.Concat(creatorConfig.outputLocation, @"/unlinked/", "master.template.json"));
                        armTemplateWriter.WriteJSONToFile(masterTemplateParameters, String.Concat(creatorConfig.outputLocation, @"/unlinked/", "master.parameters.json"));
                    }

                    ColoredConsole.WriteLine("Templates written to output location");
                }
                return 0;
            });
        }
    }
}