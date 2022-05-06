// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Xunit;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
{
    public class ReleaseTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateReleaseTemplateResourceFromCreatorConfig()
        {
            // arrange
            var releaseTemplateCreator = new ReleaseTemplateCreator();
            CreatorParameters creatorConfig = new CreatorParameters() { Apis = new List<ApiConfig>() };
            ApiConfig api = new ApiConfig()
            {
                Name = "name",
                ApiRevision = "2",
                IsCurrent = true,
                Suffix = "suffix",
                SubscriptionRequired = true,
                OpenApiSpec = "https://petstore.swagger.io/v2/swagger.json",
            };
            creatorConfig.Apis.Add(api);

            // act
            string[] dependsOn = new string[] { "dependsOn" };
            var releaseTemplateResource = releaseTemplateCreator.CreateAPIReleaseTemplateResource(api, dependsOn);

            // assert
            string releaseName = $"";
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.Name}/release-revision-{api.ApiRevision}')]", releaseTemplateResource.Name);
            Assert.Equal(dependsOn, releaseTemplateResource.DependsOn);
            Assert.Equal($"Release created to make revision {api.ApiRevision} current.", releaseTemplateResource.Properties.notes);
            Assert.Equal($"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{api.Name}')]", releaseTemplateResource.Properties.apiId);
        }
    }
}
