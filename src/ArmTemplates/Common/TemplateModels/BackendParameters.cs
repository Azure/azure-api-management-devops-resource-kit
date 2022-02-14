﻿namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels
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
