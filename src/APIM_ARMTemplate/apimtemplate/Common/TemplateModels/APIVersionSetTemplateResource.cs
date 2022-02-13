using apimtemplate.Common.Templates.Abstractions;

namespace apimtemplate.Common.TemplateModels
{
    public class APIVersionSetTemplateResource : TemplateResource
    {
        public APIVersionSetProperties properties { get; set; }
    }

    public class APIVersionSetProperties
    {
        public string displayName { get; set; }
        public string description { get; set; }
        public string versionQueryName { get; set; }
        public string versionHeaderName { get; set; }
        public string versioningScheme { get; set; }
    }
}
