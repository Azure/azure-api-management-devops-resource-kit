namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions
{
    public class TemplateResource
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string ApiVersion { get; set; }

        public string Scale { get; set; }

        public string[] DependsOn { get; set; }
    }
}
