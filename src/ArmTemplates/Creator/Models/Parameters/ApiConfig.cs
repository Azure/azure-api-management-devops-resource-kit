// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters
{
    public class ApiConfig
    {
        // used to build displayName and resource name from APITemplateResource schema
        public string Name { get; set; }
        // optional : overrides title from OpenAPI definition
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string ServiceUrl { get; set; }
        // used to build type and apiType from APITemplateResource schema
        public string Type { get; set; }
        // openApiSpec file location (local or url), used to build protocols, value, and format from APITemplateResource schema
        public string OpenApiSpec { get; set; }
        // format of the API definition.
        public OpenApiSpecFormat OpenApiSpecFormat { get; set; }
        // policy file location (local or url)
        public string Policy { get; set; }
        // used to buld path from APITemplateResource schema
        public string Suffix { get; set; }
        public bool SubscriptionRequired { get; set; }
        public bool IsCurrent { get; set; }
        public string ApiVersion { get; set; }
        public string ApiVersionDescription { get; set; }
        public string ApiVersionSetId { get; set; }
        public string ApiRevision { get; set; }
        public string ApiRevisionDescription { get; set; }
        public Dictionary<string, OperationsConfig> Operations { get; set; }
        public APITemplateAuthenticationSettings AuthenticationSettings { get; set; }
        public APITemplateSubscriptionKeyParameterNames SubscriptionKeyParameterNames { get; set; }
        public string Products { get; set; }
        public string Tags { get; set; }
        public string Protocols { get; set; }
        public DiagnosticConfig Diagnostic { get; set; }
        // does not currently include subscriptionKeyParameterNames, sourceApiId, and wsdlSelector from APITemplateResource schema
    }
}
