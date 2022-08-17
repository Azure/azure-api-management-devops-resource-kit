// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiOperations;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors
{
    public class ApiOperationDataProcessor : IApiOperationDataProcessor
    {
        public IDictionary<string, string> OverrideRules { get; }            

        public void ProcessData(List<ApiOperationTemplateResource> apiOperationTemplateResources, ExtractorParameters extractorParameters)
        {
            if (apiOperationTemplateResources.IsNullOrEmpty())
            {
                return;
            }

            foreach (var apiOperationTemplate in apiOperationTemplateResources)
            {
                apiOperationTemplate.OriginalName = apiOperationTemplate.Name;
                this.SanitazeApiOperationRepresantations(apiOperationTemplate);
            }
        }

        void SanitizeExampleValues(ApiOperationRepresentation representation)
        {
            if (representation?.Examples.IsNullOrEmpty() == false)
            {
                foreach (var exampleValue in representation.Examples.Values)
                {
                    // Documentation on escaping characters https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/template-expressions#escape-characters
                    if (exampleValue.Value is string && exampleValue.Value.ToString().StartsWith("[") && exampleValue.Value.ToString().EndsWith("]"))
                    {
                        exampleValue.Value = $"[{exampleValue.Value}";
                    }
                }
            }
        }

        void SanitazeApiOperationRepresantations(ApiOperationTemplateResource apiOperation)
        {
            if (apiOperation.Properties?.Request?.Representations?.IsNullOrEmpty() == false)
            {
                foreach (var requestRepresentation in apiOperation.Properties.Request.Representations)
                {
                    this.SanitizeExampleValues(requestRepresentation);
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
                            this.SanitizeExampleValues(responseRepresentation);
                        }
                    }
                }
            }
        }
    }
}
