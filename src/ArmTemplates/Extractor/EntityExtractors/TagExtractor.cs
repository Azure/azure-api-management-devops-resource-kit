using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class TagExtractor : EntityExtractorBase, ITagExtractor
    {
        readonly ILogger<TagExtractor> logger;
        readonly ITagClient tagClient;

        public TagExtractor(
            ILogger<TagExtractor> logger,
            ITagClient tagClient)
        {
            this.logger = logger;
            this.tagClient = tagClient;
        }

        public async Task<Template> GenerateTagsTemplateAsync(ExtractorParameters extractorParameters, string singleApiName, List<TemplateResource> apiTemplateResources, List<TemplateResource> productTemplateResources)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting tags from service");
            Template armTemplate = this.GenerateTemplateWithApimServiceNameProperty();

            // isolate tag and api operation associations in the case of a single api extraction
            var apiOperationTagResources = apiTemplateResources.Where(resource => resource.Type == ResourceTypeConstants.APIOperationTag);

            // isolate tag and api associations in the case of a single api extraction
            var apiTagResources = apiTemplateResources.Where(resource => resource.Type == ResourceTypeConstants.APITag);

            // isolate product api associations in the case of a single api extraction
            var productAPIResources = apiTemplateResources.Where(resource => resource.Type == ResourceTypeConstants.ProductApi);

            // isolate tag and product associations in the case of a single api extraction
            var productTagResources = productTemplateResources.Where(resource => resource.Type == ResourceTypeConstants.ProductTag);

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // pull all named values (Tags) for service
            JObject oTags = new JObject();
            int skipNumOfTags = 0;

            do
            {
                string tags = await this.tagClient.GetAllAsync(extractorParameters, skipNumOfTags);
                oTags = JObject.Parse(tags);

                foreach (var extractedTag in oTags["value"])
                {
                    string tagName = ((JValue)extractedTag["name"]).Value.ToString();

                    // convert returned named value to template resource class
                    TagTemplateResource tagTemplateResource = JsonConvert.DeserializeObject<TagTemplateResource>(extractedTag.ToString());
                    tagTemplateResource.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{tagName}')]";
                    tagTemplateResource.Type = ResourceTypeConstants.Tag;
                    tagTemplateResource.ApiVersion = GlobalConstants.ApiVersion;
                    tagTemplateResource.Scale = null;

                    // only extract the tag if this is a full extraction, 
                    // or in the case of a single api, if it is found in tags associated with the api operations
                    // or if it is found in tags associated with the api
                    // or if it is found in tags associated with the products associated with the api
                    if (singleApiName == null
                            || apiOperationTagResources.Any(t => t.Name.Contains($"/{tagName}'"))
                            || apiTagResources.Any(t => t.Name.Contains($"/{tagName}'"))
                            || productAPIResources.Any(t => t.Name.Contains($"/{singleApiName}"))
                                && productTagResources.Any(t => t.Name.Contains($"/{tagName}'")))
                    {
                        Console.WriteLine("'{0}' Tag found", tagName);
                        templateResources.Add(tagTemplateResource);
                    }
                }

                skipNumOfTags += GlobalConstants.NumOfRecords;
            }
            while (oTags["nextLink"] != null);


            armTemplate.Resources = templateResources.ToArray();
            return armTemplate;
        }
    }
}