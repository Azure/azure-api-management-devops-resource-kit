using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.OpenApi.Models;
using Colors.Net;
using System;
using Newtonsoft.Json;

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
            CommandOption outputLocation = this.Option("--outputLocation <outputLocation>", "Template output location", CommandOptionType.SingleValue).IsRequired();
            CommandOption xmlPolicyFile = this.Option("--xmlPolicyFile <xmlPolicyFile>", "XML policy file location", CommandOptionType.SingleValue);
            CommandOption xmlPolicyURL = this.Option("--xmlPolicyURL <xmlPolicyURL>", "XML policy remote url", CommandOptionType.SingleValue);
            CommandOption linked = this.Option("--linked <linked>", "Creates linked templates versus inlined into a single file", CommandOptionType.SingleValue);
            CommandOption path = this.Option("--path <path>", "API path", CommandOptionType.SingleValue);
            CommandOption apiRevision = this.Option("--apiRevision <apiRevision>", "API revision", CommandOptionType.SingleValue);
            CommandOption apiRevisionDescription = this.Option("--apiRevisionDescription <apiVersionSetId>", "Description of the API revision", CommandOptionType.SingleValue);
            CommandOption apiVersion = this.Option("--apiVersion <apiVersion>", "API version", CommandOptionType.SingleValue);
            CommandOption apiVersionDescription = this.Option("--apiVersionDescription <apiVersionSetId>", "Description of the API version", CommandOptionType.SingleValue);
            CommandOption apiVersionSetFile = this.Option("--apiVersionSetFile <apiVersionSetId>", "YAML file with object that follows the ApiVersionSetContractDetails object schema - https://docs.microsoft.com/en-us/azure/templates/microsoft.apimanagement/2018-06-01-preview/service/apis#ApiVersionSetContractDetails", CommandOptionType.SingleValue);
            CommandOption authenticationSettingsFile = this.Option("--authenticationSettingsFile <apiVersionSetId>", "YAML file with object that follows the AuthenticationSettingsContract object schema - https://docs.microsoft.com/en-us/azure/templates/microsoft.apimanagement/2018-06-01-preview/service/apis#AuthenticationSettingsContract", CommandOptionType.SingleValue);
            CommandOption apiVersionSetId = this.Option("--apiVersionSetId <apiVersionSetId>", "API version set id", CommandOptionType.SingleValue);
            CommandOption productIds = this.Option("--productIds <productIds>", "Product ids to associate the API with", CommandOptionType.MultipleValue);

            this.HelpOption();

            this.OnExecute(async () =>
            {
                // ensure required parameters have been passed in
                if ((openAPISpecFile.HasValue() || openAPISpecURL.HasValue()) && outputLocation.HasValue())
                {
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
                        apiRevisionDescription = apiRevisionDescription.Value(),
                        apiVersion = apiVersion.Value(),
                        apiVersionDescription = apiVersionDescription.Value(),
                        apiVersionSetFile = apiVersionSetFile.Value(),
                        apiVersionSetId = apiVersionSetId.Value(),
                        authenticationSettingsFile = authenticationSettingsFile.Value(),
                        productIds = productIds.Values
                    };

                    if (apiVersionSetFile.HasValue() && AttemptAPIVersionSetConversion(cliArguments) != null)
                    {
                        // unable to convert version set argument into object, would cause failure down the line
                        ColoredConsole.Error.WriteLine("Incorrect apiVersionSet object structure");
                        return 0;
                    }
                    else if (authenticationSettingsFile.HasValue() && AttemptAuthenticationSettingsConversion(cliArguments) != null)
                    {
                        // unable to convert version set argument into object, would cause failure down the line
                        ColoredConsole.Error.WriteLine("Incorrect authenticationSettings object structure");
                        return 0;
                    }
                    else
                    {
                        // required parameters have been supplied and versionSet has correct object structure

                        // initialize helper classes
                        OpenAPISpecReader openAPISpecReader = new OpenAPISpecReader();
                        ARMTemplateWriter armTemplateWriter = new ARMTemplateWriter();
                        APITemplateCreator apiTemplateCreator = new APITemplateCreator();
                        TagTemplateCreator tagTemplateCreator = new TagTemplateCreator();

                        // create OpenApiDocument from Open API spec file
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

        public Exception AttemptAPIVersionSetConversion(CLICreatorArguments cliArguments)
        {
            try
            {
                YAMLReader yamlReader = new YAMLReader();
                APITemplateVersionSet versionSet = yamlReader.ConvertYAMLFileToAPIVersionSet(cliArguments.apiVersionSetFile);
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public Exception AttemptAuthenticationSettingsConversion(CLICreatorArguments cliArguments)
        {
            try
            {
                YAMLReader yamlReader = new YAMLReader();
                APITemplateAuthenticationSettings authenticationSettings = yamlReader.ConvertYAMLFileToAuthenticationSettings(cliArguments.authenticationSettingsFile);
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }
}