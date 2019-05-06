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
                string singleApiName = null;

                if (apiName.Values.Count > 0)
                {
                    singleApiName = apiName.Values[0].ToString();
                }

                Console.WriteLine("API Management Template");
                Console.WriteLine();
                Console.WriteLine("Connecting to {0} API Management Service on {1} Resource Group ...", apimname, resourceGroup);

                // initialize entity extractor classes
                FileWriter fileWriter = new FileWriter();
                APIExtractor apiExtractor = new APIExtractor();
                AuthorizationServerExtractor authorizationServerExtractor = new AuthorizationServerExtractor();
                BackendExtractor backendExtractor = new BackendExtractor();
                LoggerExtractor loggerExtractor = new LoggerExtractor();
                PropertyExtractor propertyExtractor = new PropertyExtractor();
                ProductExtractor productExtractor = new ProductExtractor();

                // extract templates from apim service
                Template apiTemplate = await apiExtractor.GenerateAPIsARMTemplate(apimname, resourceGroup, fileFolder, singleApiName);
                List<TemplateResource> apiTemplateResources = apiTemplate.resources.ToList();
                Template authorizationTemplate = await authorizationServerExtractor.GenerateAuthorizationServersARMTemplate(apimname, resourceGroup, singleApiName, apiTemplateResources);
                Template loggerTemplate = await loggerExtractor.GenerateLoggerTemplate(apimname, resourceGroup, singleApiName, apiTemplateResources);
                Template productTemplate = await productExtractor.GenerateProductsARMTemplate(apimname, resourceGroup, singleApiName, apiTemplateResources);
                Template namedValueTemplate = await propertyExtractor.GenerateNamedValuesTemplate(apimname, resourceGroup, singleApiName, apiTemplateResources);
                List<TemplateResource> namedValueResources = namedValueTemplate.resources.ToList();
                Template backendTemplate = await backendExtractor.GenerateBackendsARMTemplate(apimname, resourceGroup, singleApiName, apiTemplateResources, namedValueResources);

                // write templates to output file location
                string apiFileName = singleApiName == null ? @fileFolder + Path.DirectorySeparatorChar + apimname + "-apis-template.json" : @fileFolder + Path.DirectorySeparatorChar + apimname + "-" + singleApiName + "-api-template.json";
                fileWriter.WriteJSONToFile(apiTemplate, apiFileName);
                fileWriter.WriteJSONToFile(authorizationTemplate, @fileFolder + Path.DirectorySeparatorChar + apimname + "-authorizationServers.json");
                fileWriter.WriteJSONToFile(backendTemplate, @fileFolder + Path.DirectorySeparatorChar + apimname + "-backends.json");
                fileWriter.WriteJSONToFile(loggerTemplate, @fileFolder + Path.DirectorySeparatorChar + apimname + "-loggers.json");
                fileWriter.WriteJSONToFile(namedValueTemplate, @fileFolder + Path.DirectorySeparatorChar + apimname + "-namedValues.json");
                fileWriter.WriteJSONToFile(productTemplate, @fileFolder + Path.DirectorySeparatorChar + apimname + "-products.json");

                Console.WriteLine("Templates written to output location");
                Console.WriteLine("Press any key to exit process:");
#if DEBUG
                Console.ReadKey();
#endif
            });
        }
    }
}
