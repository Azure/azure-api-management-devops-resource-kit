using apimtemplate.Common;
using apimtemplate.Common.Constants;
using apimtemplate.Common.Exceptions;
using apimtemplate.Common.FileHandlers;
using apimtemplate.Common.Helpers;
using apimtemplate.Common.Templates.Abstractions;
using apimtemplate.Common.Templates.Logger;
using apimtemplate.Extractor.EntityExtractors;
using apimtemplate.Extractor.EntityExtractors.Abstractions;
using apimtemplate.Extractor.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace apimtemplate.Commands.Executors
{
    public class ExtractorExecutor
    {
        private readonly ExtractorConfig _extractorConfig;

        private readonly IApiExtractor _apiExtractor;
        private readonly IApiVersionSetExtractor _apiVersionSetExtractor;
        private readonly IAuthorizationServerExtractor _authorizationServerExtractor;
        private readonly IBackendExtractor _backendExtractor;
        private readonly ILoggerExtractor _loggerExtractor;
        private readonly IMasterTemplateExtractor _masterTemplateExtractor;
        private readonly IPolicyExtractor _policyExtractor;
        private readonly IProductApiExtractor _productApiExtractor;
        private readonly IProductExtractor _productExtractor;
        private readonly IPropertyExtractor _propertyExtractor;
        private readonly IApiTagExtractor _apiTagExtractor;
        private readonly ITagExtractor _tagExtractor;

        public ExtractorExecutor(
            ExtractorConfig extractorConfig,
            IApiExtractor apiExtractor = null, 
            IApiVersionSetExtractor apiVersionSetExtractor = null,
            IAuthorizationServerExtractor authorizationServerExtractor = null,
            IBackendExtractor backendExtractor = null,
            ILoggerExtractor loggerExtractor = null,
            IMasterTemplateExtractor masterTemplateExtractor = null,
            IPolicyExtractor policyExtractor = null,
            IProductApiExtractor productApiExtractor = null,
            IProductExtractor productExtractor = null,
            IPropertyExtractor propertyExtractor = null,
            IApiTagExtractor apiTagExtractor = null,
            ITagExtractor tagExtractor = null)
        {
            _extractorConfig = extractorConfig;

            _apiExtractor = apiExtractor ?? new ApiExtractor();
            _apiVersionSetExtractor = apiVersionSetExtractor ?? new ApiVersionSetExtractor();
            _authorizationServerExtractor = authorizationServerExtractor ?? new AuthorizationServerExtractor();
            _backendExtractor = backendExtractor ?? new BackendExtractor();
            _loggerExtractor = loggerExtractor ?? new LoggerExtractor();
            _masterTemplateExtractor = masterTemplateExtractor ?? new MasterTemplateExtractor();
            _policyExtractor = policyExtractor ?? new PolicyExtractor();
            _productApiExtractor = productApiExtractor ?? new ProductApiExtractor();
            _propertyExtractor = propertyExtractor ?? new PropertyExtractor();
            _productExtractor = productExtractor ?? new ProductExtractor();
            _apiTagExtractor = apiTagExtractor ?? new ApiTagExtractor();
            _tagExtractor = tagExtractor ?? new TagExtractor();
        }

        /// <summary>
        /// Retrieves parameters for extractor from the configuration and runs generation automatically.
        /// For specific template generation scenarios, please, use other exposed methods
        /// </summary>
        public async Task ExecuteGenerationBasedOnExtractorConfiguration()
        {
            string singleApiName = _extractorConfig.apiName;
            bool splitAPIs = _extractorConfig.splitAPIs != null && _extractorConfig.splitAPIs.Equals("true");
            bool hasVersionSetName = _extractorConfig.apiVersionSetName != null;
            bool hasSingleApi = singleApiName != null;
            bool includeRevisions = _extractorConfig.includeAllRevisions != null && _extractorConfig.includeAllRevisions.Equals("true");
            bool hasMultipleAPIs = _extractorConfig.multipleAPIs != null;
            EntityExtractorBase.baseUrl = _extractorConfig.serviceBaseUrl == null
                ? EntityExtractorBase.baseUrl
                : _extractorConfig.serviceBaseUrl;

            Logger.LogInformation("API Management Template");
            Logger.LogInformation("Connecting to {0} API Management Service on {1} Resource Group ...", _extractorConfig.sourceApimName, _extractorConfig.resourceGroup);

            var fileNames = _extractorConfig.baseFileName == null
                ? FileNameGenerator.GenerateFileNames(_extractorConfig.sourceApimName)
                : FileNameGenerator.GenerateFileNames(_extractorConfig.baseFileName);

            if (splitAPIs)
            {
                // create split api templates for all apis in the sourceApim
                await GenerateSplitAPITemplates(fileNames);
                await GenerateTemplates(new ExtractorParameters(_extractorConfig), null, null, fileNames, null);
            }
            else if (hasVersionSetName)
            {
                // create split api templates and aggregated api templates for this apiversionset
                await GenerateAPIVersionSetTemplates(_extractorConfig, fileNames);
            }
            else if (hasMultipleAPIs)
            {
                // generate templates for multiple APIs
                await GenerateMultipleAPIsTemplates(_extractorConfig, fileNames);
            }
            else if (hasSingleApi && includeRevisions)
            {
                // handle single API include Revision extraction
                await GenerateSingleAPIWithRevisionsTemplates(_extractorConfig, singleApiName, fileNames);
            }
            else
            {
                // create single api template or create aggregated api templates for all apis within the sourceApim
                if (hasSingleApi)
                {
                    Logger.LogInformation("Executing extraction for {0} API ...", singleApiName);
                }
                else
                {
                    Logger.LogInformation("Executing full extraction ...");
                }

                await GenerateTemplates(new ExtractorParameters(_extractorConfig), singleApiName, null, fileNames, null);
            }
        }

        /// <summary>
        /// Generates split api templates / folders for each api in this sourceApim 
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns></returns>
        public async Task GenerateSplitAPITemplates(FileNames fileNames)
        {
            // Generate folders based on all apiversionset
            var apiDictionary = await GetAllAPIsDictionary(_extractorConfig.sourceApimName, _extractorConfig.resourceGroup);

            // Generate templates based on each API/APIversionSet
            foreach (var versionSetEntry in apiDictionary)
            {
                string apiFileFolder = _extractorConfig.fileFolder;

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
                    await GenerateTemplates(new ExtractorParameters(_extractorConfig, versionSetFolder), null, versionSetEntry.Value, fileNames, null);

                    Logger.LogInformation($@"Finish extracting APIVersionSet {versionSetEntry.Key}");
                }

                // Generate templates for each api 
                foreach (string apiName in versionSetEntry.Value)
                {
                    // create folder for each API
                    string tempFileFolder = String.Concat(@apiFileFolder, $@"/{apiName}");
                    System.IO.Directory.CreateDirectory(tempFileFolder);
                    // generate templates for each API
                    await GenerateTemplates(new ExtractorParameters(_extractorConfig, tempFileFolder), apiName, null, fileNames, null);

                    Logger.LogInformation($@"Finish extracting API {apiName}");
                }
            }
        }

        /// <summary>
        /// three condistions to use this function:
        /// 1. singleApiName is null, then generate one master template for the multipleAPIs in multipleApiNams
        /// 2. multipleApiNams is null, then generate separate folder and master template for each API 
        /// 3. when both singleApiName and multipleApiNams is null, then generate one master template to link all apis in the sourceapim
        /// </summary>
        public async Task GenerateTemplates(
            ExtractorParameters extractorParameters,
            string singleApiName,
            List<string> multipleApiNames,
            FileNames fileNames,
            Template apiTemplate)
        {
            if (singleApiName != null && multipleApiNames != null)
            {
                throw new SingleAndMultipleApisCanNotExistTogetherException("can't specify single API and multiple APIs to extract at the same time");
            }

            // read parameters
            //string sourceApim = extractorParameters.sourceApimName;
            //string resourceGroup = extractorParameters.resourceGroup;
            //string destinationApim = extractorParameters.destinationApimName;
            //string linkedBaseUrl = extractorParameters.linkedTemplatesBaseUrl;
            //string linkedSasToken = extractorParameters.linkedTemplatesSasToken;
            //string policyXMLBaseUrl = extractorParameters.policyXMLBaseUrl;
            //string policyXMLSasToken = extractorParameters.policyXMLSasToken;
            //string dirName = extractorParameters.fileFolder;
            //List<string> multipleApiNames = multipleAPINames;
            //string linkedUrlQueryString = extractorParameters.linkedTemplatesUrlQueryString;

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
                List<string> allApis = await _apiExtractor.GetAllApiNamesAsync(extractorParameters.sourceApimName, extractorParameters.resourceGroup);
                apisToExtract.AddRange(allApis);
            }
            Dictionary<string, object> apiLoggerId = null;
            if (extractorParameters.paramApiLoggerId)
            {
                apiLoggerId = await GetAllReferencedLoggers(apisToExtract, extractorParameters);
            }

            // extract templates from apim service
            Template globalServicePolicyTemplate = await _policyExtractor.GenerateGlobalServicePolicyTemplateAsync(
                extractorParameters.sourceApimName,
                extractorParameters.resourceGroup,
                extractorParameters.policyXMLBaseUrl,
                extractorParameters.policyXMLSasToken,
                extractorParameters.fileFolder);

            if (apiTemplate == null)
            {
                apiTemplate = await _apiExtractor.GenerateAPIsARMTemplateAsync(singleApiName, multipleApiNames, extractorParameters);
            }

            List<TemplateResource> apiTemplateResources = apiTemplate.resources.ToList();
            Template apiVersionSetTemplate = await _apiVersionSetExtractor.GenerateAPIVersionSetsARMTemplateAsync(extractorParameters.sourceApimName, extractorParameters.resourceGroup, singleApiName, apiTemplateResources);
            Template authorizationServerTemplate = await _authorizationServerExtractor.GenerateAuthorizationServersARMTemplateAsync(extractorParameters.sourceApimName, extractorParameters.resourceGroup, singleApiName, apiTemplateResources);
            Template loggerTemplate = await _loggerExtractor.GenerateLoggerTemplateAsync(extractorParameters, singleApiName, apiTemplateResources, apiLoggerId);
            Template productTemplate = await _productExtractor.GenerateProductsARMTemplateAsync(extractorParameters.sourceApimName, extractorParameters.resourceGroup, singleApiName, apiTemplateResources, extractorParameters.fileFolder, extractorParameters);
            Template productAPITemplate = await _productApiExtractor.GenerateAPIProductsARMTemplateAsync(singleApiName, multipleApiNames, extractorParameters);
            Template apiTagTemplate = await _apiTagExtractor.GenerateAPITagsARMTemplateAsync(singleApiName, multipleApiNames, extractorParameters);
            List<TemplateResource> productTemplateResources = productTemplate.resources.ToList();
            List<TemplateResource> loggerResources = loggerTemplate.resources.ToList();
            Template namedValueTemplate = await _propertyExtractor.GenerateNamedValuesTemplateAsync(singleApiName, apiTemplateResources, extractorParameters, _backendExtractor, loggerResources);
            Template tagTemplate = await _tagExtractor.GenerateTagsTemplateAsync(extractorParameters.sourceApimName, extractorParameters.resourceGroup, singleApiName, apiTemplateResources, productTemplateResources, extractorParameters.policyXMLBaseUrl, extractorParameters.policyXMLSasToken);
            List<TemplateResource> namedValueResources = namedValueTemplate.resources.ToList();

            var backendResult = await _backendExtractor.GenerateBackendsARMTemplateAsync(extractorParameters.sourceApimName, extractorParameters.resourceGroup, singleApiName, apiTemplateResources, namedValueResources, extractorParameters);

            Dictionary<string, string> loggerResourceIds = null;
            if (extractorParameters.paramLogResourceId)
            {
                loggerResourceIds = LoggerTemplateUtils.GetAllLoggerResourceIds(loggerResources);
                loggerTemplate = LoggerTemplateUtils.SetLoggerResourceId(loggerTemplate);
            }

            // create parameters file
            Template templateParameters = await _masterTemplateExtractor.CreateMasterTemplateParameterValues(apisToExtract, extractorParameters, apiLoggerId, loggerResourceIds, backendResult.Item2, namedValueResources);

            // write templates to output file location
            string apiFileName = FileNameGenerator.GenerateExtractorAPIFileName(singleApiName, fileNames.baseFileName);

            FileWriter.WriteJSONToFile(apiTemplate, string.Concat(extractorParameters.fileFolder, apiFileName));
            // won't generate template when there is no resources
            if (!apiVersionSetTemplate.resources.IsNullOrEmpty())
            {
                FileWriter.WriteJSONToFile(apiVersionSetTemplate, String.Concat(extractorParameters.fileFolder, fileNames.apiVersionSets));
            }
            if (!backendResult.Item1.resources.IsNullOrEmpty())
            {
                FileWriter.WriteJSONToFile(backendResult.Item1, String.Concat(extractorParameters.fileFolder, fileNames.backends));
            }
            if (!loggerTemplate.resources.IsNullOrEmpty())
            {
                FileWriter.WriteJSONToFile(loggerTemplate, String.Concat(extractorParameters.fileFolder, fileNames.loggers));
            }
            if (!authorizationServerTemplate.resources.IsNullOrEmpty())
            {
                FileWriter.WriteJSONToFile(authorizationServerTemplate, String.Concat(extractorParameters.fileFolder, fileNames.authorizationServers));
            }
            if (!productTemplate.resources.IsNullOrEmpty())
            {
                FileWriter.WriteJSONToFile(productTemplate, String.Concat(extractorParameters.fileFolder, fileNames.products));
            }
            if (!productAPITemplate.resources.IsNullOrEmpty())
            {
                FileWriter.WriteJSONToFile(productAPITemplate, String.Concat(extractorParameters.fileFolder, fileNames.productAPIs));
            }
            if (!apiTagTemplate.resources.IsNullOrEmpty())
            {
                FileWriter.WriteJSONToFile(apiTagTemplate, String.Concat(extractorParameters.fileFolder, fileNames.apiTags));
            }
            if (!tagTemplate.resources.IsNullOrEmpty())
            {
                FileWriter.WriteJSONToFile(tagTemplate, String.Concat(extractorParameters.fileFolder, fileNames.tags));
            }
            if (!namedValueTemplate.resources.IsNullOrEmpty())
            {
                FileWriter.WriteJSONToFile(namedValueTemplate, String.Concat(extractorParameters.fileFolder, fileNames.namedValues));
            }
            if (!globalServicePolicyTemplate.resources.IsNullOrEmpty())
            {
                FileWriter.WriteJSONToFile(globalServicePolicyTemplate, String.Concat(extractorParameters.fileFolder, fileNames.globalServicePolicy));
            }
            if (extractorParameters.linkedTemplatesBaseUrl != null)
            {
                // create a master template that links to all other templates
                Template masterTemplate = _masterTemplateExtractor.GenerateLinkedMasterTemplate(
                    apiTemplate, globalServicePolicyTemplate, apiVersionSetTemplate, productTemplate, productAPITemplate,
                    apiTagTemplate, loggerTemplate, backendResult.Item1, authorizationServerTemplate, namedValueTemplate,
                    tagTemplate, fileNames, apiFileName, extractorParameters);

                FileWriter.WriteJSONToFile(masterTemplate, string.Concat(extractorParameters.fileFolder, fileNames.linkedMaster));
            }

            // write parameters to outputLocation
            FileWriter.WriteJSONToFile(templateParameters, string.Concat(extractorParameters.fileFolder, fileNames.parameters));
        }

        /// <summary>
        /// Generates master template for each API within this version set and an extra master template to link these apis
        /// </summary>
        public async Task GenerateAPIVersionSetTemplates(ExtractorConfig extractorConfig, FileNames fileNames)
        {
            // get api dictionary and check api version set
            var apiDictionary = await GetAllAPIsDictionary(extractorConfig.sourceApimName, extractorConfig.resourceGroup);
            if (!apiDictionary.ContainsKey(extractorConfig.apiVersionSetName))
            {
                throw new NoApiVersionSetWithSuchNameFoundException("API Version Set with this name doesn't exist");
            }
            else
            {
                Logger.LogInformation("Start extracting the API version set {0}", extractorConfig.apiVersionSetName);

                foreach (string apiName in apiDictionary[extractorConfig.apiVersionSetName])
                {
                    // generate seperate folder for each API
                    string apiFileFolder = String.Concat(extractorConfig.fileFolder, $@"/{apiName}");
                    System.IO.Directory.CreateDirectory(apiFileFolder);
                    await GenerateTemplates(new ExtractorParameters(extractorConfig, apiFileFolder), apiName, null, fileNames, null);
                }

                // create master templates for this apiVersionSet 
                string versionSetFolder = String.Concat(extractorConfig.fileFolder, fileNames.versionSetMasterFolder);
                System.IO.Directory.CreateDirectory(versionSetFolder);
                await GenerateTemplates(new ExtractorParameters(extractorConfig, versionSetFolder), null, apiDictionary[extractorConfig.apiVersionSetName], fileNames, null);

                Logger.LogInformation($@"Finish extracting APIVersionSet {extractorConfig.apiVersionSetName}");
            }
        }

        /// <summary>
        /// Generates templates for multiple specified APIs
        /// </summary>
        public async Task GenerateMultipleAPIsTemplates(ExtractorConfig extractorConfig, FileNames fileNames)
        {
            if (extractorConfig.multipleAPIs == null && extractorConfig.multipleAPIs.Equals(""))
            {
                throw new Exception("multipleAPIs parameter doesn't have any data");
            }

            string[] apis = extractorConfig.multipleAPIs.Split(',');
            for (int i = 0; i < apis.Length; i++)
            {
                apis[i] = apis[i].Trim();
            }

            Logger.LogInformation("Start extracting these {0} APIs", apis.Length);

            foreach (string apiName in apis)
            {
                // generate seperate folder for each API
                string apiFileFolder = String.Concat(extractorConfig.fileFolder, $@"/{apiName}");
                System.IO.Directory.CreateDirectory(apiFileFolder);
                await GenerateTemplates(new ExtractorParameters(extractorConfig, apiFileFolder), apiName, null, fileNames, null);
            }

            // create master templates for these apis 
            string groupApiFolder = String.Concat(extractorConfig.fileFolder, fileNames.groupAPIsMasterFolder);
            System.IO.Directory.CreateDirectory(groupApiFolder);
            await GenerateTemplates(new ExtractorParameters(extractorConfig, groupApiFolder), null, apis.ToList(), fileNames, null);

            Logger.LogInformation($@"Finish extracting mutiple APIs");
        }

        public async Task GenerateSingleAPIWithRevisionsTemplates(ExtractorConfig extractorConfig, string apiName, FileNames fileNames)
        {
            Logger.LogInformation("Extracting singleAPI {0} with revisions", apiName);

            // Get all revisions for this api
            string revisions = await _apiExtractor.GetAPIRevisionsAsync(extractorConfig.sourceApimName, extractorConfig.resourceGroup, apiName);
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

                string revFileFolder = String.Concat(extractorConfig.fileFolder, $@"/{singleApiName}");
                System.IO.Directory.CreateDirectory(revFileFolder);
                await GenerateTemplates(new ExtractorParameters(extractorConfig, revFileFolder), singleApiName, null, fileNames, null);
                revList.Add(singleApiName);
            }

            if (currentRevision == null)
            {
                throw new Exception($"Revision {apiName} doesn't exist, something went wrong!");
            }
            // generate revisions master folder
            string revMasterFolder = String.Concat(extractorConfig.fileFolder, fileNames.revisionMasterFolder);
            System.IO.Directory.CreateDirectory(revMasterFolder);
            ExtractorParameters revExc = new ExtractorParameters(extractorConfig, revMasterFolder);
            Template apiRevisionTemplate = await _apiExtractor.GenerateAPIRevisionTemplateAsync(currentRevision, revList, apiName, revExc);
            await GenerateTemplates(revExc, null, null, fileNames, apiRevisionTemplate);
        }

        /// <summary>
        /// Generates an api dictionary with apiName/versionsetName (if exist one) as key, list of apiNames as value
        /// </summary>
        /// <returns></returns>
        private async Task<Dictionary<string, List<string>>> GetAllAPIsDictionary(string sourceApim, string resourceGroup)
        {
            // pull all apis from service
            JToken[] apis = await _apiExtractor.GetAllApiObjsAsync(sourceApim, resourceGroup);

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

        private async Task<Dictionary<string, object>> GetAllReferencedLoggers(List<string> apisToExtract, ExtractorParameters extractorParameters)
        {
            var apiLoggerId = new Dictionary<string, object>();

            var serviceDiagnostics = await _apiExtractor.GetServiceDiagnosticsAsync(extractorParameters.sourceApimName, extractorParameters.resourceGroup);
            JObject oServiceDiagnostics = JObject.Parse(serviceDiagnostics);

            var serviceloggerIds = new Dictionary<string, string>();
            foreach (var serviceDiagnostic in oServiceDiagnostics["value"])
            {
                string diagnosticName = ((JValue)serviceDiagnostic["name"]).Value.ToString();
                string loggerId = ((JValue)serviceDiagnostic["properties"]["loggerId"]).Value.ToString();
                apiLoggerId.Add(ParameterNamingHelper.GenerateValidParameterName(diagnosticName, ParameterPrefix.Diagnostic), loggerId);
            }


            foreach (string curApiName in apisToExtract)
            {
                var loggerIds = new Dictionary<string, string>();
                var diagnostics = await _apiExtractor.GetApiDiagnosticsAsync(extractorParameters.sourceApimName, extractorParameters.resourceGroup, curApiName);
                JObject oDiagnostics = JObject.Parse(diagnostics);
                foreach (var diagnostic in oDiagnostics["value"])
                {
                    string diagnosticName = ((JValue)diagnostic["name"]).Value.ToString();
                    string loggerId = ((JValue)diagnostic["properties"]["loggerId"]).Value.ToString();
                    loggerIds.Add(ParameterNamingHelper.GenerateValidParameterName(diagnosticName, ParameterPrefix.Diagnostic), loggerId);
                }
                if (loggerIds.Count != 0)
                {
                    apiLoggerId.Add(ParameterNamingHelper.GenerateValidParameterName(curApiName, ParameterPrefix.Api), loggerIds);
                }
            }

            return apiLoggerId;
        }
    }
}
