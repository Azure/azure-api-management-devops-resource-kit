using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues
{
    public class NamedValueTemplateResource : TemplateResource
    {
        public NamedValueProperties Properties { get; set; }
    }
}
