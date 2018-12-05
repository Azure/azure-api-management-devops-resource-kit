using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class SchemaTemplate : APITemplateResource
    {
        public string name { get; set; }
        public string type { get; set; }
        public string apiVersion { get; set; }
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
}
