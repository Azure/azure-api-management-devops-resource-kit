
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{
    public class OperationTemplateResource : APITemplateSubResource
    {
        public OperationTemplateProperties properties { get; set; }
        public PolicyTemplateResource[] resources { get; set; }
    }

    public class OperationTemplateProperties
    {
        public OperationTemplateParameter[] templateParameters { get; set; }
        public string description { get; set; }
        public OperationTemplateRequest request { get; set; }
        public OperationsTemplateResponse[] responses { get; set; }
        public string policies { get; set; }
        public string displayName { get; set; }
        public string method { get; set; }
        public string urlTemplate { get; set; }
    }

    public class OperationTemplateParameter
    {
        public string name { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public string defaultValue { get; set; }
        public bool required { get; set; }
        public string[] values { get; set; }
    }

    public class OperationTemplateRequest
    {
        public string description { get; set; }
        public OperationTemplateParameter[] queryParameters { get; set; }
        public OperationTemplateParameter[] headers { get; set; }
        public OperationTemplateRepresentation[] representations { get; set; }
    }

    public class OperationTemplateRepresentation
    {
        public string contentType { get; set; }
        public string sample { get; set; }
        public string schemaId { get; set; }
        public string typeName { get; set; }
        public OperationTemplateParameter[] formParameters { get; set; }
    }

    public class OperationsTemplateResponse
    {
        public int statusCode { get; set; }
        public string description { get; set; }
        public OperationTemplateParameter[] headers { get; set; }
        public OperationTemplateRepresentation[] representations { get; set; }
    }
}
