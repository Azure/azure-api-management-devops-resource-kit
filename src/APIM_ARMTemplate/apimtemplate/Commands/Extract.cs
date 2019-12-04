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

                    string singleApiName = extractorConfig.apiName;
                    bool splitAPIs = extractorConfig.splitAPIs != null && extractorConfig.splitAPIs.Equals("true");
                    bool hasVersionSetName = extractorConfig.apiVersionSetName != null;
                    bool hasSingleApi = singleApiName != null;
                    bool includeRevisions = extractorConfig.includeAllRevisions != null && extractorConfig.includeAllRevisions.Equals("true");

                    // validaion check
                    if (splitAPIs && hasSingleApi)
                    {
                        throw new Exception("Can't use splitAPIs and apiName at same time");
                    }

                    if (splitAPIs && hasVersionSetName)
                    {
                        throw new Exception("Can't use splitAPIs and apiVersionSetName at same time");
                    }

                    if (hasSingleApi && hasVersionSetName)
                    {
                        throw new Exception("Can't use apiName and apiVersionSetName at same time");
                    }

                    // start running extractor
                    Console.WriteLine("API Management Template");
                    Console.WriteLine("Connecting to {0} API Management Service on {1} Resource Group ...", extractorConfig.sourceApimName, extractorConfig.resourceGroup);

                    // initialize file helper classes
                    FileWriter fileWriter = new FileWriter();
                    FileNameGenerator fileNameGenerator = new FileNameGenerator();
                    FileNames fileNames = fileNameGenerator.GenerateFileNames(extractorConfig.sourceApimName);

                    if (splitAPIs)
                    {
                        // create split api templates for all apis in the sourceApim
                        await this.GenerateSplitAPITemplates(extractorConfig, fileNameGenerator, fileWriter, fileNames);
                    }
                    else if (hasVersionSetName)
                    {
                        // create split api templates and aggregated api templates for this apiversionset
                        await this.GenerateAPIVersionSetTemplates(extractorConfig, fileNameGenerator, fileNames, fileWriter);
                    }
                    else if (hasSingleApi && includeRevisions)
                    {
                        // handle singel API include Revision extraction
                        await this.GenerateSingleAPIWithRevisionsTemplates(extractorConfig, singleApiName, fileNameGenerator, fileWriter, fileNames);
                    }
                    else
                    {
                        // create single api template or create aggregated api templates for all apis within the sourceApim
                        if (hasSingleApi)
                        {
                            Console.WriteLine("Executing extraction for {0} API ...", singleApiName);
                        }
                        else
                        {
                            Console.WriteLine("Executing full extraction ...");
                        }
                        await this.GenerateTemplates(new Extractor(extractorConfig), singleApiName, null, fileNameGenerator, fileNames, fileWriter, null);
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
        private async Task GenerateTemplates(
            Extractor exc,
            string singleApiName,
            List<string> multipleAPINames,
            FileNameGenerator fileNameGenerator,
            FileNames fileNames,
            FileWriter fileWriter,
            Template apiTemplate)
        {
            if (singleApiName != null && multipleAPINames != null)
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
            string destinationApim = exc.destinationApimName;
            string linkedBaseUrl = exc.linkedTemplatesBaseUrl;
            string policyXMLBaseUrl = exc.policyXMLBaseUrl;
            string dirName = exc.fileFolder;
            List<string> multipleApiNames = multipleAPINames;
            string linkedUrlQueryString = exc.linkedTemplatesUrlQueryString;

            // extract templates from apim service
            Template globalServicePolicyTemplate = await policyExtractor.GenerateGlobalServicePolicyTemplateAsync(sourceApim, resourceGroup, policyXMLBaseUrl, dirName);
            if (apiTemplate == null)
            {
                apiTemplate = await apiExtractor.GenerateAPIsARMTemplateAsync(sourceApim, resourceGroup, singleApiName, multipleApiNames, policyXMLBaseUrl, dirName);
            }
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
                Template masterTemplate = masterTemplateExtractor.GenerateLinkedMasterTemplate(
                    apiTemplate, globalServicePolicyTemplate, apiVersionSetTemplate, productTemplate,
                    loggerTemplate, backendTemplate, authorizationServerTemplate, namedValueTemplate,
                    tagTemplate, fileNames, apiFileName, linkedUrlQueryString, policyXMLBaseUrl);

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
                    await this.GenerateTemplates(new Extractor(exc, apiFileFolder), apiName, null, fileNameGenerator, fileNames, fileWriter, null);
                }

                // create master templates for this apiVersionSet 
                string versionSetFolder = String.Concat(@exc.fileFolder, fileNames.versionSetMasterFolder);
                System.IO.Directory.CreateDirectory(versionSetFolder);
                await this.GenerateTemplates(new Extractor(exc, versionSetFolder), null, apiDictionary[exc.apiVersionSetName], fileNameGenerator, fileNames, fileWriter, null);

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
                    await this.GenerateTemplates(new Extractor(exc, versionSetFolder), null, versionSetEntry.Value, fileNameGenerator, fileNames, fileWriter, null);

                    Console.WriteLine($@"Finish extracting APIVersionSet {versionSetEntry.Key}");
                }

                // Generate templates for each api 
                foreach (string apiName in versionSetEntry.Value)
                {
                    // create folder for each API
                    string tempFileFolder = String.Concat(@apiFileFolder, $@"/{apiName}");
                    System.IO.Directory.CreateDirectory(tempFileFolder);
                    // generate templates for each API
                    await this.GenerateTemplates(new Extractor(exc, tempFileFolder), apiName, null, fileNameGenerator, fileNames, fileWriter, null);

                    Console.WriteLine($@"Finish extracting API {apiName}");
                }
            }
        }

        public async Task GenerateSingleAPIWithRevisionsTemplates(ExtractorConfig exc, string apiName, FileNameGenerator fileNameGenerator, FileWriter fileWriter, FileNames fileNames)
        {
            Console.WriteLine("Extracting singleAPI {0} with revisions", apiName);

            APIExtractor apiExtractor = new APIExtractor(fileWriter);
            // Get all revisions for this api
            string revisions = await apiExtractor.GetAPIRevisionsAsync(exc.sourceApimName, exc.resourceGroup, apiName);
            JObject revs = JObject.Parse(revisions);
            string currentRevision = null;
            List<string> revList = new List<string>();

            // Generate seperate folder for each API revision
            for (int i = 0; i < ((JContainer)revs["value"]).Count; i++)
            {
                string apiID = ((JValue)revs["value"][i]["apiId"]).Value.ToString();
                string singleApiName = apiID.Split("/")[2];
                if (((JValue)revs["value"][i]["isCurrent"]).Value.ToString().Equals("True"))
                {
                    currentRevision = singleApiName;
                }

                string revFileFolder = String.Concat(@exc.fileFolder, $@"/{singleApiName}");
                System.IO.Directory.CreateDirectory(revFileFolder);
                await this.GenerateTemplates(new Extractor(exc, revFileFolder), singleApiName, null, fileNameGenerator, fileNames, fileWriter, null);
                revList.Add(singleApiName);
            }

            if (currentRevision == null)
            {
                throw new Exception($"Revision {apiName} doesn't exist, something went wrong!");
            }
            // generate revisions master folder
            string revMasterFolder = String.Concat(@exc.fileFolder, fileNames.revisionMasterFolder);
            System.IO.Directory.CreateDirectory(revMasterFolder);
            Extractor revExc = new Extractor(exc, revMasterFolder);
            Template apiRevisionTemplate = await apiExtractor.GenerateAPIRevisionTemplateAsync(currentRevision, revList, apiName, revExc);
            await GenerateTemplates(revExc, null, null, fileNameGenerator, fileNames, fileWriter, apiRevisionTemplate);
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
