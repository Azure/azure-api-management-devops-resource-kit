using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.OpenApi.Any;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class OperationTemplateCreator
    {
        public List<OperationTemplate> CreateOperationTemplates(OpenApiDocument doc)
        {
            List<OperationTemplate> operationTemplates = new List<OperationTemplate>();
            foreach (KeyValuePair<string, OpenApiPathItem> path in doc.Paths)
            {
                foreach (KeyValuePair<OperationType, OpenApiOperation> operation in path.Value.Operations)
                {
                    OperationTemplate op = new OperationTemplate()
                    {
                        name = operation.Value.OperationId,
                        type = "Microsoft.ApiManagement/service/apis/operations",
                        apiVersion = "2018-06-01-preview",
                        properties = new OperationTemplateProperties()
                        {
                            method = operation.Key.ToString(),
                            urlTemplate = path.Key,
                            description = operation.Value.Description,
                            displayName = operation.Value.Summary,
                            templateParameters = CreateTemplateParameters(operation.Value.Parameters).ToArray(),
                            responses = CreateOperationResponses(operation.Value.Responses).ToArray(),
                            request = CreateOperationRequest(operation.Value),

                            //unfinished
                            policies = null
                        }
                    };
                    operationTemplates.Add(op);
                }
            }
            
            return operationTemplates;
        }

        public OperationTemplateRequest CreateOperationRequest(OpenApiOperation operation)
        {
            OperationTemplateRequest request = new OperationTemplateRequest()
            {
                description = operation.RequestBody != null ? operation.RequestBody.Description : null,
                queryParameters = CreateTemplateParameters(operation.Parameters.Where(p => p.In == ParameterLocation.Query).ToList()).ToArray(),
                headers = CreateTemplateParameters(operation.Parameters.Where(p => p.In == ParameterLocation.Header).ToList()).ToArray(),
                representations = operation.RequestBody != null ? CreateRepresentations(operation.RequestBody.Content).ToArray() : null
            };
            return request;
        }

        public List<OperationsTemplateResponse> CreateOperationResponses(OpenApiResponses operationResponses)
        {
            List<OperationsTemplateResponse> responses = new List<OperationsTemplateResponse>();
            foreach (KeyValuePair<string, OpenApiResponse> response in operationResponses)
            {
                OperationsTemplateResponse res = new OperationsTemplateResponse()
                {
                    statusCode = response.Key,
                    description = response.Value.Description,
                    headers = CreateResponseHeaders(response.Value.Headers).ToArray(),
                    representations = CreateRepresentations(response.Value.Content).ToArray()
                };
            }
            return responses;
        }

        public List<OperationTemplateRepresentation> CreateRepresentations(IDictionary<string, OpenApiMediaType> content)
        {
            List<OperationTemplateRepresentation> representations = new List<OperationTemplateRepresentation>();
            foreach (KeyValuePair<string, OpenApiMediaType> pair in content)
            {
                OpenApiParameterHeaderIntersection param = new OpenApiParameterHeaderIntersection()
                {
                    Example = pair.Value.Example,
                    Examples = pair.Value.Examples
                };
                OperationTemplateRepresentation representation = new OperationTemplateRepresentation()
                {
                    contentType = pair.Key,
                    sample = JsonConvert.SerializeObject(CreateParameterDefaultValue(param)),
                    // schema has not yet been created, id is null
                    schemaId = null,
                    typeName = pair.Value.Schema != null ? pair.Value.Schema.Type : null,
                    // content type is neither application/x-www-form-urlencoded or multipart/form-data, form parameters are null
                    formParameters = null
                };
                representations.Add(representation);
            }
            return representations;

        }

        public List<OperationTemplateParameter> CreateResponseHeaders(IDictionary<string, OpenApiHeader> headerPairs)
        {
            List<OperationTemplateParameter> headers = new List<OperationTemplateParameter>();
            foreach (KeyValuePair<string, OpenApiHeader> pair in headerPairs)
            {
                OpenApiParameterHeaderIntersection param = new OpenApiParameterHeaderIntersection()
                {
                    Example = pair.Value.Example,
                    Examples = pair.Value.Examples
                };
                OperationTemplateParameter headerTemplate = new OperationTemplateParameter()
                {
                    name = pair.Key,
                    description = pair.Value.Description,
                    type = pair.Value.Schema.Type,
                    required = pair.Value.Required,
                    values = CreateParameterValues(param).ToArray(),
                    defaultValue = CreateParameterDefaultValue(param)
                };
                headers.Add(headerTemplate);
            };
            return headers;
        }

        public List<OperationTemplateParameter> CreateTemplateParameters(IList<OpenApiParameter> parameters)
        {
            List<OperationTemplateParameter> templateParameters = new List<OperationTemplateParameter>();
            foreach (OpenApiParameter parameter in parameters)
            {
                OpenApiParameterHeaderIntersection param = new OpenApiParameterHeaderIntersection()
                {
                    Example = parameter.Example,
                    Examples = parameter.Examples
                };
                OperationTemplateParameter templateParameter = new OperationTemplateParameter()
                {
                    name = parameter.Name,
                    description = parameter.Description,
                    type = parameter.Schema.Type,
                    required = parameter.Required,
                    values = CreateParameterValues(param).ToArray(),
                    defaultValue = CreateParameterDefaultValue(param)
                };
                templateParameters.Add(templateParameter);
            }
            return templateParameters;
        }

        public List<string> CreateParameterValues(OpenApiParameterHeaderIntersection parameter)
        {
            List<string> values = new List<string>();
            if (parameter.Example != null)
            {
                values.Add(JsonConvert.SerializeObject(parameter.Example));

            }
            foreach (KeyValuePair<string, OpenApiExample> example in parameter.Examples)
            {
                values.Add(JsonConvert.SerializeObject(example.Value));
            }
            return values;
        }

        public string CreateParameterDefaultValue(OpenApiParameterHeaderIntersection parameter)
        {
            if (parameter.Example != null)
            {
                return JsonConvert.SerializeObject(parameter.Example);

            }
            else if (parameter.Examples != null)
            {
                return JsonConvert.SerializeObject(parameter.Examples.SingleOrDefault().Value);
            }
            else
            {
                return null;
            }
        }
    }

    public class OpenApiParameterHeaderIntersection {
        public IOpenApiAny Example { get; set; }
        public IDictionary<string, OpenApiExample> Examples { get; set; }
    }
}
