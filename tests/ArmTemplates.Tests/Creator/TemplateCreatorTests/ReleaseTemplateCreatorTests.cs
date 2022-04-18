// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Xunit;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
{
    public class ReleaseTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateReleaseTemplateResourceFromCreatorConfig()
        {
            // arrange
            var releaseTemplateCreator = new ReleaseTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { apis = new List<APIConfig>() };
            APIConfig api = new APIConfig()
            {
                name = "name",
                apiRevision = "2",
                isCurrent = true,
                suffix = "suffix",
                subscriptionRequired = true,
                openApiSpec = "https://petstore.swagger.io/v2/swagger.json",
            };
            creatorConfig.apis.Add(api);

            // act
            string[] dependsOn = new string[] { "dependsOn" };
            var releaseTemplateResource = releaseTemplateCreator.CreateAPIReleaseTemplateResource(api, dependsOn);

            // assert
            string releaseName = $"";
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.name}/release-revision-{api.apiRevision}')]", releaseTemplateResource.Name);
            Assert.Equal(dependsOn, releaseTemplateResource.DependsOn);
            Assert.Equal($"Release created to make revision {api.apiRevision} current.", releaseTemplateResource.Properties.notes);
            Assert.Equal($"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{api.name}')]", releaseTemplateResource.Properties.apiId);
        }
    }
}
