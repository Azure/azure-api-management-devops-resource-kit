using System;
using System.Collections.Generic;
using System.Text;

namespace apimtemplate.Common.Templates.Abstractions
{
    public class TemplateObjectParameterProperties : TemplateParameterProperties
    {
        public new object value { get; set; }
    }
}
