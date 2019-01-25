using System;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class ExtractCommand : CommandLineApplication
    {
        public ExtractCommand()
        {
            this.Name = Constants.ExtractName;
            this.Description = Constants.ExtractDescription;

            var apiManagementName = this.Option("--name <apimname>", "API Management name", CommandOptionType.SingleValue);
            var resourceGroupName = this.Option("--resourceGroup <resourceGroup>", "Resource Group name", CommandOptionType.SingleValue);

            this.HelpOption();

            this.OnExecute(async () =>
            {
                if (!apiManagementName.HasValue()) throw new Exception("Missing parameter <apimname>.");
                if (!resourceGroupName.HasValue()) throw new Exception("Missing parameter <resourceGroup>.");

                string resourceGroup = resourceGroupName.Values[0].ToString();
                string apimname = apiManagementName.Values[0].ToString();
                Api api = new Api();
                string apis = api.GetAPIs(apimname, resourceGroup).Result;

                ExtractedAPI extractedAPI = JsonConvert.DeserializeObject<ExtractedAPI>(apis);
                Console.WriteLine("{0} API's found!", extractedAPI.value.Count.ToString());

                for (int i = 0; i < extractedAPI.value.Count; i++)
                {
                    APIConfig apiConfig = new APIConfig();

                    CreatorConfig creatorConfig = new CreatorConfig
                    {
                        version = "1.0.0",
                        outputLocation = @"",
                        apimServiceName = apimname,
                        api = apiConfig
                    };
                    creatorConfig.api.openApiSpec = null;
                    creatorConfig.api.name = extractedAPI.value[i].name;
                    creatorConfig.api.apiVersion = extractedAPI.value[i].properties.apiVersion;
                    creatorConfig.api.apiVersionDescription = extractedAPI.value[i].properties.apiVersionDescription;
                    creatorConfig.api.suffix = extractedAPI.value[i].properties.path;
                    creatorConfig.linked = false;

                    if (extractedAPI.value[i].properties.apiVersionSetId != null)
                    {
                        string APIVersionSetFull = extractedAPI.value[i].properties.apiVersionSetId;
                        string APIVersionSetId = APIVersionSetFull.Substring(APIVersionSetFull.LastIndexOf('/') + 1);
                        APIVersionSetId = api.GetAPIVersionSet(apimname, resourceGroup, APIVersionSetId).Result;
                        APIVersionSetTemplateResource apiv = JsonConvert.DeserializeObject<APIVersionSetTemplateResource>(APIVersionSetId);

                        creatorConfig.apiVersionSet = apiv.properties;
                    }

                    TemplateCreator templateCreator = new TemplateCreator();
                    APIVersionSetTemplateCreator apiVersionSetTemplateCreator = new APIVersionSetTemplateCreator(templateCreator);
                    ProductAPITemplateCreator productAPITemplateCreator = new ProductAPITemplateCreator();
                    //PolicyTemplateCreator policyTemplateCreator = new PolicyTemplateCreator(fileReader);
                    APITemplateCreator apiTemplateCreator = new APITemplateCreator(templateCreator);
                    //MasterTemplateCreator masterTemplateCreator = new MasterTemplateCreator(templateCreator);

                    // create templates from provided configuration
                    Template apiVersionSetTemplate = creatorConfig.apiVersionSet != null ? apiVersionSetTemplateCreator.CreateAPIVersionSetTemplate(creatorConfig) : null;
                    Template apiTemplate = await apiTemplateCreator.CreateAPITemplateAsync(creatorConfig);

                    FileWriter fileWriter = new FileWriter();
                    CreatorFileNames creatorFileNames = fileWriter.GenerateCreatorFileNames();

                    if (extractedAPI.value[i].properties.apiVersionSetId != null)
                    {
                        Console.WriteLine("Writing API Version Set File for {0} API ...", extractedAPI.value[i].name);
                        fileWriter.WriteJSONToFile(apiVersionSetTemplate, String.Concat(creatorConfig.outputLocation, extractedAPI.value[i].name, "-", creatorFileNames.apiVersionSet));
                    }
                    else Console.WriteLine("{0} has no API version set.", extractedAPI.value[i].name);

                    Console.WriteLine("Writing API File for {0} API ...", extractedAPI.value[i].name);
                    fileWriter.WriteJSONToFile(apiTemplate, String.Concat(creatorConfig.outputLocation, extractedAPI.value[i].name, "-", creatorFileNames.api));
                }
                Console.WriteLine("All files are saved on {0} folder! Press any key.", Directory.GetCurrentDirectory());
                Console.ReadKey();
                return 0;
            });
        }
    }
}