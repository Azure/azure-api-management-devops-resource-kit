using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class GatewayExtractor : EntityExtractor
    {
        public async Task<string> GetGatewaysAsync(string apiManagementName, string resourceGroupName, int skipNumOfRecords)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/gateways?$skip={4}&api-version={5}",
               baseUrl, azSubId, resourceGroupName, apiManagementName, skipNumOfRecords, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetGatewayAPIsAsync(string apiManagementName, string resourceGroupName, string gatewayId, int skipNumOfRecords)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/gateways/{4}/apis?$skip={5}&api-version={6}",
               baseUrl, azSubId, resourceGroupName, apiManagementName, gatewayId, skipNumOfRecords, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        /// <summary>
        /// Generate the ARM assets for the backend resources
        /// </summary>
        //
        /// <returns>a combination of a Template and the value for the BackendSettings parameter</returns>
        public async Task<Template> GenerateGatewayARMTemplateAsync(string apiManagementName, string resourceGroupName, string singleApiName, Extractor exc)
        {
            Console.WriteLine("------------------------------------------");
            
            Template armTemplate = GenerateEmptyPropertyTemplateWithParameters();

            if (!exc.extractGateways)
            {
                Console.WriteLine("Skipping gateway extraction from service");
                return armTemplate;
            }

            Console.WriteLine("Extracting gateways from service");

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // pull all gateways for service
            JObject oGateways = new JObject();
            int skipNumberOfGateways = 0;

            do
            {
                string gateways = await GetGatewaysAsync(apiManagementName, resourceGroupName, skipNumberOfGateways);
                oGateways = JObject.Parse(gateways);

                foreach (var item in oGateways["value"])
                {
                    string gatewayName = ((JValue)item["name"]).Value.ToString();

                    // convert returned backend to template resource class
                    GatewayTemplateResource gatewayTemplateResource = JsonConvert.DeserializeObject<GatewayTemplateResource>(item.ToString());
                    gatewayTemplateResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{gatewayName}')]";
                    gatewayTemplateResource.apiVersion = GlobalConstants.APIVersion;
                    gatewayTemplateResource.type = ResourceTypeConstants.Gateway;

                    bool includeGateway = false;

                    ////only extract the gateway if this is a full extraction, or if it's referenced by single api.
                    if (singleApiName == null)
                    {
                        // if the user is extracting all apis, extract all the gateways
                        includeGateway = true;
                    }
                    else
                    {
                        includeGateway = await DoesApiReferenceGatewayAsync(apiManagementName, resourceGroupName, gatewayName, singleApiName);
                    }

                    if (includeGateway)
                    {   
                        Console.WriteLine("'{0}' Gateway found", gatewayName);
                        templateResources.Add(gatewayTemplateResource);
                    }
                }

                skipNumberOfGateways += GlobalConstants.NumOfRecords;
            }
            while (oGateways["nextLink"] != null);

            armTemplate.resources = templateResources.ToArray();
            return armTemplate;
        }

        /// <summary>
        /// Checks whether a given single API references a gateway.
        /// </summary>
        /// <param name="apiManagementName">Name of the API Management Service.</param>
        /// <param name="resourceGroupName">Name of the ResourceGroup hosting the API Management Service.</param>
        /// <param name="gatewayId">The of the self hosted gateway.</param>
        /// <param name="apiId">The api id.</param>
        /// <returns>Whether the API is hosted in a Gateway or not.</returns>
        public async Task<bool> DoesApiReferenceGatewayAsync(string apiManagementName, string resourceGroupName, string gatewayId, string apiId)
        {
            int skipNumberOfGateways = 0;
            JObject oGatewayAPIs = new JObject();

            do
            {
                string gatewayAPIs = await GetGatewayAPIsAsync(apiManagementName, resourceGroupName, gatewayId, skipNumberOfGateways);
                oGatewayAPIs = JObject.Parse(gatewayAPIs);
                foreach (var gatewayAPI in oGatewayAPIs["value"])
                {
                    string gatewayAPIName = ((JValue)gatewayAPI["name"]).Value.ToString();

                    if (gatewayAPIName.Equals(apiId))
                    {
                        return true;
                    }
                }

                skipNumberOfGateways += GlobalConstants.NumOfRecords;
            }
            while (oGatewayAPIs["nextLink"] != null);

            return false;
        }
    }
}
