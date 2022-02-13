using apimtemplate.Common.Templates.Abstractions;

namespace apimtemplate.Common.TemplateModels
{
    public class TagAPITemplateResource : TemplateResource
    {
        public TagAPITemplateProperties properties { get; set; }
    }

    public class TagAPITemplateProperties
    {
        public string displayName { get; set; }
    }
}