
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{
    public class SchemaTemplateResource : APITemplateSubResource
    {
        public SchemaTemplateProperties properties { get; set; }
    }

    public class SchemaTemplateProperties
    {
        public string contentType { get; set; }
        public SchemaTemplateDocument document { get; set; }
    }

    public class SchemaTemplateDocument
    {
        public string value { get; set; }
    }

    public class RESTReturnedSchemaTemplate : APITemplateSubResource
    {
        public RESTReturnedSchemaTemplateProperties properties { get; set; }
    }

    public class RESTReturnedSchemaTemplateProperties
    {
        public string contentType { get; set; }
        public object document { get; set; }
    }
}
