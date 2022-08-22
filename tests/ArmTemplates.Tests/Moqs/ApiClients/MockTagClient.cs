// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Moq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    class MockTagClient
    {
        public const string TagName1 = "tag-1";
        public const string TagName2 = "tag-2";
        public const string OperationTagName1 = "operation-tag-1";
        public const string OperationTagName2 = "operation-tag-2";

        public static ITagClient GetMockedApiClientWithDefaultValues()
        {
            var mockTagsClient = new Mock<ITagClient>(MockBehavior.Strict);

            mockTagsClient
                .Setup(x => x.GetTagsLinkedToApiOperationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new List<TagTemplateResource>
                {
                    new TagTemplateResource
                    {
                        Name = TagName1,
                        Type = ResourceTypeConstants.APIOperationTag
                    },

                    new TagTemplateResource
                    {
                        Name = TagName2,
                        Type = ResourceTypeConstants.APIOperationTag
                    }
                });

            mockTagsClient
                .Setup(x => x.GetAllTagsLinkedToApiAsync(It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new List<TagTemplateResource>
                {
                    new TagTemplateResource
                    {
                        Name = TagName1,
                        Type = ResourceTypeConstants.APITag
                    },

                    new TagTemplateResource
                    {
                        Name = TagName2,
                        Type = ResourceTypeConstants.APITag
                    }
                });

            mockTagsClient
                .Setup(x => x.GetAllTagsLinkedToProductAsync(It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new List<TagTemplateResource>
                {
                    new TagTemplateResource
                    {
                        Name = TagName1,
                        Type = ResourceTypeConstants.ProductTag
                    },

                    new TagTemplateResource
                    {
                        Name = TagName2,
                        Type = ResourceTypeConstants.ProductTag
                    }
                });

            mockTagsClient
                .Setup(x => x.GetAllAsync(It.IsAny<ExtractorParameters>(), It.IsAny<int>()))
                .ReturnsAsync(new List<TagTemplateResource>
                {
                    new TagTemplateResource
                    {
                        Name = TagName1,
                        Type = ResourceTypeConstants.APITag
                    },

                    new TagTemplateResource
                    {
                        Name = TagName2,
                        Type = ResourceTypeConstants.APITag
                    },
                    new TagTemplateResource
                    {
                        Name = OperationTagName1,
                        Type = ResourceTypeConstants.APITag
                    },
                    new TagTemplateResource
                    {
                        Name = OperationTagName2,
                        Type = ResourceTypeConstants.APITag
                    }
                });

            return mockTagsClient.Object;
        }
    }
}
