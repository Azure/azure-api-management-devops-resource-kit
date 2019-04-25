using System;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;
using System.Linq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class ExtractCommand : CommandLineApplication
    {
        public ExtractCommand()
        {
            this.Name = GlobalConstants.ExtractName;
            this.Description = GlobalConstants.ExtractDescription;

            var apiManagementName = this.Option("--name <apimname>", "API Management name", CommandOptionType.SingleValue);
            var resourceGroupName = this.Option("--resourceGroup <resourceGroup>", "Resource Group name", CommandOptionType.SingleValue);
            var fileFolderName = this.Option("--fileFolder <filefolder>", "ARM Template files folder", CommandOptionType.SingleValue);
            var apiName = this.Option("--apiName <apiName>", "API name", CommandOptionType.SingleValue);

            this.HelpOption();

            this.OnExecute(() =>
            {
                if (!apiManagementName.HasValue()) throw new Exception("Missing parameter <apimname>.");
                if (!resourceGroupName.HasValue()) throw new Exception("Missing parameter <resourceGroup>.");
                if (!fileFolderName.HasValue()) throw new Exception("Missing parameter <filefolder>.");

                string resourceGroup = resourceGroupName.Values[0].ToString();
                string apimname = apiManagementName.Values[0].ToString();
                string fileFolder = fileFolderName.Values[0].ToString();
                string singleApiName = null;

                if (apiName.Values.Count > 0)
                {
                    singleApiName = apiName.Values[0].ToString();
                }

                Console.WriteLine("API Management Template");
                Console.WriteLine();
                Console.WriteLine("Connecting to {0} API Management Service on {1} Resource Group ...", apimname, resourceGroup);

                GenerateARMTemplate(apimname, resourceGroup, fileFolder, singleApiName);

                Console.WriteLine("Templates written to output location");
                Console.WriteLine("Press any key to exit process:");
#if DEBUG
                Console.ReadKey();
#endif
            });
        }

        private void GenerateARMTemplate(string apimname, string resourceGroup, string fileFolder, string singleApiName)
        {
            #region API's
            FileWriter fileWriter;
            APIExtractor apiExtractor = new APIExtractor();
            string apis = apiExtractor.GetAPIs(apimname, resourceGroup).Result;
            Template armTemplate = GenerateEmptyTemplateWithParameters();

            JObject oApi = JObject.Parse(apis);
            oApi = FormatoApi(singleApiName, oApi);

            Console.WriteLine("{0} API's found ...", ((JContainer)oApi["value"]).Count.ToString());

            List<TemplateResource> templateResources = new List<TemplateResource>();

            for (int i = 0; i < ((JContainer)oApi["value"]).Count; i++)
            {
                string apiName = ((JValue)oApi["value"][i]["name"]).Value.ToString();
                string apiDetails = apiExtractor.GetAPIDetails(apimname, resourceGroup, apiName).Result;

                Console.WriteLine("------------------------------------------");
                Console.WriteLine("Getting operations from {0} API:", apiName);

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

                #region Schemas
                List<TemplateResource> schemaResources = GenerateSchemasARMTemplate(apimname, apiName, resourceGroup, fileFolder);
                templateResources.AddRange(schemaResources);
                #endregion

                #region Operations

                string operations = apiExtractor.GetAPIOperations(apimname, resourceGroup, apiName).Result;
                JObject oOperations = JObject.Parse(operations);

                foreach (var item in oOperations["value"])
                {
                    string operationName = ((JValue)item["name"]).Value.ToString();
                    string operationDetails = apiExtractor.GetAPIOperationDetail(apimname, resourceGroup, apiName, operationName).Result;

                    Console.WriteLine("'{0}' Operation found", operationName);

                    OperationTemplateResource operationResource = JsonConvert.DeserializeObject<OperationTemplateResource>(operationDetails);
                    string operationResourceName = operationResource.name;
                    operationResource.name = $"[concat(parameters('ApimServiceName'), '/{oApiName}/{operationResourceName}')]";
                    operationResource.apiVersion = "2018-06-01-preview";
                    operationResource.scale = null;

                    // depend on api and schemas if necessary
                    List<string> operationDependsOn = new List<string>() { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('ApimServiceName'), '{oApiName}')]" };
                    foreach (OperationTemplateRepresentation operationTemplateRepresentation in operationResource.properties.request.representations)
                    {
                        if (operationTemplateRepresentation.schemaId != null)
                        {
                            string dependsOn = $"[resourceId('Microsoft.ApiManagement/service/apis/schemas', parameters('ApimServiceName'), '{oApiName}', '{operationTemplateRepresentation.schemaId}')]";
                            // add value to list if schema has not already been added
                            if (!operationDependsOn.Exists(o => o == dependsOn))
                            {
                                operationDependsOn.Add(dependsOn);
                            }
                        }
                    }
                    foreach (OperationsTemplateResponse operationTemplateResponse in operationResource.properties.responses)
                    {
                        foreach (OperationTemplateRepresentation operationTemplateRepresentation in operationTemplateResponse.representations)
                        {
                            if (operationTemplateRepresentation.schemaId != null)
                            {
                                string dependsOn = $"[resourceId('Microsoft.ApiManagement/service/apis/schemas', parameters('ApimServiceName'), '{oApiName}', '{operationTemplateRepresentation.schemaId}')]";
                                // add value to list if schema has not already been added
                                if (!operationDependsOn.Exists(o => o == dependsOn))
                                {
                                    operationDependsOn.Add(dependsOn);
                                }
                            }
                        }
                    }
                    operationResource.dependsOn = operationDependsOn.ToArray();

                    templateResources.Add(operationResource);
                    try
                    {
                        string operationPolicy = apiExtractor.GetOperationPolicy(apimname, resourceGroup, oApiName, operationName).Result;
                        Console.WriteLine($" - Policy found to {operationName} operation");
                        PolicyTemplateResource operationPolicyResource = JsonConvert.DeserializeObject<PolicyTemplateResource>(operationPolicy);
                        operationPolicyResource.name = $"[concat(parameters('ApimServiceName'), '/{oApiName}/{operationResourceName}/policy')]";
                        operationPolicyResource.apiVersion = "2018-06-01-preview";
                        operationPolicyResource.scale = null;
                        operationPolicyResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis/operations', parameters('ApimServiceName'), '{oApiName}', '{operationResourceName}')]" };

                        templateResources.Add(operationPolicyResource);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($" - No policy found for {operationName} operation");
                    }
                }
                #endregion

                #region API Policies
                try
                {
                    Console.WriteLine("Getting API Policy from {0} API: ", apiName);
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

                #region API Products
                try
                {
                    Console.WriteLine("Getting API Products from {0} API: ", apiName);
                    string apiProducts = apiExtractor.GetApiProducts(apimname, resourceGroup, apiName).Result;
                    JObject oApiProducts = JObject.Parse(apiProducts);

                    foreach (var item in oApiProducts["value"])
                    {
                        string apiProductName = ((JValue)item["name"]).Value.ToString();
                        Console.WriteLine($" -- {apiProductName} Product found to {oApiName} API");
                        ProductAPITemplateResource productAPIResource = JsonConvert.DeserializeObject<ProductAPITemplateResource>(apiProducts);
                        productAPIResource.type = ResourceTypeConstants.ProductAPI;
                        productAPIResource.name = $"[concat(parameters('ApimServiceName'), '/{apiProductName}/{oApiName}')]";
                        productAPIResource.apiVersion = "2018-06-01-preview";
                        productAPIResource.scale = null;
                        productAPIResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('ApimServiceName'), '{oApiName}')]" };

                        templateResources.Add(productAPIResource);

                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("No API products!");
                }
                #endregion

                #region Diagnostics

                Console.WriteLine("------------------------------------------");
                Console.WriteLine("Getting diagnostics from {0} API:", apiName);
                string diagnostics = apiExtractor.GetAPIDiagnostics(apimname, resourceGroup, apiName).Result;
                JObject oDiagnostics = JObject.Parse(diagnostics);
                foreach (var diagnostic in oDiagnostics["value"])
                {
                    string diagnosticName = ((JValue)diagnostic["name"]).Value.ToString();
                    Console.WriteLine("'{0}' Diagnostic found", diagnosticName);

                    DiagnosticTemplateResource diagnosticResource = diagnostic.ToObject<DiagnosticTemplateResource>();
                    diagnosticResource.name = $"[concat(parameters('ApimServiceName'), '/{oApiName}/{diagnosticName}')]";
                    diagnosticResource.type = ResourceTypeConstants.APIDiagnostic;
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

                armTemplate.resources = templateResources.ToArray();

                if (singleApiName != null)
                {
                    fileWriter = new FileWriter();
                    fileWriter.WriteJSONToFile(armTemplate, @fileFolder + Path.DirectorySeparatorChar + apimname + "-" + oApiName + "-template.json");
                }
            }

            // extract resources that do not fall under api. Pass in the single api name and associated resources for the single api case
            GenerateProductsARMTemplate(apimname, resourceGroup, fileFolder, singleApiName, templateResources);
            GenerateNamedValuesTemplate(resourceGroup, apimname, fileFolder, singleApiName, templateResources);
            GenerateLoggerTemplate(resourceGroup, apimname, fileFolder, singleApiName, templateResources);

            if (singleApiName == null)
            {
                fileWriter = new FileWriter();
                fileWriter.WriteJSONToFile(armTemplate, @fileFolder + Path.DirectorySeparatorChar + apimname + "-apis-template.json");
            }
            #endregion
        }
        private static JObject FormatoApi(string singleApiName, JObject oApi)
        {
            if (singleApiName != null)
            {
                string json = @"{ 'value': [] }";

                JObject value = JObject.Parse(json);
                JArray item2 = new JArray();
                var objectSelector = string.Format("$.value[?(@.name == '{0}')]", singleApiName);
                var selectedApi = (JObject)oApi.SelectTokens(objectSelector).FirstOrDefault();
                if (selectedApi == null)
                {
                    throw new Exception($"{singleApiName} API not found!");
                }
                item2.Add(selectedApi);
                value["value"] = item2;
                oApi = value;
            }

            return oApi;
        }
        private void GenerateVersionSetARMTemplate(string apimname, string resourceGroup, string versionSetName, string fileFolder)
        {
            APIExtractor apiExtractor = new APIExtractor();
            Template armTemplate = GenerateEmptyTemplateWithParameters();

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

        private void GenerateProductsARMTemplate(string apimname, string resourceGroup, string fileFolder, string singleApiName, List<TemplateResource> armTemplateResources)
        {
            APIExtractor apiExtractor = new APIExtractor();
            Template armTemplate = GenerateEmptyTemplateWithParameters();

            // isolate product api associations in the case of a single api extraction
            var productAPIResources = armTemplateResources.Where(resource => resource.type == ResourceTypeConstants.ProductAPI);

            List<TemplateResource> templateResources = new List<TemplateResource>();

            string products = apiExtractor.GetProducts(apimname, resourceGroup).Result;
            JObject oProducts = JObject.Parse(products);

            foreach (var item in oProducts["value"])
            {
                string productName = ((JValue)item["name"]).Value.ToString();

                Console.WriteLine("'{0}' Product found", productName);

                string productDetails = apiExtractor.GetProductDetails(apimname, resourceGroup, productName).Result;

                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                ProductsTemplateResource productsTemplateResource = JsonConvert.DeserializeObject<ProductsTemplateResource>(productDetails, settings);
                productsTemplateResource.name = $"[concat(parameters('ApimServiceName'), '/{productName}')]";
                productsTemplateResource.apiVersion = "2018-06-01-preview";

                // only extract the product if this is a full extraction, or in the case of a single api, if it is found in products associated with the api
                if (singleApiName == null || productAPIResources.SingleOrDefault(p => p.name.Contains(productName)) != null)
                {
                    templateResources.Add(productsTemplateResource);
                }
            }

            armTemplate.resources = templateResources.ToArray();
            FileWriter fileWriter = new FileWriter();
            fileWriter.WriteJSONToFile(armTemplate, @fileFolder + Path.DirectorySeparatorChar + apimname + "-products.json");

        }

        private List<TemplateResource> GenerateSchemasARMTemplate(string apimServiceName, string apiName, string resourceGroup, string fileFolder)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Getting operation schemas from service");

            APIExtractor apiExtractor = new APIExtractor();
            List<TemplateResource> templateResources = new List<TemplateResource>();

            string schemas = apiExtractor.GetApiSchemas(apimServiceName, resourceGroup, apiName).Result;
            JObject oSchemas = JObject.Parse(schemas);

            foreach (var item in oSchemas["value"])
            {
                string schemaName = ((JValue)item["name"]).Value.ToString();
                Console.WriteLine("'{0}' Schema found", schemaName);

                string schemaDetails = apiExtractor.GetApiSchemaDetails(apimServiceName, resourceGroup, apiName, schemaName).Result;

                // pull returned document and convert to correct format
                RESTReturnedSchemaTemplate restReturnedSchemaTemplate = JsonConvert.DeserializeObject<RESTReturnedSchemaTemplate>(schemaDetails);
                SchemaTemplateResource schemaDetailsResource = JsonConvert.DeserializeObject<SchemaTemplateResource>(schemaDetails);
                schemaDetailsResource.properties.document.value = JsonConvert.SerializeObject(restReturnedSchemaTemplate.properties.document);
                schemaDetailsResource.name = $"[concat(parameters('ApimServiceName'), '/{apiName}/{schemaName}')]";
                schemaDetailsResource.apiVersion = "2018-06-01-preview";
                schemaDetailsResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('ApimServiceName'), '{apiName}')]" };

                templateResources.Add(schemaDetailsResource);

            }
            return templateResources;
        }

        private async void GenerateLoggerTemplate(string resourceGroup, string apimname, string fileFolder, string singleApiName, List<TemplateResource> armTemplateResources)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Getting loggers from service");
            LoggerExtractor loggerExtractor = new LoggerExtractor();
            Template armTemplate = GenerateEmptyTemplateWithParameters();

            // isolate product api associations in the case of a single api extraction
            var diagnosticResources = armTemplateResources.Where(resource => resource.type == ResourceTypeConstants.APIDiagnostic);
            var policyResources = armTemplateResources.Where(resource => (resource.type == ResourceTypeConstants.APIPolicy || resource.type == ResourceTypeConstants.APIOperationPolicy));

            List<TemplateResource> templateResources = new List<TemplateResource>();

            string loggers = loggerExtractor.GetLoggers(apimname, resourceGroup).Result;
            JObject oLoggers = JObject.Parse(loggers);
            foreach (var extractedLogger in oLoggers["value"])
            {
                string loggerName = ((JValue)extractedLogger["name"]).Value.ToString();

                string fullLoggerResource = await loggerExtractor.GetLogger(apimname, resourceGroup, loggerName);
                LoggerTemplateResource loggerResource = JsonConvert.DeserializeObject<LoggerTemplateResource>(fullLoggerResource);
                loggerResource.name = $"[concat(parameters('ApimServiceName'), '/{loggerName}')]";
                loggerResource.type = ResourceTypeConstants.Logger;
                loggerResource.apiVersion = "2018-06-01-preview";
                loggerResource.scale = null;

                if (singleApiName == null)
                {
                    // if the user is extracting all apis, extract all the loggers
                    Console.WriteLine("'{0}' Logger found", loggerName);
                    templateResources.Add(loggerResource);
                }
                else
                {
                    // if the user is extracting a single api, extract the loggers referenced by its diagnostics and api policies
                    bool isReferencedInPolicy = false;
                    bool isReferencedInDiagnostic = false;
                    foreach (PolicyTemplateResource policyTemplateResource in policyResources)
                    {
                        if (policyTemplateResource.properties.policyContent.Contains(loggerName))
                        {
                            isReferencedInPolicy = true;
                        }
                    }
                    foreach (DiagnosticTemplateResource diagnosticTemplateResource in diagnosticResources)
                    {
                        if (diagnosticTemplateResource.properties.loggerId.Contains(loggerName))
                        {
                            isReferencedInPolicy = true;
                        }
                    }
                    if (isReferencedInPolicy == true || isReferencedInDiagnostic == true)
                    {
                        // logger was used in policy or diagnostic, extract it
                        Console.WriteLine("'{0}' Logger found", loggerName);
                        templateResources.Add(loggerResource);
                    }
                };
            }

            armTemplate.resources = templateResources.ToArray();
            FileWriter fileWriter = new FileWriter();
            fileWriter.WriteJSONToFile(armTemplate, @fileFolder + Path.DirectorySeparatorChar + apimname + "-loggers.json");
        }

        private async void GenerateNamedValuesTemplate(string resourceGroup, string apimname, string fileFolder, string singleApiName, List<TemplateResource> armTemplateResources)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Getting named values from service");
            PropertyExtractor propertyExtractor = new PropertyExtractor();
            Template armTemplate = GenerateEmptyTemplateWithParameters();

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // pull named values for later credential reference
            string properties = propertyExtractor.GetProperties(apimname, resourceGroup).Result;
            JObject oProperties = JObject.Parse(properties);
            foreach (var extractedProperty in oProperties["value"])
            {
                string propertyName = ((JValue)extractedProperty["name"]).Value.ToString();

                string fullLoggerResource = await propertyExtractor.GetProperty(apimname, resourceGroup, propertyName);
                PropertyTemplateResource propertyTemplateResource = JsonConvert.DeserializeObject<PropertyTemplateResource>(fullLoggerResource);
                propertyTemplateResource.name = $"[concat(parameters('ApimServiceName'), '/{propertyName}')]";
                propertyTemplateResource.type = ResourceTypeConstants.Property;
                propertyTemplateResource.apiVersion = "2018-06-01-preview";
                propertyTemplateResource.scale = null;

                if (singleApiName == null)
                {
                    // if the user is executing a full extraction, extract all the loggers
                    Console.WriteLine("'{0}' Named value found", propertyName);
                    templateResources.Add(propertyTemplateResource);
                }
                else
                {
                    // TODO - if the user is executing a single api, extract all the named values used in the template resources
                    Console.WriteLine("'{0}' Named value found", propertyName);
                    templateResources.Add(propertyTemplateResource);
                };
            }

            armTemplate.resources = templateResources.ToArray();
            FileWriter fileWriter = new FileWriter();
            fileWriter.WriteJSONToFile(armTemplate, @fileFolder + Path.DirectorySeparatorChar + apimname + "-namedvalues.json");
        }

        public Template GenerateEmptyTemplateWithParameters()
        {
            TemplateCreator templateCreator = new TemplateCreator();
            Template armTemplate = templateCreator.CreateEmptyTemplate();
            armTemplate.parameters = new Dictionary<string, TemplateParameterProperties> { { "ApimServiceName", new TemplateParameterProperties() { type = "string" } } };
            return armTemplate;
        }
    }
}
