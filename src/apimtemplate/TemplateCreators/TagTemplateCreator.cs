using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class TagTemplateCreator
    {
        public List<TagTemplate> CreateTagTemplates(OpenApiDocument doc)
        {
            List<TagTemplate> tagTemplates = new List<TagTemplate>();
            foreach (OpenApiTag tag in doc.Tags)
            {
                TagTemplate tagSchema = new TagTemplate()
                {
                    name = tag.Name,
                    type = "Microsoft.ApiManagement/service/apis/operations/tags",
                    apiVersion = "2018-06-01-preview"
                };
                tagTemplates.Add(tagSchema);
            }
            return tagTemplates;
        }

        public List<TagDescriptionTemplate> CreateTagDescriptionTemplates(OpenApiDocument doc)
        {
            List<TagDescriptionTemplate> tagDescriptionTemplates = new List<TagDescriptionTemplate>();
            foreach (OpenApiTag tag in doc.Tags)
            {
                TagDescriptionTemplate tagDescriptionSchema = new TagDescriptionTemplate()
                {
                    name = tag.Name,
                    type = "Microsoft.ApiManagement/service/apis/operations/tagDescriptions",
                    apiVersion = "2018-06-01-preview",
                    properties = new TagDescriptionTemplateProperties()
                    {
                        description = tag.Description,
                        externalDocsDescription = tag.ExternalDocs != null && tag.ExternalDocs.Description != null ? tag.ExternalDocs.Description : "",
                        externalDocsUrl = tag.ExternalDocs != null && tag.ExternalDocs.Url.AbsoluteUri != null ? tag.ExternalDocs.Url.AbsoluteUri : ""
                    }
                };
                tagDescriptionTemplates.Add(tagDescriptionSchema);
            }
            return tagDescriptionTemplates;
        }
    }
}
