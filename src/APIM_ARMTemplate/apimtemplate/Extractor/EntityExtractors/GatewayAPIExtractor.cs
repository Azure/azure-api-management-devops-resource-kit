using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class GatewayAPIExtractor : GatewayExtractor
    {   
        public async Task<List<TemplateResource>> GenerateGatewayAPIResourceAsync(string apiName, Extractor exc, string[] dependsOn)
        {
            List<TemplateResource> templateResources = new List<TemplateResource>();
            string apiManagementName = exc.sourceApimName, resourceGroupName = exc.resourceGroup;

            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting gateways from {0} API:", apiName);

            try
            {
                // Get Gateways
                JObject oGateways = new JObject();
                int skipNumberOfGateways = 0;

                do
                {
                    string gateways = await GetGatewaysAsync(apiManagementName, resourceGroupName, skipNumberOfGateways);
                    oGateways = JObject.Parse(gateways);
                    string lastGatewayAPIName = null;
                    foreach (var item in oGateways["value"])
                    {
                        string gatewayId = ((JValue)item["name"]).Value.ToString();
                        bool doesApiReferenceGateway = await DoesApiReferenceGatewayAsync(apiManagementName, resourceGroupName, gatewayId, apiName);

                        // If the API references the Gateway
                        if (doesApiReferenceGateway)
                        {
                            Console.WriteLine("'{0}' gateway association found", gatewayId);

                            // convert returned gateway to template resource class
                            GatewayAPITemplateResource gatewayAPIResource = new GatewayAPITemplateResource();
                            gatewayAPIResource.type = ResourceTypeConstants.GatewayAPI;
                            gatewayAPIResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{gatewayId}/{apiName}')]";
                            gatewayAPIResource.apiVersion = GlobalConstants.APIVersion;
                            gatewayAPIResource.scale = null;
                            gatewayAPIResource.dependsOn = (lastGatewayAPIName != null) ? new string[] { lastGatewayAPIName } : dependsOn;
                            gatewayAPIResource.properties = new GatewayAPITemplateResource.GatewayAPIProperties { provisioningState = "created" };

                            lastGatewayAPIName = $"[resourceId('Microsoft.ApiManagement/service/gateways/apis', parameters('{ParameterNames.ApimServiceName}'), '{gatewayId}', '{apiName}')]";

                            templateResources.Add(gatewayAPIResource);
                        }
                    }

                    skipNumberOfGateways += GlobalConstants.NumOfRecords;
                }
                while (oGateways["nextLink"] != null);
            }
            catch (Exception) { }

            return templateResources;
        }

        public async Task<Template> GenerateGatewayAPIARMTemplateAsync(string singleApiName, List<string> multipleApiNames, Extractor exc)
        {
            string apiManagementName = exc.sourceApimName, resourceGroupName = exc.resourceGroup;

            // initialize arm template
            Template armTemplate = GenerateEmptyPropertyTemplateWithParameters();

            List<TemplateResource> templateResources = new List<TemplateResource>();
            // when extract single API
            if (singleApiName != null)
            {
                // check if this api exist
                try
                {   
                    templateResources.AddRange(await GenerateGatewayAPIResourceAsync(singleApiName, exc, new string[] { }));
                }
                catch (Exception)
                {
                    throw new Exception($"{singleApiName} API not found!");
                }
            }
            // when extract multiple APIs and generate one master template
            else if (multipleApiNames != null)
            {
                Console.WriteLine("{0} APIs found ...", multipleApiNames.Count().ToString());

                string[] dependsOn = new string[] { };
                foreach (string apiName in multipleApiNames)
                {
                    templateResources.AddRange(await GenerateGatewayAPIResourceAsync(apiName, exc, dependsOn));
                    string apiProductName = templateResources.Last().name.Split('/', 3)[1];
                    dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/gateways/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiProductName}', '{apiName}')]" };
                }
            }
            // when extract all APIs and generate one master template
            else
            {
                // Get Gateways
                JObject oGateways = new JObject();
                int skipNumberOfGateways = 0;

                do
                {
                    string gateways = await GetGatewaysAsync(apiManagementName, resourceGroupName, skipNumberOfGateways);
                    oGateways = JObject.Parse(gateways);
                    
                    foreach (var gateway in oGateways["value"])
                    {
                        string gatewayId = ((JValue)gateway["name"]).Value.ToString();
                        string[] dependsOn = new string[] { };
                        int skipNumberOfGatewayAPIs = 0;
                        JObject oGatewayAPIs = new JObject();

                        do
                        {
                            string gatewayAPIs = await GetGatewayAPIsAsync(apiManagementName, resourceGroupName, gatewayId, skipNumberOfGateways);
                            oGatewayAPIs = JObject.Parse(gatewayAPIs);
                            foreach (var gatewayAPI in oGatewayAPIs["value"])
                            {
                                string apiName = ((JValue)gatewayAPI["name"]).Value.ToString();
                                templateResources.AddRange(await GenerateGatewayAPIResourceAsync(apiName, exc, dependsOn));
                            }
                            skipNumberOfGatewayAPIs += GlobalConstants.NumOfRecords;
                        }
                        while (oGatewayAPIs["nextLink"] != null);
                    }

                    skipNumberOfGateways += GlobalConstants.NumOfRecords;
                }
                while (oGateways["nextLink"] != null);
            }

            armTemplate.resources = templateResources.ToArray();

            return armTemplate;
        }
    }
}
