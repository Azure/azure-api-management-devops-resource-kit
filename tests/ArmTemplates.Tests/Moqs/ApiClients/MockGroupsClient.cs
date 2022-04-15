// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Groups;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Moq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    class MockGroupsClient
    {
        public const string GroupName1 = "group-1";
        public const string GroupName2 = "group-2";

        public static IGroupsClient GetMockedApiClientWithDefaultValues()
        {
            var mockGroupsClient = new Mock<IGroupsClient>(MockBehavior.Strict);

            mockGroupsClient
                .Setup(x => x.GetAllLinkedToProductAsync(It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new List<GroupTemplateResource>
                {
                    new GroupTemplateResource
                    {
                        Name = GroupName1,
                        Type = ResourceTypeConstants.ProductGroup,
                        Properties = new GroupProperties
                        {
                            DisplayName = $"{GroupName1}-display-name",
                            Description = $"{GroupName1}-description",
                            BuiltIn = true,
                            Type = "system"
                        }
                    },

                    new GroupTemplateResource
                    {
                        Name = GroupName2,
                        Type = ResourceTypeConstants.ProductGroup,
                        Properties = new GroupProperties
                        {
                            DisplayName = $"{GroupName2}-display-name",
                            Description = $"{GroupName2}-description",
                            BuiltIn = true,
                            Type = "system"
                        }
                    }
                });

            mockGroupsClient
                .Setup(x => x.GetAllAsync(It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new List<GroupTemplateResource>
                {
                    new GroupTemplateResource
                    {
                        Name = GroupName1,
                        Type = ResourceTypeConstants.ProductGroup,
                        Properties = new GroupProperties
                        {
                            DisplayName = $"{GroupName1}-display-name",
                            Description = $"{GroupName1}-description",
                            BuiltIn = true,
                            Type = "system"
                        }
                    },

                    new GroupTemplateResource
                    {
                        Name = GroupName2,
                        Type = ResourceTypeConstants.ProductGroup,
                        Properties = new GroupProperties
                        {
                            DisplayName = $"{GroupName2}-display-name",
                            Description = $"{GroupName2}-description",
                            BuiltIn = true,
                            Type = "system"
                        }
                    }
                });

            return mockGroupsClient.Object;
        }
    }
}
