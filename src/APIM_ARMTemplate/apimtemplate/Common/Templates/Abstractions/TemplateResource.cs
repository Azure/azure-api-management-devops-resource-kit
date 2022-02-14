namespace apimtemplate.Common.Templates.Abstractions
{
    public class TemplateResource
    {
        public string name { get; set; }
        public string type { get; set; }
        public string apiVersion { get; set; }
        public string scale { get; set; }
        public string[] dependsOn { get; set; }
    }
}
