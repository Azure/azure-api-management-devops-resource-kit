﻿using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues
{
    public class NamedValueTemplateResource : TemplateResource
    {
        public string OriginalName { get; set; }

        public NamedValueProperties Properties { get; set; }
    }
}
