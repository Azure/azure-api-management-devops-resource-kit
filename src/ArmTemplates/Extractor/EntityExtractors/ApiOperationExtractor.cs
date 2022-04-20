// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiOperations;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class ApiOperationExtractor : IApiOperationExtractor
    {
        readonly ILogger<ApiOperationExtractor> logger;
        readonly IApiOperationClient apiOperationClient;

        public ApiOperationExtractor(
            ILogger<ApiOperationExtractor> logger,
            IApiOperationClient apiOperationClient)
        {
            this.logger = logger;

            this.apiOperationClient = apiOperationClient;
        }

        public async Task<List<ApiOperationTemplateResource>> GenerateApiOperationsResourcesAsync(string apiName, ExtractorParameters extractorParameters)
        {
            var apiOperations = await this.apiOperationClient.GetOperationsLinkedToApiAsync(apiName, extractorParameters);

            if (apiOperations.IsNullOrEmpty())
            {
                this.logger.LogWarning("No operations found for api {0}", apiName);
                return apiOperations;
            }

            foreach (var apiOperation in apiOperations)
            {
                apiOperation.OriginalName = apiOperation.Name;

                apiOperation.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{apiOperation.OriginalName}')]";
                apiOperation.ApiVersion = GlobalConstants.ApiVersion;
                apiOperation.Scale = null;

                var operationDependsOn = new List<string>() { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiName}')]" };
                if (apiOperation.Properties?.Request?.Representations?.IsNullOrEmpty() == false)
                {
                    foreach (var requestRepresentation in apiOperation.Properties.Request.Representations)
                    {
                        AddSchemaDependencyToOperationIfNecessary(apiName, operationDependsOn, requestRepresentation);
                    }
                }

                if (apiOperation.Properties?.Responses?.IsNullOrEmpty() == false)
                {
                    foreach (var operationResponse in apiOperation.Properties?.Responses)
                    {
                        if (operationResponse?.Representations?.IsNullOrEmpty() == false)
                        {
                            foreach (var responseRepresentation in operationResponse.Representations)
                            {
                                AddSchemaDependencyToOperationIfNecessary(apiName, operationDependsOn, responseRepresentation);
                            }
                        }
                    }
                }

                apiOperation.DependsOn = operationDependsOn.ToArray();
            }

            return apiOperations;
        }

        static void AddSchemaDependencyToOperationIfNecessary(string apiName, List<string> operationDependsOn, ApiOperationRepresentation operationRepresentation)
        {
            if (operationRepresentation.SchemaId is not null)
            {
                string dependsOn = $"[resourceId('Microsoft.ApiManagement/service/apis/schemas', parameters('{ParameterNames.ApimServiceName}'), '{apiName}', '{operationRepresentation.SchemaId}')]";
                // add value to list if schema has not already been added
                if (!operationDependsOn.Exists(o => o == dependsOn))
                {
                    operationDependsOn.Add(dependsOn);
                }
            }
        }
    }
}
