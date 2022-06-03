// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Configurations;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.AuthorizationServer;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters
{
    public class CreatorParameters
    {
        public bool ConsiderAllApiForDeployments { get; private set; } = true;

        public string[] PreferredApis { get; private set; }

        public string Version { get; set; }
        public string ApimServiceName { get; set; }
        // policy file location (local or url)
        public string Policy { get; set; }
        public List<ApiVersionSetConfig> ApiVersionSets { get; set; }
        public List<ApiConfig> Apis { get; set; }
        public List<ProductConfig> Products { get; set; }
        public List<PropertyConfig> NamedValues { get; set; }
        public List<LoggerConfig> Loggers { get; set; }
        public List<AuthorizationServerProperties> AuthorizationServers { get; set; }
        public List<BackendTemplateProperties> Backends { get; set; }
        public List<TagProperties> Tags { get; set; }
        public List<SubscriptionConfig> Subscriptions { get; set; }
        public string OutputLocation { get; set; }
        public bool Linked { get; set; }
        public string LinkedTemplatesBaseUrl { get; set; }
        public string LinkedTemplatesUrlQueryString { get; set; }
        public string BaseFileName { get; set; }
        public bool ParameterizeNamedValues { get; private set; }
        public List<ServiceUrlProperty> ServiceUrlParameters { get; set; }

        public FileNames FileNames { get; private set; }

        internal void OverrideParameters(CreateConsoleAppConfiguration configuration, FileReader fileReader)
        {
            if (!string.IsNullOrEmpty(configuration.ApimNameValue))
            {
                this.ApimServiceName = configuration.ApimNameValue;
            }

            this.UpdateAppInsightNameAndInstrumentationKey(configuration.AppInsightsInstrumentationKey, configuration.AppInsightsName);
            this.UpdateNamedValueInstances(configuration.NamedValueKeys);

            //if preferredAPIsForDeployment passed as parameter
            if (configuration.PreferredAPIsForDeployment != null && !string.IsNullOrEmpty(configuration.PreferredAPIsForDeployment))
            {
                this.ConsiderAllApiForDeployments = false;
                this.PreferredApis = configuration.PreferredAPIsForDeployment.Split(",");
            }

            //if parameterizeNamedValuesAndSecrets passed as parameter
            if (!string.IsNullOrEmpty(configuration.ParameterizeNamedValues))
            {
                this.ParameterizeNamedValues = true;
            }

            //if backendurlfile passed as parameter
            if (configuration.BackendUrlConfigFile != null && !string.IsNullOrEmpty(configuration.BackendUrlConfigFile))
            {
                this.UpdateBackendServiceUrl(configuration.BackendUrlConfigFile, fileReader);
            }
        }

        public void GenerateFileNames()
        {
            this.FileNames = this.BaseFileName == null
                ? FileNameGenerator.GenerateFileNames(this.ApimServiceName)
                : FileNameGenerator.GenerateFileNames(this.BaseFileName);
        }

        void UpdateBackendServiceUrl(string backendServiceUrlFile, FileReader fileReader)
        {
            string backendurlConfigContent = fileReader.RetrieveLocalFileContents(backendServiceUrlFile);

            //if the file is json file
            if (fileReader.IsJSON(backendurlConfigContent))
            {
                var backendUrls = backendurlConfigContent.Deserialize<List<BackendUrlsConfig>>();

                foreach (ApiConfig apiConfig in this.Apis)
                {
                    //if the apiname matches with the one in valid yaml file
                    BackendUrlsConfig backendUrlsConfig = backendUrls.Find(f => f.apiName == apiConfig.Name);

                    //update the backendurl as per the input json file
                    if (backendUrlsConfig != null && !string.IsNullOrEmpty(backendUrlsConfig.apiUrl))
                    {
                        apiConfig.ServiceUrl = backendUrlsConfig.apiUrl;
                    }
                }
            }
        }

        void UpdateNamedValueInstances(string namedValuesInstance)
        {
            const char MultiKeySeparator = ';';
            const char KeyValueSeparator = '|';

            if (!string.IsNullOrEmpty(namedValuesInstance))
            {
                string inputNamedInstances = namedValuesInstance;
                // Validation to see number of underscores match number of semicolons
                if (inputNamedInstances.Count(f => f == MultiKeySeparator) == inputNamedInstances.Count(f => f == KeyValueSeparator))
                {
                    // Splint based on semicolon
                    string[] namedValues = inputNamedInstances.Split(MultiKeySeparator);

                    foreach (string keyValue in namedValues)
                    {
                        if (keyValue.Contains(KeyValueSeparator))
                        {
                            string[] keyValueSeperatedArray = keyValue.Split(KeyValueSeparator);
                            if (this.NamedValues != null && this.NamedValues.Count > 0)
                            {
                                this.NamedValues.Where(x => x.DisplayName == keyValueSeperatedArray[0]).FirstOrDefault().Value = keyValueSeperatedArray[1];
                            }
                        }
                    }
                }
            }
        }

        void UpdateAppInsightNameAndInstrumentationKey(string appInsightsInstrumentationKey, string appInsightsName)
        {
            string appInsightNamePassed = string.Empty;

            if (appInsightsName != null && !string.IsNullOrEmpty(appInsightsName))
            {
                appInsightNamePassed = appInsightsName;
                foreach (ApiConfig aPIConfig in this.Apis)
                {
                    if (aPIConfig.Diagnostic != null)
                    {
                        aPIConfig.Diagnostic.LoggerId = appInsightNamePassed;
                    }
                }
            }

            if (appInsightsInstrumentationKey != null && !string.IsNullOrEmpty(appInsightsInstrumentationKey))
            {
                string appInsightsInstrumentationKeyPassed = appInsightsInstrumentationKey;
                if (this.Loggers != null && this.Loggers.Count > 0)
                {
                    if (!string.IsNullOrEmpty(appInsightNamePassed))
                    {
                        this.Loggers[0].Name = appInsightNamePassed;
                    }
                    if (this.Loggers[0].Credentials != null)
                    {
                        this.Loggers[0].Credentials.InstrumentationKey = appInsightsInstrumentationKeyPassed;
                    }
                }
            }
        }
    }
}
