// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels
{
    public class DiagnosticTemplateResource : APITemplateSubResource
    {
        public DiagnosticTemplateProperties Properties { get; set; }
    }

    public class DiagnosticTemplateProperties
    {
        public string AlwaysLog { get; set; }
        public string LoggerId { get; set; }
        public string HttpCorrelationProtocol { get; set; }
        public string Verbosity { get; set; }
        public bool? LogClientIp { get; set; }

        public DiagnosticTemplateSampling Sampling { get; set; }
        public DiagnosticTemplateFrontendBackend Frontend { get; set; }
        public DiagnosticTemplateFrontendBackend Backend { get; set; }
        public bool? EnableHttpCorrelationHeaders { get; set; }
    }

    public class DiagnosticTemplateSampling
    {
        public string SamplingType { get; set; }
        public double Percentage { get; set; }
    }

    public class DiagnosticTemplateFrontendBackend
    {
        public DiagnosticTemplateRequestResponse Request { get; set; }
        public DiagnosticTemplateRequestResponse Response { get; set; }
    }

    public class DiagnosticTemplateRequestResponse
    {
        public string[] Headers { get; set; }
        public DiagnosticTemplateRequestResponseBody Body { get; set; }
    }

    public class DiagnosticTemplateRequestResponseBody
    {
        public int Bytes { get; set; }
    }
}
