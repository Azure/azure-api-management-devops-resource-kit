using System;
using System.Collections.Generic;
using System.Text;

namespace apimtemplate.Common.TemplateModels
{
    /// <summary>
    /// Backend Parameters for a single API
    /// </summary>
    public class BackendApiParameters
    {
        public string resourceId { get; set; }
        public string url { get; set; }
        public string protocol { get; set; }
    }

}
