using System;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{
    public class DiagnosticTemplateResource : APITemplateSubResource
    {
        public DiagnosticTemplateProperties properties { get; set; }
    }

    public class DiagnosticTemplateProperties
    {
        public string alwaysLog { get; set; }
        public string loggerId { get; set; }
        public DiagnosticTemplateSampling sampling { get; set; }
        public DiagnosticTemplateFrontendBackend frontend { get; set; }
        public DiagnosticTemplateFrontendBackend backend { get; set; }
        public Nullable<bool> enableHttpCorrelationHeaders { get; set; }
    }

    public class DiagnosticTemplateSampling
    {
        public string samplingType { get; set; }
        public double percentage { get; set; }
    }

    public class DiagnosticTemplateFrontendBackend
    {
        public DiagnosticTemplateRequestResponse request { get; set; }
        public DiagnosticTemplateRequestResponse response { get; set; }
    }

    public class DiagnosticTemplateRequestResponse
    {
        public string[] headers { get; set; }
        public DiagnosticTemplateRequestResponseBody body { get; set; }
    }

    public class DiagnosticTemplateRequestResponseBody
    {
        public int bytes { get; set; }
    }




}
