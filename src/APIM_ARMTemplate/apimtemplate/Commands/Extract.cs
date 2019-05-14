using System;
using McMaster.Extensions.CommandLineUtils;
using System.Collections.Generic;
using System.IO;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System.Linq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class ExtractCommand : CommandLineApplication
    {
        public ExtractCommand()
        {
            this.Name = GlobalConstants.ExtractName;
            this.Description = GlobalConstants.ExtractDescription;

            var apiManagementName = this.Option("--name <apimname>", "API Management name", CommandOptionType.SingleValue);
            var resourceGroupName = this.Option("--resourceGroup <resourceGroup>", "Resource Group name", CommandOptionType.SingleValue);
            var fileFolderName = this.Option("--fileFolder <filefolder>", "ARM Template files folder", CommandOptionType.SingleValue);
            var apiName = this.Option("--apiName <apiName>", "API name", CommandOptionType.SingleValue);
            var linkedBaseUrlName = this.Option("--linkedBaseUrl <apiName>", "Creates a master template with links", CommandOptionType.SingleValue);

            this.HelpOption();

            this.OnExecute(async () =>
            {
                if (!apiManagementName.HasValue()) throw new Exception("Missing parameter <apimname>.");
                if (!resourceGroupName.HasValue()) throw new Exception("Missing parameter <resourceGroup>.");
                if (!fileFolderName.HasValue()) throw new Exception("Missing parameter <filefolder>.");

                // isolate cli parameters
                string resourceGroup = resourceGroupName.Values[0].ToString();
                string apimname = apiManagementName.Values[0].ToString();
                string fileFolder = fileFolderName.Values[0].ToString();
                string linkedBaseUrl = linkedBaseUrlName.Values[0].ToString();
                string singleApiName = null;

                if (apiName.Values.Count > 0)
                {
                    singleApiName = apiName.Values[0].ToString();
                }

                Console.WriteLine("API Management Template");
                Console.WriteLine();
                Console.WriteLine("Connecting to {0} API Management Service on {1} Resource Group ...", apimname, resourceGroup);
                if (singleApiName != null)
                {
                    Console.WriteLine("Executing extraction for {0} API ...", singleApiName);
                }
                else
                {
                    Console.WriteLine("Executing full extraction ...", singleApiName);
                }

                // initialize file helper classes
                FileWriter fileWriter = new FileWriter();
                FileNameGenerator fileNameGenerator = new FileNameGenerator();
                FileNames fileNames = fileNameGenerator.GenerateFileNames(apimname);

                // initialize entity extractor classes
                APIExtractor apiExtractor = new APIExtractor();
                APIVersionSetExtractor apiVersionSetExtractor = new APIVersionSetExtractor();
                AuthorizationServerExtractor authorizationServerExtractor = new AuthorizationServerExtractor();
                BackendExtractor backendExtractor = new BackendExtractor();
                LoggerExtractor loggerExtractor = new LoggerExtractor();
                PropertyExtractor propertyExtractor = new PropertyExtractor();
                ProductExtractor productExtractor = new ProductExtractor();
                MasterTemplateExtractor masterTemplateExtractor = new MasterTemplateExtractor();

                // extract templates from apim service
                Template apiTemplate = await apiExtractor.GenerateAPIsARMTemplate(apimname, resourceGroup, fileFolder, singleApiName);
                List<TemplateResource> apiTemplateResources = apiTemplate.resources.ToList();
                Template apiVersionSetTemplate = await apiVersionSetExtractor.GenerateAPIVersionSetsARMTemplate(apimname, resourceGroup, singleApiName, apiTemplateResources);
                Template authorizationServerTemplate = await authorizationServerExtractor.GenerateAuthorizationServersARMTemplate(apimname, resourceGroup, singleApiName, apiTemplateResources);
                Template loggerTemplate = await loggerExtractor.GenerateLoggerTemplate(apimname, resourceGroup, singleApiName, apiTemplateResources);
                Template productTemplate = await productExtractor.GenerateProductsARMTemplate(apimname, resourceGroup, singleApiName, apiTemplateResources);
                Template namedValueTemplate = await propertyExtractor.GenerateNamedValuesTemplate(apimname, resourceGroup, singleApiName, apiTemplateResources);
                List<TemplateResource> namedValueResources = namedValueTemplate.resources.ToList();
                Template backendTemplate = await backendExtractor.GenerateBackendsARMTemplate(apimname, resourceGroup, singleApiName, apiTemplateResources, namedValueResources);

                // create parameters file
                Template templateParameters = masterTemplateExtractor.CreateMasterTemplateParameterValues(apimname, linkedBaseUrl);

                // write templates to output file location
                string apiFileName = fileNameGenerator.GenerateExtractorAPIFileName(singleApiName, apimname);
                fileWriter.WriteJSONToFile(apiTemplate, String.Concat(@fileFolder, apiFileName));
                fileWriter.WriteJSONToFile(apiVersionSetTemplate, String.Concat(@fileFolder, fileNames.apiVersionSets));
                fileWriter.WriteJSONToFile(authorizationServerTemplate, String.Concat(@fileFolder, fileNames.authorizationServers));
                fileWriter.WriteJSONToFile(backendTemplate, String.Concat(@fileFolder, fileNames.backends));
                fileWriter.WriteJSONToFile(loggerTemplate, String.Concat(@fileFolder, fileNames.loggers));
                fileWriter.WriteJSONToFile(namedValueTemplate, String.Concat(@fileFolder, fileNames.namedValues));
                fileWriter.WriteJSONToFile(productTemplate, String.Concat(@fileFolder, fileNames.products));

                if (linkedBaseUrl != null)
                {
                    // create a master template that links to all other templates
                    Template masterTemplate = masterTemplateExtractor.GenerateLinkedMasterTemplate(apiTemplate, apiVersionSetTemplate, productTemplate, loggerTemplate, backendTemplate, authorizationServerTemplate, namedValueTemplate, fileNames);
                    fileWriter.WriteJSONToFile(masterTemplate, String.Concat(@fileFolder, fileNames.linkedMaster));
                }

                // write parameters to outputLocation
                fileWriter.WriteJSONToFile(templateParameters, String.Concat(fileFolder, fileNames.parameters));
                Console.WriteLine("Templates written to output location");
                Console.WriteLine("Press any key to exit process:");
#if DEBUG
                Console.ReadKey();
#endif
            });
        }
    }
}
