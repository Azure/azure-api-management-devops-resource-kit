using System;
using McMaster.Extensions.CommandLineUtils;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class ExtractCommand : CommandLineApplication
    {
        public ExtractCommand()
        {
            this.Name = GlobalConstants.ExtractName;
            this.Description = GlobalConstants.ExtractDescription;

            var sourceApimName = this.Option("--sourceApimName <sourceApimName>", "Source API Management name", CommandOptionType.SingleValue);
            var destinationAPIManagementName = this.Option("--destinationApimName <destinationApimName>", "Destination API Management name", CommandOptionType.SingleValue);
            var resourceGroupName = this.Option("--resourceGroup <resourceGroup>", "Resource Group name", CommandOptionType.SingleValue);
            var fileFolderName = this.Option("--fileFolder <filefolder>", "ARM Template files folder", CommandOptionType.SingleValue);
            var apiName = this.Option("--apiName <apiName>", "API name", CommandOptionType.SingleValue);
            var linkedTemplatesBaseUrlName = this.Option("--linkedTemplatesBaseUrl <linkedTemplatesBaseUrl>", "Creates a master template with links", CommandOptionType.SingleValue);
            var linkedTemplatesUrlQueryString = this.Option("--linkedTemplatesUrlQueryString <linkedTemplatesUrlQueryString>", "Query string appended to linked templates uris that enables retrieval from private storage", CommandOptionType.SingleValue);
            var policyXMLBaseUrlName = this.Option("--policyXMLBaseUrl <policyXMLBaseUrl>", "Writes policies to local XML files that require deployment to remote folder", CommandOptionType.SingleValue);
            var splitAPITemplates = this.Option("--splitAPIs <splitAPIs>", "Split APIs into multiple templates", CommandOptionType.SingleValue);

            this.HelpOption();

            this.OnExecute(async () =>
            {
                try
                {
                    if (!sourceApimName.HasValue()) throw new Exception("Missing parameter <sourceApimName>.");
                    if (!destinationAPIManagementName.HasValue()) throw new Exception("Missing parameter <destinationApimName>.");
                    if (!resourceGroupName.HasValue()) throw new Exception("Missing parameter <resourceGroup>.");
                    if (!fileFolderName.HasValue()) throw new Exception("Missing parameter <filefolder>.");

                    // check if specify "split apis" and use "single api" at the same time
                    string splitAPIs = splitAPITemplates.HasValue() ? splitAPITemplates.Value().ToString() : null;
                    if (splitAPIs != null && splitAPIs.Equals("true") && apiName.Values.Count > 0)
                    {
                        throw new Exception("Can't use splitAPIs when you extract single API");
                    }

                    // isolate cli parameters
                    string resourceGroup = resourceGroupName.Value().ToString();
                    string sourceApim = sourceApimName.Value().ToString();
                    string destinationApim = destinationAPIManagementName.Value().ToString();
                    string dirName = fileFolderName.Value().ToString();
                    string linkedBaseUrl = linkedTemplatesBaseUrlName.HasValue() ? linkedTemplatesBaseUrlName.Value().ToString() : null;
                    string linkedUrlQueryString = linkedTemplatesUrlQueryString.HasValue() ? linkedTemplatesUrlQueryString.Value().ToString() : null;
                    string policyXMLBaseUrl = policyXMLBaseUrlName.HasValue() ? policyXMLBaseUrlName.Value().ToString() : null;
                    string singleApiName = null;

                    if (apiName.Values.Count > 0)
                    {
                        singleApiName = apiName.Value().ToString();
                    }

                    Console.WriteLine("API Management Template");
                    Console.WriteLine();
                    Console.WriteLine("Connecting to {0} API Management Service on {1} Resource Group ...", sourceApim, resourceGroup);
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
                    FileNames fileNames = fileNameGenerator.GenerateFileNames(sourceApim);

                    // create template folder with all apis and split api templates
                    if (splitAPIs != null && splitAPIs.Equals("true"))
                    {
                        APIExtractor apiExtractor = new APIExtractor(fileWriter);
                        // pull all apis from service
                        string apis = await apiExtractor.GetAPIsAsync(sourceApim, resourceGroup);
                        JObject oApi = JObject.Parse(apis);

                        // Generate folders based on all apiversionset
                        var apiDictionary = new Dictionary<string, List<string>>();
                        for (int i = 0; i < ((JContainer)oApi["value"]).Count; i++)
                        {
                            string apiDisplayName = ((JValue)oApi["value"][i]["properties"]["displayName"]).Value.ToString();
                            if (!apiDictionary.ContainsKey(apiDisplayName))
                            {
                                List<string> apiVersionSet = new List<string>();
                                apiVersionSet.Add(((JValue)oApi["value"][i]["name"]).Value.ToString());
                                apiDictionary[apiDisplayName] = apiVersionSet;
                            }
                            else
                            {
                                apiDictionary[apiDisplayName].Add(((JValue)oApi["value"][i]["name"]).Value.ToString());
                            }
                        }

                        // Generate templates based on each API/APIversionSet
                        foreach (KeyValuePair<string, List<string>> versionSetEntry in apiDictionary)
                        {
                            string apiFileFolder = dirName;

                            // Check if it's APIVersionSet
                            if (versionSetEntry.Value.Count > 1)
                            {
                                // this API has VersionSet
                                string apiDisplayName = versionSetEntry.Key;

                                // create apiVersionSet folder
                                apiFileFolder = String.Concat(@apiFileFolder, $@"/{apiDisplayName}");
                                System.IO.Directory.CreateDirectory(apiFileFolder);

                                // create master templates for each apiVersionSet
                                string versionSetFolder = String.Concat(@apiFileFolder, fileNames.versionSetMasterFolder);
                                System.IO.Directory.CreateDirectory(versionSetFolder);
                                await this.GenerateTemplates(sourceApim, destinationApim, null, versionSetEntry.Value, resourceGroup, policyXMLBaseUrl, versionSetFolder, linkedBaseUrl, linkedUrlQueryString, fileNameGenerator, fileNames, fileWriter);

                                Console.WriteLine($@"Finish extracting APIVersionSet {versionSetEntry.Key}");
                            }

                            // Generate templates
                            foreach (string apiName in versionSetEntry.Value)
                            {
                                // create folder for each API
                                string tempFileFolder = String.Concat(@apiFileFolder, $@"/{apiName}");
                                System.IO.Directory.CreateDirectory(tempFileFolder);

                                // generate templates for each API
                                await this.GenerateTemplates(sourceApim, destinationApim, apiName, null, resourceGroup, policyXMLBaseUrl, tempFileFolder, linkedBaseUrl, linkedUrlQueryString, fileNameGenerator, fileNames, fileWriter);

                                Console.WriteLine($@"Finish extracting API {apiName}");
                            }
                        }
                    }
                    else
                    {
                        await this.GenerateTemplates(sourceApim, destinationApim, singleApiName, null, resourceGroup, policyXMLBaseUrl, dirName, linkedBaseUrl, linkedUrlQueryString, fileNameGenerator, fileNames, fileWriter);
                    }
                    Console.WriteLine("Templates written to output location");
                    Console.WriteLine("Press any key to exit process:");
#if DEBUG
                    Console.ReadKey();
#endif
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error occured: " + ex.Message);
                    throw;
                }
            });
        }
        private async Task<Boolean> GenerateTemplates(string sourceApim, string destinationApim, string singleApiName, List<string> multipleApiNams, string resourceGroup, string policyXMLBaseUrl, string dirName, string linkedBaseUrl, string linkedUrlQueryString, FileNameGenerator fileNameGenerator, FileNames fileNames, FileWriter fileWriter)
        {
            // initialize entity extractor classes
            APIExtractor apiExtractor = new APIExtractor(fileWriter);
            APIVersionSetExtractor apiVersionSetExtractor = new APIVersionSetExtractor();
            AuthorizationServerExtractor authorizationServerExtractor = new AuthorizationServerExtractor();
            BackendExtractor backendExtractor = new BackendExtractor();
            LoggerExtractor loggerExtractor = new LoggerExtractor();
            PolicyExtractor policyExtractor = new PolicyExtractor(fileWriter);
            PropertyExtractor propertyExtractor = new PropertyExtractor();
            TagExtractor tagExtractor = new TagExtractor();
            ProductExtractor productExtractor = new ProductExtractor(fileWriter);
            MasterTemplateExtractor masterTemplateExtractor = new MasterTemplateExtractor();

            // extract templates from apim service
            Template globalServicePolicyTemplate = await policyExtractor.GenerateGlobalServicePolicyTemplateAsync(sourceApim, resourceGroup, policyXMLBaseUrl, dirName);
            Template apiTemplate = await apiExtractor.GenerateAPIsARMTemplateAsync(sourceApim, resourceGroup, singleApiName, multipleApiNams, policyXMLBaseUrl, dirName);
            List<TemplateResource> apiTemplateResources = apiTemplate.resources.ToList();
            Template apiVersionSetTemplate = await apiVersionSetExtractor.GenerateAPIVersionSetsARMTemplateAsync(sourceApim, resourceGroup, singleApiName, apiTemplateResources, policyXMLBaseUrl);
            Template authorizationServerTemplate = await authorizationServerExtractor.GenerateAuthorizationServersARMTemplateAsync(sourceApim, resourceGroup, singleApiName, apiTemplateResources, policyXMLBaseUrl);
            Template loggerTemplate = await loggerExtractor.GenerateLoggerTemplateAsync(sourceApim, resourceGroup, singleApiName, apiTemplateResources, policyXMLBaseUrl);
            Template productTemplate = await productExtractor.GenerateProductsARMTemplateAsync(sourceApim, resourceGroup, singleApiName, apiTemplateResources, policyXMLBaseUrl, dirName);
            List<TemplateResource> productTemplateResources = productTemplate.resources.ToList();
            Template namedValueTemplate = await propertyExtractor.GenerateNamedValuesTemplateAsync(sourceApim, resourceGroup, singleApiName, apiTemplateResources, policyXMLBaseUrl);
            Template tagTemplate = await tagExtractor.GenerateTagsTemplateAsync(sourceApim, resourceGroup, singleApiName, apiTemplateResources, productTemplateResources, policyXMLBaseUrl);
            List<TemplateResource> namedValueResources = namedValueTemplate.resources.ToList();
            Template backendTemplate = await backendExtractor.GenerateBackendsARMTemplateAsync(sourceApim, resourceGroup, singleApiName, apiTemplateResources, namedValueResources, policyXMLBaseUrl);

            // create parameters file
            Template templateParameters = masterTemplateExtractor.CreateMasterTemplateParameterValues(destinationApim, linkedBaseUrl, linkedUrlQueryString, policyXMLBaseUrl);

            // write templates to output file location
            string apiFileName = fileNameGenerator.GenerateExtractorAPIFileName(singleApiName, sourceApim);
            fileWriter.WriteJSONToFile(apiTemplate, String.Concat(@dirName, apiFileName));
            fileWriter.WriteJSONToFile(apiVersionSetTemplate, String.Concat(@dirName, fileNames.apiVersionSets));
            fileWriter.WriteJSONToFile(authorizationServerTemplate, String.Concat(@dirName, fileNames.authorizationServers));
            fileWriter.WriteJSONToFile(backendTemplate, String.Concat(@dirName, fileNames.backends));
            fileWriter.WriteJSONToFile(loggerTemplate, String.Concat(@dirName, fileNames.loggers));
            fileWriter.WriteJSONToFile(namedValueTemplate, String.Concat(@dirName, fileNames.namedValues));
            fileWriter.WriteJSONToFile(tagTemplate, String.Concat(@dirName, fileNames.tags));
            fileWriter.WriteJSONToFile(productTemplate, String.Concat(@dirName, fileNames.products));
            fileWriter.WriteJSONToFile(globalServicePolicyTemplate, String.Concat(@dirName, fileNames.globalServicePolicy));

            if (linkedBaseUrl != null)
            {
                // create a master template that links to all other templates
                Template masterTemplate = masterTemplateExtractor.GenerateLinkedMasterTemplate(apiTemplate, globalServicePolicyTemplate, apiVersionSetTemplate, productTemplate, loggerTemplate, backendTemplate, authorizationServerTemplate, namedValueTemplate, tagTemplate, fileNames, apiFileName, linkedUrlQueryString, policyXMLBaseUrl);
                fileWriter.WriteJSONToFile(masterTemplate, String.Concat(@dirName, fileNames.linkedMaster));
            }

            // write parameters to outputLocation
            fileWriter.WriteJSONToFile(templateParameters, String.Concat(dirName, fileNames.parameters));
            return true;
        }
    }
}
