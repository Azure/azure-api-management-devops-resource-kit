using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.OpenApi.Models;
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
            CommandOption openAPISpecFile = this.Option("--openAPISpecFile <openAPISpecFile>", "Open API spec file location", CommandOptionType.SingleValue);
            CommandOption openAPISpecURL = this.Option("--openAPISpecURL <openAPISpecURL>", "Open API spec remote url", CommandOptionType.SingleValue);
            CommandOption outputLocation = this.Option("--outputLocation <outputLocation>", "Template output location", CommandOptionType.SingleValue);
            CommandOption xmlPolicyFile = this.Option("--xmlPolicyFile <xmlPolicyFile>", "XML policy file location", CommandOptionType.SingleValue);
            CommandOption xmlPolicyURL = this.Option("--xmlPolicyURL <xmlPolicyURL>", "XML policy remote url", CommandOptionType.SingleValue);
            CommandOption linked = this.Option("--linked <linked>", "Creates linked templates versus inlined into a single file", CommandOptionType.SingleValue);
            CommandOption path = this.Option("--path <path>", "API path", CommandOptionType.SingleValue);
            CommandOption apiRevision = this.Option("--apiRevision <apiRevision>", "API revision", CommandOptionType.SingleValue);
            CommandOption apiVersion = this.Option("--apiVersion <apiVersion>", "API version", CommandOptionType.SingleValue);
            CommandOption apiVersionSetId = this.Option("--apiVersionSetId <apiVersionSetId>", "API version set id", CommandOptionType.SingleValue);
            CommandOption productIds = this.Option("--productIds <productIds>", "Product ids to associate the API with", CommandOptionType.MultipleValue);

            // convert command options to CLIArguments class
            CLICreatorArguments cliArguments = new CLICreatorArguments()
            {
                openAPISpecFile = openAPISpecFile.Value(),
                openAPISpecURL = openAPISpecURL.Value(),
                outputLocation = outputLocation.Value(),
                xmlPolicyFile = xmlPolicyFile.Value(),
                xmlPolicyURL = xmlPolicyURL.Value(),
                linked = linked.HasValue(),
                path = path.Value(),
                apiRevision = apiRevision.Value(),
                apiVersion = apiVersion.Value(),
                apiVersionSetId = apiVersionSetId.Value(),
                productIds = productIds.Values
            };
            
            this.HelpOption();

            this.OnExecute(async () =>
            {
                if ((openAPISpecFile.HasValue() || openAPISpecURL.HasValue()) && outputLocation.HasValue())
                {
                    // initialize helper classes
                    OpenAPISpecReader openAPISpecReader = new OpenAPISpecReader();
                    ARMTemplateWriter armTemplateWriter = new ARMTemplateWriter();
                    APITemplateCreator apiTemplateCreator = new APITemplateCreator();
                    TagTemplateCreator tagTemplateCreator = new TagTemplateCreator();

                    // create OpenApiDocument
                    OpenApiDocument doc = new OpenApiDocument();
                    if (cliArguments.openAPISpecFile != null)
                    {
                        doc = openAPISpecReader.ConvertLocalFileToOpenAPISpec(cliArguments.openAPISpecFile);
                    }
                    else
                    {
                        doc = await openAPISpecReader.ConvertRemoteURLToOpenAPISpecAsync(cliArguments.openAPISpecURL);
                    }

                    // create templates from OpenApiDocument
                    APITemplate apiTemplate = await apiTemplateCreator.CreateAPITemplateAsync(doc, cliArguments);
                    List<TagTemplate> tagTemplates = tagTemplateCreator.CreateTagTemplates(doc);
                    List<TagDescriptionTemplate> tagDescriptionTemplates = tagTemplateCreator.CreateTagDescriptionTemplates(doc);

                    // write templates to outputLocation
                    armTemplateWriter.WriteAPITemplateToFile(apiTemplate, cliArguments.outputLocation);
                    armTemplateWriter.WriteTagTemplatesToFile(tagTemplates, cliArguments.outputLocation);
                    armTemplateWriter.WriteTagDescriptionTemplatesToFile(tagDescriptionTemplates, cliArguments.outputLocation);
                    ColoredConsole.WriteLine("Templates written to output location");
                }
                else if (!outputLocation.HasValue())
                {
                    ColoredConsole.Error.WriteLine("Output location is required");
                }
                else if (!(openAPISpecFile.HasValue() || openAPISpecURL.HasValue()))
                {
                    ColoredConsole.Error.WriteLine("Open API spec file or remote url is required");
                };
                return 0;
            });
        }
    }
}