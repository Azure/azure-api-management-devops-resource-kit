using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class APIExtractor : EntityExtractor
    {
        public async Task<string> GetAPIOperations(string ApiManagementName, string ResourceGroupName, string ApiName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/operations?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, GlobalConstants.APIVersion);

            return await CallApiManagement(azToken, requestUrl);
        }

        public async Task<string> GetAPIOperationDetail(string ApiManagementName, string ResourceGroupName, string ApiName, string OperationName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/operations/{5}?api-version={6}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, OperationName, GlobalConstants.APIVersion);

            return await CallApiManagement(azToken, requestUrl);
        }

        public async Task<string> GetOperationPolicy(string ApiManagementName, string ResourceGroupName, string ApiName, string OperationId)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/operations/{5}/policies/policy?api-version={6}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, OperationId, GlobalConstants.APIVersion);

            return await CallApiManagement(azToken, requestUrl);
        }

        public async Task<string> GetAPIDetails(string ApiManagementName, string ResourceGroupName, string ApiName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, GlobalConstants.APIVersion);

            return await CallApiManagement(azToken, requestUrl);
        }

        public async Task<string> GetAPIVersionSet(string ApiManagementName, string ResourceGroupName, string VersionSetName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/{4}?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, VersionSetName, GlobalConstants.APIVersion);

            return await CallApiManagement(azToken, requestUrl);
        }

        public async Task<string> GetAPIs(string ApiManagementName, string ResourceGroupName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis?api-version={4}",
                baseUrl, azSubId, ResourceGroupName, ApiManagementName, GlobalConstants.APIVersion);

            return await CallApiManagement(azToken, requestUrl);
        }

        public async Task<string> GetAPIPolicies(string ApiManagementName, string ResourceGroupName, string ApiName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/policies/policy?api-version={5}",
                baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, GlobalConstants.APIVersion);

            return await CallApiManagement(azToken, requestUrl);
        }

        public async Task<string> GetAPIDiagnostics(string ApiManagementName, string ResourceGroupName, string ApiName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/diagnostics?api-version={5}",
                baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, GlobalConstants.APIVersion);

            return await CallApiManagement(azToken, requestUrl);
        }

        public async Task<string> GetAPIOperationPolicies(string ApiManagementName, string ResourceGroupName, string ApiName, string OperationName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/operations/get/policies/policy?api-version={5}",
                baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, GlobalConstants.APIVersion);

            return await CallApiManagement(azToken, requestUrl);
        }

        public async Task<string> GetAPIProducts(string ApiManagementName, string ResourceGroupName, string ApiName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/products?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, GlobalConstants.APIVersion);

            return await CallApiManagement(azToken, requestUrl);
        }

        public async Task<string> GetAPISchemas(string ApiManagementName, string ResourceGroupName, string ApiName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/schemas?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, GlobalConstants.APIVersion);

            return await CallApiManagement(azToken, requestUrl);
        }

        public async Task<string> GetAPISchemaDetails(string ApiManagementName, string ResourceGroupName, string ApiName, string schemaName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/schemas/{5}?api-version={6}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, schemaName, GlobalConstants.APIVersion);

            return await CallApiManagement(azToken, requestUrl);
        }

        public async Task<Template> GenerateAPIsARMTemplate(string apimname, string resourceGroup, string fileFolder, string singleApiName)
        {
            // pull all apis from service
            string apis = await GetAPIs(apimname, resourceGroup);
            // initialize arm template
            Template armTemplate = GenerateEmptyTemplateWithParameters();

            JObject oApi = JObject.Parse(apis);
            oApi = FormatoApi(singleApiName, oApi);

            Console.WriteLine("{0} API's found ...", ((JContainer)oApi["value"]).Count.ToString());

            List<TemplateResource> templateResources = new List<TemplateResource>();

            for (int i = 0; i < ((JContainer)oApi["value"]).Count; i++)
            {
                string apiName = ((JValue)oApi["value"][i]["name"]).Value.ToString();
                string apiDetails = await GetAPIDetails(apimname, resourceGroup, apiName);

                Console.WriteLine("------------------------------------------");
                Console.WriteLine("Getting operations from {0} API:", apiName);

                // convert returned api to template resource class
                JObject oApiDetails = JObject.Parse(apiDetails);
                APITemplateResource apiResource = JsonConvert.DeserializeObject<APITemplateResource>(apiDetails);
                string oApiName = ((JValue)oApiDetails["name"]).Value.ToString();

                apiResource.type = ((JValue)oApiDetails["type"]).Value.ToString();
                apiResource.name = $"[concat(parameters('ApimServiceName'), '/{oApiName}')]";
                apiResource.apiVersion = GlobalConstants.APIVersion;
                apiResource.scale = null;

                if (apiResource.properties.apiVersionSetId != null)
                {
                    // extract the version set if referenced by an
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
                // add schema resources to api template
                List<TemplateResource> schemaResources = await GenerateSchemasARMTemplate(apimname, apiName, resourceGroup, fileFolder);
                templateResources.AddRange(schemaResources);
                #endregion

                #region Operations

                // pull api operations for service
                string operations = await GetAPIOperations(apimname, resourceGroup, apiName);
                JObject oOperations = JObject.Parse(operations);

                foreach (var item in oOperations["value"])
                {
                    string operationName = ((JValue)item["name"]).Value.ToString();
                    string operationDetails = await GetAPIOperationDetail(apimname, resourceGroup, apiName, operationName);

                    Console.WriteLine("'{0}' Operation found", operationName);

                    // convert returned operation to template resource class
                    OperationTemplateResource operationResource = JsonConvert.DeserializeObject<OperationTemplateResource>(operationDetails);
                    string operationResourceName = operationResource.name;
                    operationResource.name = $"[concat(parameters('ApimServiceName'), '/{oApiName}/{operationResourceName}')]";
                    operationResource.apiVersion = GlobalConstants.APIVersion;
                    operationResource.scale = null;

                    // add api and schemas to operation dependsOn, if necessary
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

                    // add operation policy resource to api template
                    try
                    {
                        string operationPolicy = await GetOperationPolicy(apimname, resourceGroup, oApiName, operationName);
                        Console.WriteLine($" - Policy found to {operationName} operation");
                        PolicyTemplateResource operationPolicyResource = JsonConvert.DeserializeObject<PolicyTemplateResource>(operationPolicy);
                        operationPolicyResource.name = $"[concat(parameters('ApimServiceName'), '/{oApiName}/{operationResourceName}/policy')]";
                        operationPolicyResource.apiVersion = GlobalConstants.APIVersion;
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
                // add api policy resource to api template
                try
                {
                    Console.WriteLine("Getting API Policy from {0} API: ", apiName);
                    string apiPolicies = await GetAPIPolicies(apimname, resourceGroup, apiName);
                    Console.WriteLine("API Policy found!");
                    PolicyTemplateResource apiPoliciesResource = JsonConvert.DeserializeObject<PolicyTemplateResource>(apiPolicies);

                    apiPoliciesResource.apiVersion = GlobalConstants.APIVersion;
                    apiPoliciesResource.name = $"[concat(parameters('ApimServiceName'), '/{oApiName}/{apiPoliciesResource.name}')]";
                    apiPoliciesResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('ApimServiceName'), '{apiName}')]" };

                    templateResources.Add(apiPoliciesResource);
                }
                catch (Exception)
                {
                    Console.WriteLine("No API policy!");
                }
                #endregion

                // add product api associations to template
                #region API Products
                try
                {
                    Console.WriteLine("Getting API Products from {0} API: ", apiName);

                    // pull product api associations
                    string apiProducts = await GetAPIProducts(apimname, resourceGroup, apiName);
                    JObject oApiProducts = JObject.Parse(apiProducts);

                    foreach (var item in oApiProducts["value"])
                    {
                        string apiProductName = ((JValue)item["name"]).Value.ToString();
                        Console.WriteLine($" -- {apiProductName} Product found to {oApiName} API");

                        // convert returned api product associations to template resource class
                        ProductAPITemplateResource productAPIResource = JsonConvert.DeserializeObject<ProductAPITemplateResource>(apiProducts);
                        productAPIResource.type = ResourceTypeConstants.ProductAPI;
                        productAPIResource.name = $"[concat(parameters('ApimServiceName'), '/{apiProductName}/{oApiName}')]";
                        productAPIResource.apiVersion = GlobalConstants.APIVersion;
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
                // add diagnostics to template
                Console.WriteLine("------------------------------------------");
                Console.WriteLine("Getting diagnostics from {0} API:", apiName);
                // pull diagnostics for api
                string diagnostics = await GetAPIDiagnostics(apimname, resourceGroup, apiName);
                JObject oDiagnostics = JObject.Parse(diagnostics);
                foreach (var diagnostic in oDiagnostics["value"])
                {
                    string diagnosticName = ((JValue)diagnostic["name"]).Value.ToString();
                    Console.WriteLine("'{0}' Diagnostic found", diagnosticName);

                    // convert returned diagnostic to template resource class
                    DiagnosticTemplateResource diagnosticResource = diagnostic.ToObject<DiagnosticTemplateResource>();
                    diagnosticResource.name = $"[concat(parameters('ApimServiceName'), '/{oApiName}/{diagnosticName}')]";
                    diagnosticResource.type = ResourceTypeConstants.APIDiagnostic;
                    diagnosticResource.apiVersion = GlobalConstants.APIVersion;
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

            armTemplate.resources = templateResources.ToArray();
            return armTemplate;
        }

        public static JObject FormatoApi(string singleApiName, JObject oApi)
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

        public async void GenerateVersionSetARMTemplate(string apimname, string resourceGroup, string versionSetName, string fileFolder)
        {
            Template armTemplate = GenerateEmptyTemplateWithParameters();

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // pull version set resource
            string versionSet = await GetAPIVersionSet(apimname, resourceGroup, versionSetName);
            // convert returned version set to template resource
            APIVersionSetTemplateResource versionSetResource = JsonConvert.DeserializeObject<APIVersionSetTemplateResource>(versionSet);

            string filePath = fileFolder + Path.DirectorySeparatorChar + string.Format(versionSetResource.name, "/", "-") + ".json";

            versionSetResource.name = $"[concat(parameters('ApimServiceName'), '/{versionSetResource.name}')]";
            versionSetResource.apiVersion = GlobalConstants.APIVersion;

            templateResources.Add(versionSetResource);
            armTemplate.resources = templateResources.ToArray();

            // write version set template to output file location
            FileWriter fileWriter = new FileWriter();
            fileWriter.WriteJSONToFile(armTemplate, filePath);
        }

        public async Task<List<TemplateResource>> GenerateSchemasARMTemplate(string apimServiceName, string apiName, string resourceGroup, string fileFolder)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Getting operation schemas from service");
            List<TemplateResource> templateResources = new List<TemplateResource>();

            // pull all schemas from service
            string schemas = await GetAPISchemas(apimServiceName, resourceGroup, apiName);
            JObject oSchemas = JObject.Parse(schemas);

            foreach (var item in oSchemas["value"])
            {
                string schemaName = ((JValue)item["name"]).Value.ToString();
                Console.WriteLine("'{0}' Schema found", schemaName);

                string schemaDetails = await GetAPISchemaDetails(apimServiceName, resourceGroup, apiName, schemaName);

                // pull returned schema and convert to template resource
                RESTReturnedSchemaTemplate restReturnedSchemaTemplate = JsonConvert.DeserializeObject<RESTReturnedSchemaTemplate>(schemaDetails);
                SchemaTemplateResource schemaDetailsResource = JsonConvert.DeserializeObject<SchemaTemplateResource>(schemaDetails);
                schemaDetailsResource.properties.document.value = JsonConvert.SerializeObject(restReturnedSchemaTemplate.properties.document);
                schemaDetailsResource.name = $"[concat(parameters('ApimServiceName'), '/{apiName}/{schemaName}')]";
                schemaDetailsResource.apiVersion = GlobalConstants.APIVersion;
                schemaDetailsResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('ApimServiceName'), '{apiName}')]" };

                templateResources.Add(schemaDetailsResource);

            }
            return templateResources;
        }
    }
}