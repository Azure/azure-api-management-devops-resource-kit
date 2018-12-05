using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class SchemaTemplateCreator
    {
        public List<SchemaTemplate> CreateSchemaTemplates(OpenApiDocument doc)
        {
            List<SchemaTemplate> schemaTemplates = new List<SchemaTemplate>();
            foreach (KeyValuePair<string, OpenApiSchema> definition in doc.Components.Schemas)
            {
                SchemaTemplate schema = new SchemaTemplate()
                {
                    name = definition.Key,
                    type = "Microsoft.ApiManagement/service/apis/schemas",
                    apiVersion = "2018-06-01-preview",
                    properties = new SchemaTemplateProperties()
                    {
                        contentType = "application/json",
                        document = new SchemaTemplateDocument()
                        {
                            value = JsonConvert.SerializeObject(definition.Value.Example)
                        }
                    }
                };
                schemaTemplates.Add(schema);
            }
            return schemaTemplates;
        }
    }

}
