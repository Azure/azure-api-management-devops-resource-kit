
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{
    public class LoggerTemplateResource : TemplateResource
    {
        public LoggerTemplateProperties properties { get; set; }
    }

    public class LoggerTemplateProperties
    {
        public string loggerType { get; set; }
        public string description { get; set; }
        public LoggerCredentials credentials { get; set; }
        public bool isBuffered { get; set; }
        public string resourceId { get; set; }
    }

    public class LoggerCredentials
    {
        public string name { get; set; }
        public string connectionString { get; set; }
        public string instrumentationKey { get; set; }
    }
}
