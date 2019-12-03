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

            this.HelpOption();

            this.OnExecute(async () =>
            {
                // convert config file to extractorConfig class
                FileReader fileReader = new FileReader();
                ExtractorConfig extractorConfig = fileReader.ConvertConfigJsonToExtractorConfig();
                
                try
                {
                    if (extractorConfig.sourceApimName == null) throw new Exception("Missing parameter <sourceApimName>.");
                    if (extractorConfig.destinationApimName == null) throw new Exception("Missing parameter <destinationApimName>.");
                    if (extractorConfig.resourceGroup == null) throw new Exception("Missing parameter <resourceGroup>.");
                    if (extractorConfig.fileFolder == null) throw new Exception("Missing parameter <filefolder>.");

                    bool splitAPIs = extractorConfig.splitAPIs != null && extractorConfig.splitAPIs.Equals("true");
                    string apiVersionSetName = extractorConfig.apiVersionSetName;
                    string singleApiName = extractorConfig.apiName;

                    // validaion check
                    if (splitAPIs && singleApiName != null)
                    {
                        throw new Exception("Can't use --splitAPIs and --apiName at same time");
                    }

                    if (splitAPIs && apiVersionSetName != null)
                    {
                        throw new Exception("Can't use --splitAPIs and --apiVersionSetName at same time");
                    }

                    if (singleApiName != null && apiVersionSetName != null)
                    {
                        throw new Exception("Can't use --apiName and --apiVersionSetName at same time");
                    }

                    // isolate cli parameters
                    string resourceGroup = extractorConfig.resourceGroup;
                    string sourceApim = extractorConfig.sourceApimName;
                    string destinationApim = extractorConfig.destinationApimName;
                    string dirName = extractorConfig.fileFolder;
                    string linkedBaseUrl = extractorConfig.linkedTemplatesBaseUrl;
                    string linkedUrlQueryString = extractorConfig.linkedTemplatesUrlQueryString;
                    string policyXMLBaseUrl = extractorConfig.policyXMLBaseUrl;
                    bool includeRevisions = extractorConfig.includeAllRevisions != null && extractorConfig.includeAllRevisions.Equals("true");

                    Console.WriteLine("API Management Template");
                    Console.WriteLine();
                    Console.WriteLine("Connecting to {0} API Management Service on {1} Resource Group ...", sourceApim, resourceGroup);
                    if (singleApiName != null)
                    {
                        Console.WriteLine("Executing extraction for {0} API ...", singleApiName);
                    }
                    else
                    {
                        Console.WriteLine("Executing full extraction ...");
                    }

                    // initialize file helper classes
                    FileWriter fileWriter = new FileWriter();
                    FileNameGenerator fileNameGenerator = new FileNameGenerator();
                    FileNames fileNames = fileNameGenerator.GenerateFileNames(sourceApim);

                    // create template folder with all apis and split api templates
                    if (splitAPIs)
                    {
                        // create split api templates for all apis in the sourceApim
                        await this.GenerateSplitAPITemplates(new ExtractorConfig(extractorConfig), fileNameGenerator, fileWriter, fileNames);
                    }
                    else if (apiVersionSetName != null)
                    {
                        // create split api templates and aggregated api templates for this apiversionset
                        await this.GenerateAPIVersionSetTemplates(new ExtractorConfig(extractorConfig), fileNameGenerator, fileNames, fileWriter);
                    }
                    else
                    {
                        // create single api template or create aggregated api templates for all apis within the sourceApim
                        await this.GenerateTemplates(new ExtractorConfig(extractorConfig, dirName), fileNameGenerator, fileNames, fileWriter);
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

        /* three condistions to use this function:
            1. singleApiName is null, then generate one master template for the multipleAPIs in multipleApiNams
            2. multipleApiNams is null, then generate separate folder and master template for each API 
            3. when both singleApiName and multipleApiNams is null, then generate one master template to link all apis in the sourceapim
        */
        private async Task GenerateTemplates(ExtractorConfig exc, FileNameGenerator fileNameGenerator, FileNames fileNames, FileWriter fileWriter)
        {
            if (exc.apiName != null && exc.multipleAPINames != null)
            {
                throw new Exception("can't specify single API and multiple APIs to extract at the same time");
            }
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

            // read parameters
            string sourceApim = exc.sourceApimName;
            string resourceGroup = exc.resourceGroup;
            string singleApiName = exc.apiName;
            string destinationApim = exc.destinationApimName;
            string linkedBaseUrl = exc.linkedTemplatesBaseUrl;
            string policyXMLBaseUrl = exc.policyXMLBaseUrl;
            string dirName = exc.fileFolder;
            List<string> multipleApiNames = exc.multipleAPINames;
            string linkedUrlQueryString = exc.linkedTemplatesUrlQueryString;

            // extract templates from apim service
            Template globalServicePolicyTemplate = await policyExtractor.GenerateGlobalServicePolicyTemplateAsync(sourceApim, resourceGroup, policyXMLBaseUrl, dirName);
            Template apiTemplate = await apiExtractor.GenerateAPIsARMTemplateAsync(sourceApim, resourceGroup, singleApiName, multipleApiNames, policyXMLBaseUrl, dirName);
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
            // won't generate template when there is no resources
            if (apiVersionSetTemplate.resources.Count() != 0)
            {
                fileWriter.WriteJSONToFile(apiVersionSetTemplate, String.Concat(@dirName, fileNames.apiVersionSets));
            }
            if (backendTemplate.resources.Count() != 0)
            {
                fileWriter.WriteJSONToFile(backendTemplate, String.Concat(@dirName, fileNames.backends));
            }
            if (authorizationServerTemplate.resources.Count() != 0)
            {
                fileWriter.WriteJSONToFile(authorizationServerTemplate, String.Concat(@dirName, fileNames.authorizationServers));
            }
            if (productTemplate.resources.Count() != 0)
            {
                fileWriter.WriteJSONToFile(productTemplate, String.Concat(@dirName, fileNames.products));
            }
            if (tagTemplate.resources.Count() != 0)
            {
                fileWriter.WriteJSONToFile(tagTemplate, String.Concat(@dirName, fileNames.tags));
            }
            if (namedValueTemplate.resources.Count() != 0)
            {
                fileWriter.WriteJSONToFile(namedValueTemplate, String.Concat(@dirName, fileNames.namedValues));
            }
            if (globalServicePolicyTemplate.resources.Count() != 0)
            {
                fileWriter.WriteJSONToFile(globalServicePolicyTemplate, String.Concat(@dirName, fileNames.globalServicePolicy));
            }
            if (linkedBaseUrl != null)
            {
                // create a master template that links to all other templates
                Template masterTemplate = masterTemplateExtractor.GenerateLinkedMasterTemplate(apiTemplate, globalServicePolicyTemplate, apiVersionSetTemplate, productTemplate, loggerTemplate, backendTemplate, authorizationServerTemplate, namedValueTemplate, tagTemplate, fileNames, apiFileName, linkedUrlQueryString, policyXMLBaseUrl);
                fileWriter.WriteJSONToFile(masterTemplate, String.Concat(@dirName, fileNames.linkedMaster));
            }

            // write parameters to outputLocation
            fileWriter.WriteJSONToFile(templateParameters, String.Concat(dirName, fileNames.parameters));
        }

        // this function will generate master template for each API within this version set and an extra master template to link these apis
        public async Task GenerateAPIVersionSetTemplates(ExtractorConfig exc, FileNameGenerator fileNameGenerator, FileNames fileNames, FileWriter fileWriter)
        {
            // get api dictionary and check api version set
            var apiDictionary = await this.GetAllAPIsDictionary(exc.sourceApimName, exc.resourceGroup, fileWriter);
            if (!apiDictionary.ContainsKey(exc.apiVersionSetName))
            {
                throw new Exception("API Version Set with this name doesn't exist");
            }
            else
            {
                foreach (string apiName in apiDictionary[exc.apiVersionSetName])
                {
                    // generate seperate folder for each API
                    string apiFileFolder = String.Concat(@exc.fileFolder, $@"/{apiName}");
                    System.IO.Directory.CreateDirectory(apiFileFolder);
                    // config instance with singleApiName
                    ExtractorConfig excConfig = new ExtractorConfig(exc, apiName, apiFileFolder);
                    await this.GenerateTemplates(excConfig, fileNameGenerator, fileNames, fileWriter);
                }

                // create master templates for this apiVersionSet 
                string versionSetFolder = String.Concat(@exc.fileFolder, fileNames.versionSetMasterFolder);
                System.IO.Directory.CreateDirectory(versionSetFolder);
                ExtractorConfig versionConfig = new ExtractorConfig(exc, apiDictionary[exc.apiVersionSetName], versionSetFolder);
                await this.GenerateTemplates(versionConfig, fileNameGenerator, fileNames, fileWriter);

                Console.WriteLine($@"Finish extracting APIVersionSet {exc.apiVersionSetName}");

            }
        }

        // this function will generate split api templates / folders for each api in this sourceApim
        public async Task GenerateSplitAPITemplates(ExtractorConfig exc, FileNameGenerator fileNameGenerator, FileWriter fileWriter, FileNames fileNames)
        {
            // Generate folders based on all apiversionset
            var apiDictionary = await this.GetAllAPIsDictionary(exc.sourceApimName, exc.resourceGroup, fileWriter);

            // Generate templates based on each API/APIversionSet
            foreach (KeyValuePair<string, List<string>> versionSetEntry in apiDictionary)
            {
                string apiFileFolder = exc.fileFolder;

                // if it's APIVersionSet, generate the versionsetfolder for templates
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
                    ExtractorConfig masterConfig = new ExtractorConfig(exc, versionSetEntry.Value, versionSetFolder);
                    await this.GenerateTemplates(masterConfig, fileNameGenerator, fileNames, fileWriter);

                    Console.WriteLine($@"Finish extracting APIVersionSet {versionSetEntry.Key}");
                }

                // Generate templates for each api 
                foreach (string apiName in versionSetEntry.Value)
                {
                    // create folder for each API
                    string tempFileFolder = String.Concat(@apiFileFolder, $@"/{apiName}");
                    System.IO.Directory.CreateDirectory(tempFileFolder);
                    ExtractorConfig excConfig = new ExtractorConfig(exc, apiName, tempFileFolder);
                    // generate templates for each API
                    await this.GenerateTemplates(excConfig, fileNameGenerator, fileNames, fileWriter);

                    Console.WriteLine($@"Finish extracting API {apiName}");
                }
            }
        }

        // this function will generate an api dictionary with apiName/versionsetName (if exist one) as key, list of apiNames as value
        public async Task<Dictionary<string, List<string>>> GetAllAPIsDictionary(string sourceApim, string resourceGroup, FileWriter fileWriter)
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
            return apiDictionary;
        }
    }
}
