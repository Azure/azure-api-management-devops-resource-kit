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

        private void GenerateARMTemplate(string apimname, string resourceGroup, string fileFolder )
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
                apiResource.apiVersion = "2018-01-01";
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
                    operationResource.apiVersion = "2018-01-01";
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

                    apiPoliciesResource.apiVersion = "2018-01-01";
                    apiPoliciesResource.name = $"[concat(parameters('ApimServiceName'), '/{oApiName}/{apiPoliciesResource.name}')]";
                    apiPoliciesResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('ApimServiceName'), '{apiName}')]" };

                    armTemplate.resources.Add(apiPoliciesResource);
                }
                catch (Exception)
                {
                    Console.WriteLine("Policy NOT found!");     
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
            versionSetResource.apiVersion = "2018-01-01";
            armTemplate.resources.Add(versionSetResource);

            FileWriter fileWriter = new FileWriter();

            fileWriter.WriteJSONToFile(armTemplate, filePath);
        }
    }
}
