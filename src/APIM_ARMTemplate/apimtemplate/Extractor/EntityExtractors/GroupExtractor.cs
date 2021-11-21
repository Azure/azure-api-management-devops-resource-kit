using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class GroupExtractor : EntityExtractor
    {
        private FileWriter fileWriter;

        public GroupExtractor(FileWriter fileWriter)
        {
            this.fileWriter = fileWriter;
        }

        public async Task<string> GetGroupsAsync(string ApiManagementName, string ResourceGroupName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/groups?api-version={4}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }



        public async Task<Template> GenerateGroupsTemplateAsync(Extractor exc, string singleApiName)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting groups from service");
            Template armTemplate = GenerateEmptyPropertyTemplateWithParameters();

            List<GroupTemplateResource> templateResources = new List<GroupTemplateResource>();

            string groups = await GetGroupsAsync(exc.sourceApimName, exc.resourceGroup);
            JObject oGroups = JObject.Parse(groups);
            foreach (var extractedGroup in oGroups["value"])
            {
                GroupTemplateResource groupTemplate = new GroupTemplateResource();

                groupTemplate.type = extractedGroup["type"].ToString();
                groupTemplate.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{extractedGroup["name"].ToString()}')]";
                groupTemplate.apiVersion = GlobalConstants.APIVersion;
                groupTemplate.scale = null;

                var properties = extractedGroup["properties"];

                groupTemplate.properties = new GroupResourceProperties();

                groupTemplate.properties.displayName = ((JValue)properties["displayName"]).Value.ToString();
                groupTemplate.properties.description = properties["description"].ToString();
                groupTemplate.properties.type = properties["type"].ToString();
                if (((JValue)properties["externalId"]).Value == null)
                {
                    groupTemplate.properties.externalId = null;
                }
                else
                {
                    groupTemplate.properties.externalId = ((JValue)properties["externalId"]).Value.ToString();
                }

                templateResources.Add(groupTemplate);

            }

            armTemplate.resources = templateResources.ToArray();
            return armTemplate;
        }
    }
}
