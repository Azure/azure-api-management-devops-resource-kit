// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiOperations;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Utilities
{
    public class ApiOperationDataProcessorTest : ExtractorMockerTestsBase
    {
        public List<ApiOperationTemplateResource> GetMockedApiOperationTemplates() {
            return new List<ApiOperationTemplateResource>
                {
                    new ApiOperationTemplateResource
                    {
                        Name = "api-operation-1",
                        Type = ResourceTypeConstants.APIOperation,
                        Properties = new ApiOperationProperties
                        {
                            DisplayName = "operation-1",
                            Description = "operation 1 description",
                            Request = new ApiOperationRequest
                            {
                                Representations = new ApiOperationRepresentation[]
                                {
                                    new ApiOperationRepresentation
                                    {
                                        Examples = new Dictionary<string, ParameterExampleContract>
                                        {
                                            { "default", new ParameterExampleContract {
                                                Value = "[{\"example-key\": \"example-value\"}]"
                                            } },
                                            { "non-default", new ParameterExampleContract {
                                                Value = "[{\"example-key\": \"example-value\"}]"
                                            } }
                                        }
                                    }
                                }
                            },
                            Responses = new ApiOperationResponse[]
                            {
                                new ApiOperationResponse{
                                    Representations = new ApiOperationRepresentation[]
                                    {
                                        new ApiOperationRepresentation
                                        {
                                            Examples = new Dictionary<string, ParameterExampleContract>
                                            {
                                                { "default", new ParameterExampleContract {
                                                    Value = "[{\"example-key\": \"example-value\"}]"
                                                } },
                                                { "non-default", new ParameterExampleContract {
                                                    Value = "[{\"example-key\": \"example-value\"}] test"
                                                } }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
            };
        }

        [Fact]
        public void TestApiOperationDataProcess()
        {
            var apiOperationTemplates = this.GetMockedApiOperationTemplates();
            var apiOperationDataProcessor = new ApiOperationDataProcessor();

            apiOperationDataProcessor.ProcessData(apiOperationTemplates, default);

            apiOperationTemplates.Count.Should().Be(1);
            apiOperationTemplates[0].Properties.Responses.Count().Should().Be(1);

            foreach (var response in apiOperationTemplates[0].Properties.Responses)
            {
                this.ValidateSanitizedExampleValue(response.Representations);
            }
            this.ValidateSanitizedExampleValue(apiOperationTemplates[0].Properties.Request.Representations);
        }

        void ValidateSanitizedExampleValue(ApiOperationRepresentation[] representations)
        {
            foreach (var representation in representations)
            {
                foreach (var exampleValue in representation.Examples.Values)
                {                   
                    if (exampleValue.Value.GetType() == typeof(string))
                    {
                        var defaultRepresentationValuess = (string)exampleValue.Value;
                        if (defaultRepresentationValuess.EndsWith("]"))
                        {
                            defaultRepresentationValuess.StartsWith("[[").Should().BeTrue();
                        }
                    }
                }
            }
        }
    }
}
