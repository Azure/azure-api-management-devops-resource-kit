using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Exceptions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors
{
    public class ExtractorExecutor
    {
        readonly ExtractorConfig extractorConfig;

        readonly IApiExtractor apiExtractor;
        readonly IApiVersionSetExtractor apiVersionSetExtractor;
        readonly IAuthorizationServerExtractor authorizationServerExtractor;
        readonly IBackendExtractor backendExtractor;
        readonly ILoggerExtractor loggerExtractor;
        readonly IMasterTemplateExtractor masterTemplateExtractor;
        readonly IPolicyExtractor policyExtractor;
        readonly IProductApiExtractor productApiExtractor;
        readonly IProductExtractor productExtractor;
        readonly IPropertyExtractor propertyExtractor;
        readonly IApiTagExtractor apiTagExtractor;
        readonly ITagExtractor tagExtractor;

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
            this.extractorConfig = extractorConfig;

            this.apiExtractor = apiExtractor ?? new ApiExtractor();
            this.apiVersionSetExtractor = apiVersionSetExtractor ?? new ApiVersionSetExtractor();
            this.authorizationServerExtractor = authorizationServerExtractor ?? new AuthorizationServerExtractor();
            this.backendExtractor = backendExtractor ?? new BackendExtractor();
            this.loggerExtractor = loggerExtractor ?? new LoggerExtractor();
            this.masterTemplateExtractor = masterTemplateExtractor ?? new MasterTemplateExtractor();
            this.policyExtractor = policyExtractor ?? new PolicyExtractor();
            this.productApiExtractor = productApiExtractor ?? new ProductApiExtractor();
            this.propertyExtractor = propertyExtractor ?? new PropertyExtractor();
            this.productExtractor = productExtractor ?? new ProductExtractor();
            this.apiTagExtractor = apiTagExtractor ?? new ApiTagExtractor();
            this.tagExtractor = tagExtractor ?? new TagExtractor();
        }

        /// <summary>
        /// Retrieves parameters for extractor from the configuration and runs generation automatically.
        /// For specific template generation scenarios, please, use other exposed methods
        /// </summary>
        public async Task ExecuteGenerationBasedOnExtractorConfiguration()
        {
            string singleApiName = this.extractorConfig.apiName;
            bool splitAPIs = this.extractorConfig.splitAPIs != null && this.extractorConfig.splitAPIs.Equals("true");
            bool hasVersionSetName = this.extractorConfig.apiVersionSetName != null;
            bool hasSingleApi = singleApiName != null;
            bool includeRevisions = this.extractorConfig.includeAllRevisions != null && this.extractorConfig.includeAllRevisions.Equals("true");
            bool hasMultipleAPIs = this.extractorConfig.multipleAPIs != null;
            EntityExtractorBase.BaseUrl = this.extractorConfig.serviceBaseUrl == null
                ? EntityExtractorBase.BaseUrl
                : this.extractorConfig.serviceBaseUrl;

            Logger.LogInformation("API Management Template");
            Logger.LogInformation("Connecting to {0} API Management Service on {1} Resource Group ...", this.extractorConfig.sourceApimName, this.extractorConfig.resourceGroup);

            var fileNames = this.extractorConfig.baseFileName == null
                ? FileNameGenerator.GenerateFileNames(this.extractorConfig.sourceApimName)
                : FileNameGenerator.GenerateFileNames(this.extractorConfig.baseFileName);

            if (splitAPIs)
            {
                // create split api templates for all apis in the sourceApim
                await this.GenerateSplitAPITemplates(fileNames);
                await this.GenerateTemplates(new ExtractorParameters(this.extractorConfig), null, null, fileNames, null);
            }
            else if (hasVersionSetName)
            {
                // create split api templates and aggregated api templates for this apiversionset
                await this.GenerateAPIVersionSetTemplates(this.extractorConfig, fileNames);
            }
            else if (hasMultipleAPIs)
            {
                // generate templates for multiple APIs
                await this.GenerateMultipleAPIsTemplates(this.extractorConfig, fileNames);
            }
            else if (hasSingleApi && includeRevisions)
            {
                // handle single API include Revision extraction
                await this.GenerateSingleAPIWithRevisionsTemplates(this.extractorConfig, singleApiName, fileNames);
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

                await this.GenerateTemplates(new ExtractorParameters(this.extractorConfig), singleApiName, null, fileNames, null);
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
            var apiDictionary = await this.GetAllAPIsDictionary(this.extractorConfig.sourceApimName, this.extractorConfig.resourceGroup);

            // Generate templates based on each API/APIversionSet
            foreach (var versionSetEntry in apiDictionary)
            {
                string apiFileFolder = this.extractorConfig.fileFolder;

                // if it's APIVersionSet, generate the versionsetfolder for templates
                if (versionSetEntry.Value.Count > 1)
                {
                    // this API has VersionSet
                    string apiDisplayName = versionSetEntry.Key;

                    // create apiVersionSet folder
                    apiFileFolder = string.Concat(@apiFileFolder, $@"/{apiDisplayName}");
                    System.IO.Directory.CreateDirectory(apiFileFolder);

                    // create master templates for each apiVersionSet
                    string versionSetFolder = string.Concat(@apiFileFolder, fileNames.versionSetMasterFolder);
                    System.IO.Directory.CreateDirectory(versionSetFolder);
                    await this.GenerateTemplates(new ExtractorParameters(this.extractorConfig, versionSetFolder), null, versionSetEntry.Value, fileNames, null);

                    Logger.LogInformation($@"Finish extracting APIVersionSet {versionSetEntry.Key}");
                }

                // Generate templates for each api 
                foreach (string apiName in versionSetEntry.Value)
                {
                    // create folder for each API
                    string tempFileFolder = string.Concat(@apiFileFolder, $@"/{apiName}");
                    System.IO.Directory.CreateDirectory(tempFileFolder);
                    // generate templates for each API
                    await this.GenerateTemplates(new ExtractorParameters(this.extractorConfig, tempFileFolder), apiName, null, fileNames, null);

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
                List<string> allApis = await this.apiExtractor.GetAllApiNamesAsync(extractorParameters.sourceApimName, extractorParameters.resourceGroup);
                apisToExtract.AddRange(allApis);
            }
            Dictionary<string, object> apiLoggerId = null;
            if (extractorParameters.paramApiLoggerId)
            {
                apiLoggerId = await this.GetAllReferencedLoggers(apisToExtract, extractorParameters);
            }

            // extract templates from apim service
            Template globalServicePolicyTemplate = await this.policyExtractor.GenerateGlobalServicePolicyTemplateAsync(
                extractorParameters.sourceApimName,
                extractorParameters.resourceGroup,
                extractorParameters.policyXMLBaseUrl,
                extractorParameters.policyXMLSasToken,
                extractorParameters.fileFolder);

            if (apiTemplate == null)
            {
                apiTemplate = await this.apiExtractor.GenerateAPIsARMTemplateAsync(singleApiName, multipleApiNames, extractorParameters);
            }

            List<TemplateResource> apiTemplateResources = apiTemplate.resources.ToList();
            Template apiVersionSetTemplate = await this.apiVersionSetExtractor.GenerateAPIVersionSetsARMTemplateAsync(extractorParameters.sourceApimName, extractorParameters.resourceGroup, singleApiName, apiTemplateResources);
            Template authorizationServerTemplate = await this.authorizationServerExtractor.GenerateAuthorizationServersARMTemplateAsync(extractorParameters.sourceApimName, extractorParameters.resourceGroup, singleApiName, apiTemplateResources);
            Template loggerTemplate = await this.loggerExtractor.GenerateLoggerTemplateAsync(extractorParameters, singleApiName, apiTemplateResources, apiLoggerId);
            Template productTemplate = await this.productExtractor.GenerateProductsARMTemplateAsync(extractorParameters.sourceApimName, extractorParameters.resourceGroup, singleApiName, apiTemplateResources, extractorParameters.fileFolder, extractorParameters);
            Template productAPITemplate = await this.productApiExtractor.GenerateAPIProductsARMTemplateAsync(singleApiName, multipleApiNames, extractorParameters);
            Template apiTagTemplate = await this.apiTagExtractor.GenerateAPITagsARMTemplateAsync(singleApiName, multipleApiNames, extractorParameters);
            List<TemplateResource> productTemplateResources = productTemplate.resources.ToList();
            List<TemplateResource> loggerResources = loggerTemplate.resources.ToList();
            Template namedValueTemplate = await this.propertyExtractor.GenerateNamedValuesTemplateAsync(singleApiName, apiTemplateResources, extractorParameters, this.backendExtractor, loggerResources);
            Template tagTemplate = await this.tagExtractor.GenerateTagsTemplateAsync(extractorParameters.sourceApimName, extractorParameters.resourceGroup, singleApiName, apiTemplateResources, productTemplateResources, extractorParameters.policyXMLBaseUrl, extractorParameters.policyXMLSasToken);
            List<TemplateResource> namedValueResources = namedValueTemplate.resources.ToList();

            var backendResult = await this.backendExtractor.GenerateBackendsARMTemplateAsync(extractorParameters.sourceApimName, extractorParameters.resourceGroup, singleApiName, apiTemplateResources, namedValueResources, extractorParameters);

            Dictionary<string, string> loggerResourceIds = null;
            if (extractorParameters.paramLogResourceId)
            {
                loggerResourceIds = LoggerTemplateUtils.GetAllLoggerResourceIds(loggerResources);
                loggerTemplate = LoggerTemplateUtils.SetLoggerResourceId(loggerTemplate);
            }

            // create parameters file
            Template templateParameters = await this.masterTemplateExtractor.CreateMasterTemplateParameterValues(apisToExtract, extractorParameters, apiLoggerId, loggerResourceIds, backendResult.Item2, namedValueResources);

            // write templates to output file location
            string apiFileName = FileNameGenerator.GenerateExtractorAPIFileName(singleApiName, fileNames.baseFileName);

            FileWriter.WriteJSONToFile(apiTemplate, string.Concat(extractorParameters.fileFolder, apiFileName));
            // won't generate template when there is no resources
            if (!apiVersionSetTemplate.resources.IsNullOrEmpty())
            {
                FileWriter.WriteJSONToFile(apiVersionSetTemplate, string.Concat(extractorParameters.fileFolder, fileNames.apiVersionSets));
            }
            if (!backendResult.Item1.resources.IsNullOrEmpty())
            {
                FileWriter.WriteJSONToFile(backendResult.Item1, string.Concat(extractorParameters.fileFolder, fileNames.backends));
            }
            if (!loggerTemplate.resources.IsNullOrEmpty())
            {
                FileWriter.WriteJSONToFile(loggerTemplate, string.Concat(extractorParameters.fileFolder, fileNames.loggers));
            }
            if (!authorizationServerTemplate.resources.IsNullOrEmpty())
            {
                FileWriter.WriteJSONToFile(authorizationServerTemplate, string.Concat(extractorParameters.fileFolder, fileNames.authorizationServers));
            }
            if (!productTemplate.resources.IsNullOrEmpty())
            {
                FileWriter.WriteJSONToFile(productTemplate, string.Concat(extractorParameters.fileFolder, fileNames.products));
            }
            if (!productAPITemplate.resources.IsNullOrEmpty())
            {
                FileWriter.WriteJSONToFile(productAPITemplate, string.Concat(extractorParameters.fileFolder, fileNames.productAPIs));
            }
            if (!apiTagTemplate.resources.IsNullOrEmpty())
            {
                FileWriter.WriteJSONToFile(apiTagTemplate, string.Concat(extractorParameters.fileFolder, fileNames.apiTags));
            }
            if (!tagTemplate.resources.IsNullOrEmpty())
            {
                FileWriter.WriteJSONToFile(tagTemplate, string.Concat(extractorParameters.fileFolder, fileNames.tags));
            }
            if (!namedValueTemplate.resources.IsNullOrEmpty())
            {
                FileWriter.WriteJSONToFile(namedValueTemplate, string.Concat(extractorParameters.fileFolder, fileNames.namedValues));
            }
            if (!globalServicePolicyTemplate.resources.IsNullOrEmpty())
            {
                FileWriter.WriteJSONToFile(globalServicePolicyTemplate, string.Concat(extractorParameters.fileFolder, fileNames.globalServicePolicy));
            }
            if (extractorParameters.linkedTemplatesBaseUrl != null)
            {
                // create a master template that links to all other templates
                Template masterTemplate = this.masterTemplateExtractor.GenerateLinkedMasterTemplate(
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
            var apiDictionary = await this.GetAllAPIsDictionary(extractorConfig.sourceApimName, extractorConfig.resourceGroup);
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
                    string apiFileFolder = string.Concat(extractorConfig.fileFolder, $@"/{apiName}");
                    System.IO.Directory.CreateDirectory(apiFileFolder);
                    await this.GenerateTemplates(new ExtractorParameters(extractorConfig, apiFileFolder), apiName, null, fileNames, null);
                }

                // create master templates for this apiVersionSet 
                string versionSetFolder = string.Concat(extractorConfig.fileFolder, fileNames.versionSetMasterFolder);
                System.IO.Directory.CreateDirectory(versionSetFolder);
                await this.GenerateTemplates(new ExtractorParameters(extractorConfig, versionSetFolder), null, apiDictionary[extractorConfig.apiVersionSetName], fileNames, null);

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
                string apiFileFolder = string.Concat(extractorConfig.fileFolder, $@"/{apiName}");
                System.IO.Directory.CreateDirectory(apiFileFolder);
                await this.GenerateTemplates(new ExtractorParameters(extractorConfig, apiFileFolder), apiName, null, fileNames, null);
            }

            // create master templates for these apis 
            string groupApiFolder = string.Concat(extractorConfig.fileFolder, fileNames.groupAPIsMasterFolder);
            System.IO.Directory.CreateDirectory(groupApiFolder);
            await this.GenerateTemplates(new ExtractorParameters(extractorConfig, groupApiFolder), null, apis.ToList(), fileNames, null);

            Logger.LogInformation($@"Finish extracting mutiple APIs");
        }

        public async Task GenerateSingleAPIWithRevisionsTemplates(ExtractorConfig extractorConfig, string apiName, FileNames fileNames)
        {
            Logger.LogInformation("Extracting singleAPI {0} with revisions", apiName);

            // Get all revisions for this api
            string revisions = await this.apiExtractor.GetAPIRevisionsAsync(extractorConfig.sourceApimName, extractorConfig.resourceGroup, apiName);
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

                string revFileFolder = string.Concat(extractorConfig.fileFolder, $@"/{singleApiName}");
                System.IO.Directory.CreateDirectory(revFileFolder);
                await this.GenerateTemplates(new ExtractorParameters(extractorConfig, revFileFolder), singleApiName, null, fileNames, null);
                revList.Add(singleApiName);
            }

            if (currentRevision == null)
            {
                throw new Exception($"Revision {apiName} doesn't exist, something went wrong!");
            }
            // generate revisions master folder
            string revMasterFolder = string.Concat(extractorConfig.fileFolder, fileNames.revisionMasterFolder);
            System.IO.Directory.CreateDirectory(revMasterFolder);
            ExtractorParameters revExc = new ExtractorParameters(extractorConfig, revMasterFolder);
            Template apiRevisionTemplate = await this.apiExtractor.GenerateAPIRevisionTemplateAsync(currentRevision, revList, apiName, revExc);
            await this.GenerateTemplates(revExc, null, null, fileNames, apiRevisionTemplate);
        }

        /// <summary>
        /// Generates an api dictionary with apiName/versionsetName (if exist one) as key, list of apiNames as value
        /// </summary>
        /// <returns></returns>
        async Task<Dictionary<string, List<string>>> GetAllAPIsDictionary(string sourceApim, string resourceGroup)
        {
            // pull all apis from service
            JToken[] apis = await this.apiExtractor.GetAllApiObjsAsync(sourceApim, resourceGroup);

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

        async Task<Dictionary<string, object>> GetAllReferencedLoggers(List<string> apisToExtract, ExtractorParameters extractorParameters)
        {
            var apiLoggerId = new Dictionary<string, object>();

            var serviceDiagnostics = await this.apiExtractor.GetServiceDiagnosticsAsync(extractorParameters.sourceApimName, extractorParameters.resourceGroup);
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
                var diagnostics = await this.apiExtractor.GetApiDiagnosticsAsync(extractorParameters.sourceApimName, extractorParameters.resourceGroup, curApiName);
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
