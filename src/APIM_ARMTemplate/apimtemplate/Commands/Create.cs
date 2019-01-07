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
                    TemplateCreator templateCreator = new TemplateCreator();
                    APIVersionSetTemplateCreator apiVersionSetTemplateCreator = new APIVersionSetTemplateCreator(templateCreator);
                    APITemplateCreator apiTemplateCreator = new APITemplateCreator(templateCreator, fileReader);
                    PolicyTemplateCreator policyTemplateCreator = new PolicyTemplateCreator(templateCreator, fileReader);
                    ARMTemplateWriter armTemplateWriter = new ARMTemplateWriter();

                    // create templates from provided configuration
                    Template apiVersionSetTemplate = creatorConfig.apiVersionSet != null ? apiVersionSetTemplateCreator.CreateAPIVersionSetTemplate(creatorConfig) : null;
                    Template initialAPITemplate = apiTemplateCreator.CreateInitialAPITemplateAsync(creatorConfig);
                    Template subsequentAPITemplate = await apiTemplateCreator.CreateSubsequentAPITemplate(creatorConfig);
                    Template apiPolicyTemplate = creatorConfig.api.policy != null ? await policyTemplateCreator.CreateAPIPolicyAsync(creatorConfig) : null;
                    List<Template> operationPolicyTemplates = await policyTemplateCreator.CreateOperationPolicies(creatorConfig);

                    // write templates to outputLocation
                    if (apiVersionSetTemplate != null)
                    {
                        armTemplateWriter.WriteJSONToFile(apiVersionSetTemplate, String.Concat(creatorConfig.outputLocation, @"\APIVersionSetTemplate.json"));
                    }
                    armTemplateWriter.WriteJSONToFile(initialAPITemplate, String.Concat(creatorConfig.outputLocation, @"\InitialAPITemplate.json"));
                    armTemplateWriter.WriteJSONToFile(subsequentAPITemplate, String.Concat(creatorConfig.outputLocation, @"\SubsequentAPITemplate.json"));
                    if (apiPolicyTemplate != null)
                    {
                        armTemplateWriter.WriteJSONToFile(apiPolicyTemplate, String.Concat(creatorConfig.outputLocation, @"\APIPolicyTemplate.json"));
                    }
                    if (operationPolicyTemplates.Count > 0)
                    {
                        int count = 0;
                        foreach (Template operationPolicyTemplate in operationPolicyTemplates)
                        {
                            armTemplateWriter.WriteJSONToFile(operationPolicyTemplate, String.Concat(creatorConfig.outputLocation, $@"\OperationPolicyTemplate-{++count}.json"));
                        }
                    }
                    ColoredConsole.WriteLine("Templates written to output location");
                }
                return 0;
            });
        }
    }
}