using apimtemplate.Common.Templates.Abstractions;

namespace apimtemplate.Common.Templates.Policy
{
    public class PolicyTemplateResource : APITemplateSubResource
    {
        public PolicyTemplateProperties properties { get; set; }
    }

    public class PolicyTemplateProperties
    {
        public string value { get; set; }
        public string format { get; set; }
    }
}
