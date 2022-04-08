namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend
{
    /// <summary>
    /// Backend Parameters for a single API
    /// </summary>
    public class BackendApiParameters
    {
        public string ResourceId { get; set; }
        public string Url { get; set; }
        public string Protocol { get; set; }
    }

}
