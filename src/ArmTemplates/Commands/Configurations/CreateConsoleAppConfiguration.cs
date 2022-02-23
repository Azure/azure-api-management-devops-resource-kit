// --------------------------------------------------------------------------
//  <copyright file="CreateConsoleAppConfiguration.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using CommandLine;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Configurations
{
    [Verb(GlobalConstants.CreateName, HelpText = GlobalConstants.CreateDescription)]
    public class CreateConsoleAppConfiguration
    {
        [Option(longName: "appInsightsInstrumentationKey", HelpText = "AppInsights intrumentationkey")]
        public string AppInsightsInstrumentationKey { get; set; }

        [Option(longName: "appInsightsName", HelpText = "AppInsights Name")]
        public string AppInsightsName { get; set; }

        [Option(longName: "namedValueKeys", HelpText = "Named Values")]
        public string NamedValueKeys { get; set; }

        [Option(longName: "apimNameValue", HelpText = "Apim Name Value")]
        public string ApimNameValue { get; set; }

        [Option(longName: "configFile", HelpText = "Config YAML file location")]
        public string ConfigFile { get; set; }

        [Option(longName: "backendUrlConfigFile", HelpText = "Backend url json file location")]
        public string BackendUrlConfigFile { get; set; }

        [Option(longName: "preferredAPIsForDeployment", HelpText = "Create ARM templates for the given APIs Name(comma separated) else leave this parameter blank then by default all api's will be considered")]
        public string PreferredAPIsForDeployment { get; set; }
    }
}
