
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class LoggerResource : Resource
    {
        public string type { get; set; }
        public string name { get; set; }
        public string apiVersion { get; set; }
        public object scale { get; set; }
        public LoggerResourceProperties properties { get; set; }
        public string[] dependsOn { get; set; }
    }

    public class LoggerResourceProperties
    {
        public string loggerType { get; set; }
        public string description { get; set; }
        public object credentials { get; set; }
        public bool isBuffered { get; set; }
        public string resourceId { get; set; }
    }
}
