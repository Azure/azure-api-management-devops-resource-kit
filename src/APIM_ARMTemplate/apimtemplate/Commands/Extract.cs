using System;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
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
            var fileFolderName = this.Option("--fileFolder <filefolder>", "ARM Template files folder", CommandOptionType.SingleValue);

            this.HelpOption();

            this.OnExecute(async () =>
            {
                if (!apiManagementName.HasValue()) throw new Exception("Missing parameter <apimname>.");
                if (!resourceGroupName.HasValue()) throw new Exception("Missing parameter <resourceGroup>.");
                if (!fileFolderName.HasValue()) throw new Exception("Missing parameter <filefolder>.");

                string resourceGroup = resourceGroupName.Values[0].ToString();
                string apimname = apiManagementName.Values[0].ToString();
                string fileFolder = fileFolderName.Values[0].ToString();

                Console.WriteLine("API Management Template");
                Console.WriteLine();
                Console.WriteLine("Connecting to {0} API Management Service on {1} Resource Group ...", apimname, resourceGroup);

                GenerateARMTemplate(apimname, resourceGroup, fileFolder);

                Console.WriteLine("Templates written to output location");

            });
        }

        private void GenerateARMTemplate(string apimname, string resourceGroup, string fileFolder)
        {
            #region API's
            Api api = new Api();
            string apis = api.GetAPIs(apimname, resourceGroup).Result;

            ArmTemplate armTemplate = new ArmTemplate()
            {
                schema = "https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#",
                contentVersion = "1.0.0.0",
                parameters = new Dictionary<string, ExtractorTemplateParameterProperties>
                {
                    { "ApimServiceName", new ExtractorTemplateParameterProperties(){ type = "string" } }
                },
                variables = { },
                resources = { },
                outputs = { }
            };

            JObject oApi = JObject.Parse(apis);
            Console.WriteLine("{0} API's found ...", ((JContainer)oApi["value"]).Count.ToString());

            armTemplate.resources = new List<Resource>();

            GenerateLoggerTemplate(resourceGroup, apimname, fileFolder);

            for (int i = 0; i < ((JContainer)oApi["value"]).Count; i++) //TODO: Refactory
            {
                string apiName = ((JValue)oApi["value"][i]["name"]).Value.ToString();
                string apiDetails = api.GetAPIDetails(apimname, resourceGroup, apiName).Result;

                Console.WriteLine("------------------------------------------");
                Console.WriteLine("Geting operations from {0} API:", apiName);

                JObject oApiDetails = JObject.Parse(apiDetails);
                ApiResource apiResource = JsonConvert.DeserializeObject<ApiResource>(apiDetails);
                string oApiName = ((JValue)oApiDetails["name"]).Value.ToString();

                apiResource.type = ((JValue)oApiDetails["type"]).Value.ToString();
                apiResource.name = $"[concat(parameters('ApimServiceName'), '/{oApiName}')]";
                apiResource.apiVersion = "2018-06-01-preview";
                apiResource.scale = null;
                if (apiResource.properties.apiVersionSetId != null)
                {
                    apiResource.dependsOn = new string[] { };

                    string versionSetName = apiResource.properties.apiVersionSetId;
                    int versionSetPosition = versionSetName.IndexOf("api-version-sets/");
                    versionSetName = versionSetName.Substring(versionSetPosition, (versionSetName.Length - versionSetPosition));
                    apiResource.properties.apiVersionSetId = $"[concat(resourceId('Microsoft.ApiManagement/service', parameters('ApimServiceName')), '/{versionSetName}')]";
                    GenerateVersionSetARMTemplate(apimname, resourceGroup, versionSetName, fileFolder);
                }
                else
                {
                    apiResource.dependsOn = new string[] { };
                }

                armTemplate.resources.Add(apiResource);
                #region Operations

                string operations = api.GetAPIOperations(apimname, resourceGroup, apiName).Result;
                JObject oOperations = JObject.Parse(operations);

                foreach (var item in oOperations["value"])
                {
                    string operationName = ((JValue)item["name"]).Value.ToString();
                    string operationDetails = api.GetAPIOperationDetail(apimname, resourceGroup, apiName, operationName).Result;

                    Console.WriteLine("'{0}' Operation found", operationName);

                    JObject oOperationDetails = JObject.Parse(operationDetails);
                    OperationResource operationResource = JsonConvert.DeserializeObject<OperationResource>(operationDetails);
                    operationResource.name = $"[concat(parameters('ApimServiceName'), '/{oApiName}/{operationResource.name}')]";
                    operationResource.apiVersion = "2018-06-01-preview";
                    operationResource.scale = null;
                    operationResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('ApimServiceName'), '{oApiName}')]" };

                    armTemplate.resources.Add(operationResource);
                }
                #endregion

                #region API Policies
                try
                {
                    Console.Write("Geting API Policy from {0} API: ", apiName);
                    string apiPolicies = api.GetAPIPolicies(apimname, resourceGroup, apiName).Result;
                    Console.WriteLine("Policy found!");
                    JObject oApiPolicies = JObject.Parse(apiPolicies);
                    ApiPoliciesResource apiPoliciesResource = JsonConvert.DeserializeObject<ApiPoliciesResource>(apiPolicies);

                    apiPoliciesResource.apiVersion = "2018-06-01-preview";
                    apiPoliciesResource.name = $"[concat(parameters('ApimServiceName'), '/{oApiName}/{apiPoliciesResource.name}')]";
                    apiPoliciesResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('ApimServiceName'), '{apiName}')]" };

                    armTemplate.resources.Add(apiPoliciesResource);
                }
                catch (Exception)
                {
                    Console.WriteLine("Policy NOT found!");
                }
                #endregion

                #region Diagnostics

                Console.WriteLine("------------------------------------------");
                Console.WriteLine("Geting diagnostics from {0} API:", apiName);
                string diagnostics = api.GetAPIDiagnostics(apimname, resourceGroup, apiName).Result;
                JObject oDiagnostics = JObject.Parse(diagnostics);
                foreach (var diagnostic in oDiagnostics["value"])
                {
                    string diagnosticName = ((JValue)diagnostic["name"]).Value.ToString();
                    Console.WriteLine("'{0}' Diagnostic found", diagnosticName);

                    DiagnosticResource diagnosticResource = diagnostic.ToObject<DiagnosticResource>();
                    diagnosticResource.name = $"[concat(parameters('ApimServiceName'), '/{oApiName}/{diagnosticName}')]";
                    diagnosticResource.type = "Microsoft.ApiManagement/service/apis/diagnostics";
                    diagnosticResource.apiVersion = "2018-06-01-preview";
                    diagnosticResource.scale = null;
                    diagnosticResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('ApimServiceName'), '{oApiName}')]" };

                    if (!diagnosticName.Contains("applicationinsights"))
                    {
                        // enableHttpCorrelationHeaders only works for application insights, causes errors otherwise
                        diagnosticResource.properties.enableHttpCorrelationHeaders = null;
                    }

                    armTemplate.resources.Add(diagnosticResource);

                }

                #endregion
            }

            #endregion

            FileWriter fileWriter = new FileWriter();
            fileWriter.WriteJSONToFile(armTemplate, @fileFolder + Path.DirectorySeparatorChar + apimname + "-apis-template.json");
        }

        private void GenerateVersionSetARMTemplate(string apimname, string resourceGroup, string versionSetName, string fileFolder)
        {
            Api api = new Api();
            ArmTemplate armTemplate = new ArmTemplate()
            {
                schema = "https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#",
                contentVersion = "1.0.0.0",
                parameters = new Dictionary<string, ExtractorTemplateParameterProperties>
                {
                    { "ApimServiceName", new ExtractorTemplateParameterProperties(){ type = "string" } }
                },
                variables = { },
                resources = { },
                outputs = { }
            };

            armTemplate.resources = new List<Resource>();

            string versionSet = api.GetAPIVersionSet(apimname, resourceGroup, versionSetName).Result;
            VersionSetResource versionSetResource = JsonConvert.DeserializeObject<VersionSetResource>(versionSet);

            string filePath = fileFolder + Path.DirectorySeparatorChar + string.Format(versionSetResource.name, "/", "-") + ".json";

            versionSetResource.name = $"[concat(parameters('ApimServiceName'), '/{versionSetResource.name}')]";
            versionSetResource.apiVersion = "2018-06-01-preview";
            armTemplate.resources.Add(versionSetResource);

            FileWriter fileWriter = new FileWriter();

            fileWriter.WriteJSONToFile(armTemplate, filePath);
        }

        private async void GenerateLoggerTemplate(string resourceGroup, string apimname, string fileFolder)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Geting loggers from service");
            Logger logger = new Logger();
            Property property = new Property();
            ArmTemplate armTemplate = new ArmTemplate()
            {
                schema = "https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#",
                contentVersion = "1.0.0.0",
                parameters = new Dictionary<string, ExtractorTemplateParameterProperties>
                {
                    { "ApimServiceName", new ExtractorTemplateParameterProperties(){ type = "string" } }
                },
                variables = { },
                resources = { },
                outputs = { }
            };

            armTemplate.resources = new List<Resource>();

            // pull named values for later credential reference
            string properties = property.GetProperties(apimname, resourceGroup).Result;
            JObject oProperties = JObject.Parse(properties);
            List<PropertyResource> propertyResources = oProperties["value"].ToObject<List<PropertyResource>>();

            string loggers = logger.GetLoggers(apimname, resourceGroup).Result;
            JObject oLoggers = JObject.Parse(loggers);
            foreach (var extractedLogger in oLoggers["value"])
            {
                string loggerName = ((JValue)extractedLogger["name"]).Value.ToString();
                Console.WriteLine("'{0}' Logger found", loggerName);

                string fullLoggerResource = await logger.GetLogger(apimname, resourceGroup, loggerName);
                LoggerResource loggerResource = JsonConvert.DeserializeObject<LoggerResource>(fullLoggerResource);
                loggerResource.name = $"[concat(parameters('ApimServiceName'), '/{loggerName}')]";
                loggerResource.type = "Microsoft.ApiManagement/service/loggers";
                loggerResource.apiVersion = "2018-06-01-preview";
                loggerResource.scale = null;

                // swap credentials for their hidden values, taken from named values
                if (loggerResource.properties.credentials != null)
                {
                    if (loggerResource.properties.credentials.instrumentationKey != null)
                    {
                        string hiddenKey = loggerResource.properties.credentials.instrumentationKey.Substring(2, loggerResource.properties.credentials.instrumentationKey.Length - 4);
                        loggerResource.properties.credentials.instrumentationKey = propertyResources.Find(p => p.properties.displayName == hiddenKey).properties.value;
                    } else if (loggerResource.properties.credentials.connectionString != null)
                    {
                        string hiddenKey = loggerResource.properties.credentials.connectionString.Substring(2, loggerResource.properties.credentials.connectionString.Length - 4);
                        loggerResource.properties.credentials.connectionString = propertyResources.Find(p => p.properties.displayName == hiddenKey).properties.value;
                    }
                }

                armTemplate.resources.Add(loggerResource);
            }

            FileWriter fileWriter = new FileWriter();
            string filePath = fileFolder + Path.DirectorySeparatorChar + string.Format("loggers", "/", "-") + ".json";
            fileWriter.WriteJSONToFile(armTemplate, filePath);
        }
    }
}
