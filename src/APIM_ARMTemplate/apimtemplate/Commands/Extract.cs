using System;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;

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

            this.OnExecute(() =>
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
            APIExtractor apiExtractor = new APIExtractor();
            string apis = apiExtractor.GetAPIs(apimname, resourceGroup).Result;

            Template armTemplate = new Template()
            {
                schema = "https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#",
                contentVersion = "1.0.0.0",
                parameters = new Dictionary<string, TemplateParameterProperties>
                {
                    { "ApimServiceName", new TemplateParameterProperties(){ type = "string" } }
                },
                variables = { },
                resources = { },
                outputs = { }
            };

            JObject oApi = JObject.Parse(apis);
            Console.WriteLine("{0} API's found ...", ((JContainer)oApi["value"]).Count.ToString());

            List<TemplateResource> templateResources = new List<TemplateResource>();

            GenerateLoggerTemplate(resourceGroup, apimname, fileFolder);
            GenerateProductsARMTemplate(apimname, resourceGroup, fileFolder);

            for (int i = 0; i < ((JContainer)oApi["value"]).Count; i++) 
            {
                string apiName = ((JValue)oApi["value"][i]["name"]).Value.ToString();
                string apiDetails = apiExtractor.GetAPIDetails(apimname, resourceGroup, apiName).Result;

                Console.WriteLine("------------------------------------------");
                Console.WriteLine("Geting operations from {0} API:", apiName);

                JObject oApiDetails = JObject.Parse(apiDetails);
                APITemplateResource apiResource = JsonConvert.DeserializeObject<APITemplateResource>(apiDetails);
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

                templateResources.Add(apiResource);
                #region Operations

                string operations = apiExtractor.GetAPIOperations(apimname, resourceGroup, apiName).Result;
                JObject oOperations = JObject.Parse(operations);

                foreach (var item in oOperations["value"])
                {
                    string operationName = ((JValue)item["name"]).Value.ToString();
                    string operationDetails = apiExtractor.GetAPIOperationDetail(apimname, resourceGroup, apiName, operationName).Result;

                    Console.WriteLine("'{0}' Operation found", operationName);

                    OperationTemplateResource operationResource = JsonConvert.DeserializeObject<OperationTemplateResource>(operationDetails);
                    operationResource.name = $"[concat(parameters('ApimServiceName'), '/{oApiName}/{operationResource.name}')]";
                    operationResource.apiVersion = "2018-06-01-preview";
                    operationResource.scale = null;
                    operationResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('ApimServiceName'), '{oApiName}')]" };

                    templateResources.Add(operationResource);
                }
                #endregion

                #region API Policies
                try
                {
                    Console.Write("Geting API Policy from {0} API: ", apiName);
                    string apiPolicies = apiExtractor.GetAPIPolicies(apimname, resourceGroup, apiName).Result;
                    Console.WriteLine("API Policy found!");
                    PolicyTemplateResource apiPoliciesResource = JsonConvert.DeserializeObject<PolicyTemplateResource>(apiPolicies);

                    apiPoliciesResource.apiVersion = "2018-06-01-preview";
                    apiPoliciesResource.name = $"[concat(parameters('ApimServiceName'), '/{oApiName}/{apiPoliciesResource.name}')]";
                    apiPoliciesResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('ApimServiceName'), '{apiName}')]" };

                    templateResources.Add(apiPoliciesResource);
                }
                catch (Exception)
                {
                    Console.WriteLine("No API policy!");
                }
                #endregion

                #region Diagnostics

                Console.WriteLine("------------------------------------------");
                Console.WriteLine("Geting diagnostics from {0} API:", apiName);
                string diagnostics = apiExtractor.GetAPIDiagnostics(apimname, resourceGroup, apiName).Result;
                JObject oDiagnostics = JObject.Parse(diagnostics);
                foreach (var diagnostic in oDiagnostics["value"])
                {
                    string diagnosticName = ((JValue)diagnostic["name"]).Value.ToString();
                    Console.WriteLine("'{0}' Diagnostic found", diagnosticName);

                    DiagnosticTemplateResource diagnosticResource = diagnostic.ToObject<DiagnosticTemplateResource>();
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

                    templateResources.Add(diagnosticResource);

                }

                #endregion
            }

            #endregion
            
            armTemplate.resources = templateResources.ToArray();
            FileWriter fileWriter = new FileWriter();
            fileWriter.WriteJSONToFile(armTemplate, @fileFolder + Path.DirectorySeparatorChar + apimname + "-apis-template.json");
        }
        private void GenerateVersionSetARMTemplate(string apimname, string resourceGroup, string versionSetName, string fileFolder)
        {
            APIExtractor apiExtractor = new APIExtractor();
            Template armTemplate = new Template()
            {
                schema = "https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#",
                contentVersion = "1.0.0.0",
                parameters = new Dictionary<string, TemplateParameterProperties>
                {
                    { "ApimServiceName", new TemplateParameterProperties(){ type = "string" } }
                },
                variables = { },
                resources = { },
                outputs = { }
            };

            List<TemplateResource> templateResources = new List<TemplateResource>();            

            string versionSet = apiExtractor.GetAPIVersionSet(apimname, resourceGroup, versionSetName).Result;
            APIVersionSetTemplateResource versionSetResource = JsonConvert.DeserializeObject<APIVersionSetTemplateResource>(versionSet);

            string filePath = fileFolder + Path.DirectorySeparatorChar + string.Format(versionSetResource.name, "/", "-") + ".json";

            versionSetResource.name = $"[concat(parameters('ApimServiceName'), '/{versionSetResource.name}')]";
            versionSetResource.apiVersion = "2018-06-01-preview";

            templateResources.Add(versionSetResource);
            armTemplate.resources = templateResources.ToArray();

            FileWriter fileWriter = new FileWriter();
            fileWriter.WriteJSONToFile(armTemplate, filePath);
        }
        private void GenerateProductsARMTemplate(string apimname, string resourceGroup, string fileFolder)
        {
            APIExtractor apiExtractor = new APIExtractor();
            Template armTemplate = new Template()
            {
                schema = "https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#",
                contentVersion = "1.0.0.0",
                parameters = new Dictionary<string, TemplateParameterProperties>
                {
                    { "ApimServiceName", new TemplateParameterProperties(){ type = "string" } }
                },
                variables = { },
                resources = { },
                outputs = { }
            };

            List<TemplateResource> templateResources = new List<TemplateResource>();

            string products = apiExtractor.GetProducts(apimname, resourceGroup).Result;
            JObject oProducts = JObject.Parse(products);

            foreach (var item in oProducts["value"])
            {
                string productName = ((JValue)item["name"]).Value.ToString();

                Console.WriteLine("'{0}' Product found", productName);

                string productDetails = apiExtractor.GetProductDetails(apimname, resourceGroup, productName).Result;

                ProductsDetailsTemplateResource productsDetailsResource = JsonConvert.DeserializeObject<ProductsDetailsTemplateResource>(productDetails);
                productsDetailsResource.name = $"[concat(parameters('ApimServiceName'), '/{productName}')]";
                productsDetailsResource.apiVersion = "2018-06-01-preview";

                templateResources.Add(productsDetailsResource);

            }

            armTemplate.resources = templateResources.ToArray();
            FileWriter fileWriter = new FileWriter();
            fileWriter.WriteJSONToFile(armTemplate, @fileFolder + Path.DirectorySeparatorChar + apimname + "-products.json");

        }
        private async void GenerateLoggerTemplate(string resourceGroup, string apimname, string fileFolder)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Geting loggers from service");
            LoggerExtractor loggerExtractor = new LoggerExtractor();
            PropertyExtractor propertyExtractor = new PropertyExtractor();
            Template armTemplate = new Template()
            {
                schema = "https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#",
                contentVersion = "1.0.0.0",
                parameters = new Dictionary<string, TemplateParameterProperties>
                {
                    { "ApimServiceName", new TemplateParameterProperties(){ type = "string" } }
                },
                variables = { },
                resources = { },
                outputs = { }
            };

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // pull named values for later credential reference
            string properties = propertyExtractor.GetProperties(apimname, resourceGroup).Result;
            JObject oProperties = JObject.Parse(properties);
            List<PropertyTemplateResource> propertyResources = oProperties["value"].ToObject<List<PropertyTemplateResource>>();

            string loggers = loggerExtractor.GetLoggers(apimname, resourceGroup).Result;
            JObject oLoggers = JObject.Parse(loggers);
            foreach (var extractedLogger in oLoggers["value"])
            {
                string loggerName = ((JValue)extractedLogger["name"]).Value.ToString();
                Console.WriteLine("'{0}' Logger found", loggerName);

                string fullLoggerResource = await loggerExtractor.GetLogger(apimname, resourceGroup, loggerName);
                LoggerTemplateResource loggerResource = JsonConvert.DeserializeObject<LoggerTemplateResource>(fullLoggerResource);
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

                templateResources.Add(loggerResource);
            }

            armTemplate.resources = templateResources.ToArray();
            FileWriter fileWriter = new FileWriter();
            string filePath = fileFolder + Path.DirectorySeparatorChar + string.Format("loggers", "/", "-") + ".json";
            fileWriter.WriteJSONToFile(armTemplate, filePath);
        }
    }
}
