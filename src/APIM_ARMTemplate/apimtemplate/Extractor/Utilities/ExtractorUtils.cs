using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    static class ExtractorUtils
    {
        public static List<TemplateResource> removeResourceType(string resourceType, List<TemplateResource> resources)
        {
            List<TemplateResource> newResourcesList = new List<TemplateResource>();
            foreach (TemplateResource resource in resources)
            {
                if (!resource.type.Equals(resourceType))
                {
                    newResourcesList.Add(resource);
                }
            }
            return newResourcesList;
        }

        /* three condistions to use this function:
            1. singleApiName is null, then generate one master template for the multipleAPIs in multipleApiNams
            2. multipleApiNams is null, then generate separate folder and master template for each API 
            3. when both singleApiName and multipleApiNams is null, then generate one master template to link all apis in the sourceapim
        */
        public static async Task GenerateTemplates(
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
            string linkedSasToken = exc.linkedTemplatesSasToken;
            string policyXMLBaseUrl = exc.policyXMLBaseUrl;
            string policyXMLSasToken = exc.policyXMLSasToken;
            string dirName = exc.fileFolder;
            List<string> multipleApiNames = multipleAPINames;
            string linkedUrlQueryString = exc.linkedTemplatesUrlQueryString;

            // Get all Apis that will be extracted
            List<string> apisToExtract = new List<string>();
            if (singleApiName != null)
            {
                apisToExtract.Add(singleApiName);
            }
            else if (multipleApiNames != null)
            {
                apisToExtract.AddRange(multipleApiNames);
            }
            else
            {
                List<string> allApis = await apiExtractor.GetAllAPINamesAsync(exc.sourceApimName, exc.resourceGroup);
                apisToExtract.AddRange(allApis);
            }
            Dictionary<string, Dictionary<string, string>> apiLoggerId = null;
            if (exc.paramApiLoggerId)
            {
                apiLoggerId = await GetAllReferencedLoggers(apisToExtract, exc);
            }

            // extract templates from apim service
            Template globalServicePolicyTemplate = await policyExtractor.GenerateGlobalServicePolicyTemplateAsync(sourceApim, resourceGroup, policyXMLBaseUrl, policyXMLSasToken, dirName);
            if (apiTemplate == null)
            {
                apiTemplate = await apiExtractor.GenerateAPIsARMTemplateAsync(singleApiName, multipleApiNames, exc);
            }
            List<TemplateResource> apiTemplateResources = apiTemplate.resources.ToList();
            Template apiVersionSetTemplate = await apiVersionSetExtractor.GenerateAPIVersionSetsARMTemplateAsync(sourceApim, resourceGroup, singleApiName, apiTemplateResources, policyXMLBaseUrl, policyXMLSasToken);
            Template authorizationServerTemplate = await authorizationServerExtractor.GenerateAuthorizationServersARMTemplateAsync(sourceApim, resourceGroup, singleApiName, apiTemplateResources, policyXMLBaseUrl, policyXMLSasToken);
            Template loggerTemplate = await loggerExtractor.GenerateLoggerTemplateAsync(exc, singleApiName, apiTemplateResources, apiLoggerId);
            Template productTemplate = await productExtractor.GenerateProductsARMTemplateAsync(sourceApim, resourceGroup, singleApiName, apiTemplateResources, policyXMLBaseUrl, policyXMLSasToken, dirName);
            List<TemplateResource> productTemplateResources = productTemplate.resources.ToList();
            Template namedValueTemplate = await propertyExtractor.GenerateNamedValuesTemplateAsync(singleApiName, apiTemplateResources, exc);
            Template tagTemplate = await tagExtractor.GenerateTagsTemplateAsync(sourceApim, resourceGroup, singleApiName, apiTemplateResources, productTemplateResources, policyXMLBaseUrl, policyXMLSasToken);
            List<TemplateResource> namedValueResources = namedValueTemplate.resources.ToList();
            Template backendTemplate = await backendExtractor.GenerateBackendsARMTemplateAsync(sourceApim, resourceGroup, singleApiName, apiTemplateResources, namedValueResources, policyXMLBaseUrl, policyXMLSasToken);

            Dictionary<string, string> loggerResourceIds = null;
            if (exc.paramLogResourceId)
            {
                List<TemplateResource> loggerResources = loggerTemplate.resources.ToList();
                loggerResourceIds = loggerExtractor.GetAllLoggerResourceIds(loggerResources);
                loggerTemplate = loggerExtractor.SetLoggerResourceId(loggerTemplate);
            }

            // create parameters file
            Template templateParameters = await masterTemplateExtractor.CreateMasterTemplateParameterValues(apisToExtract, exc, apiLoggerId, loggerResourceIds);

            // write templates to output file location
            string apiFileName = fileNameGenerator.GenerateExtractorAPIFileName(singleApiName, fileNames.baseFileName);

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
            if (loggerTemplate.resources.Count() != 0)
            {
                fileWriter.WriteJSONToFile(loggerTemplate, String.Concat(@dirName, fileNames.loggers));
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
                    tagTemplate, fileNames, apiFileName, exc);

                fileWriter.WriteJSONToFile(masterTemplate, String.Concat(@dirName, fileNames.linkedMaster));
            }

            // write parameters to outputLocation
            fileWriter.WriteJSONToFile(templateParameters, String.Concat(dirName, fileNames.parameters));
        }

        // this function will generate master template for each API within this version set and an extra master template to link these apis
        public static async Task GenerateAPIVersionSetTemplates(ExtractorConfig exc, FileNameGenerator fileNameGenerator, FileNames fileNames, FileWriter fileWriter)
        {
            // get api dictionary and check api version set
            var apiDictionary = await GetAllAPIsDictionary(exc.sourceApimName, exc.resourceGroup, fileWriter);
            if (!apiDictionary.ContainsKey(exc.apiVersionSetName))
            {
                throw new Exception("API Version Set with this name doesn't exist");
            }
            else
            {
                Console.WriteLine("Start extracting the API version set {0}", exc.apiVersionSetName);

                foreach (string apiName in apiDictionary[exc.apiVersionSetName])
                {
                    // generate seperate folder for each API
                    string apiFileFolder = String.Concat(@exc.fileFolder, $@"/{apiName}");
                    System.IO.Directory.CreateDirectory(apiFileFolder);
                    await GenerateTemplates(new Extractor(exc, apiFileFolder), apiName, null, fileNameGenerator, fileNames, fileWriter, null);
                }

                // create master templates for this apiVersionSet 
                string versionSetFolder = String.Concat(@exc.fileFolder, fileNames.versionSetMasterFolder);
                System.IO.Directory.CreateDirectory(versionSetFolder);
                await GenerateTemplates(new Extractor(exc, versionSetFolder), null, apiDictionary[exc.apiVersionSetName], fileNameGenerator, fileNames, fileWriter, null);

                Console.WriteLine($@"Finish extracting APIVersionSet {exc.apiVersionSetName}");

            }
        }

        // this function will generate templates for multiple specified APIs
        public static async Task GenerateMultipleAPIsTemplates(ExtractorConfig exc, FileNameGenerator fileNameGenerator, FileWriter fileWriter, FileNames fileNames)
        {
            if (exc.multipleAPIs == null && exc.multipleAPIs.Equals(""))
            {
                throw new Exception("multipleAPIs parameter doesn't have any data");
            }

            string[] apis = exc.multipleAPIs.Split(',');
            for (int i = 0; i < apis.Length; i++)
            {
                apis[i] = apis[i].Trim();
            }

            Console.WriteLine("Start extracting these {0} APIs", apis.Length);

            foreach (string apiName in apis)
            {
                // generate seperate folder for each API
                string apiFileFolder = String.Concat(@exc.fileFolder, $@"/{apiName}");
                System.IO.Directory.CreateDirectory(apiFileFolder);
                await GenerateTemplates(new Extractor(exc, apiFileFolder), apiName, null, fileNameGenerator, fileNames, fileWriter, null);
            }

            // create master templates for these apis 
            string groupApiFolder = String.Concat(@exc.fileFolder, fileNames.groupAPIsMasterFolder);
            System.IO.Directory.CreateDirectory(groupApiFolder);
            await GenerateTemplates(new Extractor(exc, groupApiFolder), null, apis.ToList(), fileNameGenerator, fileNames, fileWriter, null);

            Console.WriteLine($@"Finish extracting mutiple APIs");
        }

        // this function will generate split api templates / folders for each api in this sourceApim
        public static async Task GenerateSplitAPITemplates(ExtractorConfig exc, FileNameGenerator fileNameGenerator, FileWriter fileWriter, FileNames fileNames)
        {
            // Generate folders based on all apiversionset
            var apiDictionary = await GetAllAPIsDictionary(exc.sourceApimName, exc.resourceGroup, fileWriter);

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
                    await GenerateTemplates(new Extractor(exc, versionSetFolder), null, versionSetEntry.Value, fileNameGenerator, fileNames, fileWriter, null);

                    Console.WriteLine($@"Finish extracting APIVersionSet {versionSetEntry.Key}");
                }

                // Generate templates for each api 
                foreach (string apiName in versionSetEntry.Value)
                {
                    // create folder for each API
                    string tempFileFolder = String.Concat(@apiFileFolder, $@"/{apiName}");
                    System.IO.Directory.CreateDirectory(tempFileFolder);
                    // generate templates for each API
                    await GenerateTemplates(new Extractor(exc, tempFileFolder), apiName, null, fileNameGenerator, fileNames, fileWriter, null);

                    Console.WriteLine($@"Finish extracting API {apiName}");
                }
            }
        }

        public static string GenValidParamName(string apiName, string prefix)
        {
            string validApiName = Regex.Replace(apiName, "[^a-zA-Z0-9]", "");
            if (Char.IsDigit(validApiName.First()))
            {
                return prefix + validApiName;
            }
            else
            {
                return validApiName;
            }
        }

        public static async Task GenerateSingleAPIWithRevisionsTemplates(ExtractorConfig exc, string apiName, FileNameGenerator fileNameGenerator, FileWriter fileWriter, FileNames fileNames)
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
                await GenerateTemplates(new Extractor(exc, revFileFolder), singleApiName, null, fileNameGenerator, fileNames, fileWriter, null);
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
        public static async Task<Dictionary<string, List<string>>> GetAllAPIsDictionary(string sourceApim, string resourceGroup, FileWriter fileWriter)
        {
            APIExtractor apiExtractor = new APIExtractor(fileWriter);
            // pull all apis from service
            JToken[] apis = await apiExtractor.GetAllAPIObjsAsync(sourceApim, resourceGroup);

            // Generate folders based on all apiversionset
            var apiDictionary = new Dictionary<string, List<string>>();
            foreach (JToken oApi in apis)
            {
                string apiDisplayName = ((JValue)oApi["properties"]["displayName"]).Value.ToString();
                if (!apiDictionary.ContainsKey(apiDisplayName))
                {
                    List<string> apiVersionSet = new List<string>();
                    apiVersionSet.Add(((JValue)oApi["name"]).Value.ToString());
                    apiDictionary[apiDisplayName] = apiVersionSet;
                }
                else
                {
                    apiDictionary[apiDisplayName].Add(((JValue)oApi["name"]).Value.ToString());
                }
            }
            return apiDictionary;
        }

        // this function generate all reference loggers in all extracted apis
        public static async Task<Dictionary<string, Dictionary<string, string>>> GetAllReferencedLoggers(List<string> apisToExtract, Extractor exc)
        {
            Dictionary<string, Dictionary<string, string>> ApiLoggerId = new Dictionary<string, Dictionary<string, string>>();
            APIExtractor apiExc = new APIExtractor(new FileWriter());
            foreach (string curApiName in apisToExtract)
            {
                Dictionary<string, string> loggerIds = new Dictionary<string, string>();
                string diagnostics = await apiExc.GetAPIDiagnosticsAsync(exc.sourceApimName, exc.resourceGroup, curApiName);
                JObject oDiagnostics = JObject.Parse(diagnostics);
                foreach (var diagnostic in oDiagnostics["value"])
                {
                    string diagnosticName = ((JValue)diagnostic["name"]).Value.ToString();
                    string loggerId = ((JValue)diagnostic["properties"]["loggerId"]).Value.ToString();
                    loggerIds.Add(ExtractorUtils.GenValidParamName(diagnosticName, ParameterPrefix.Diagnostic), loggerId);
                }
                if (loggerIds.Count != 0)
                {
                    ApiLoggerId.Add(ExtractorUtils.GenValidParamName(curApiName, ParameterPrefix.Api), loggerIds);
                }
            }
            return ApiLoggerId;
        }
    }
}