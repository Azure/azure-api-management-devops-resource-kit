// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels
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

        public dynamic components { get; set; }
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
