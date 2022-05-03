// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Xunit;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiVersionSet;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
{
    public class APIVersionSetTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateAPIVersionSetTemplateFromCreatorConfig()
        {
            // arrange
            ApiVersionSetTemplateCreator apiVersionSetTemplateCreator = new ApiVersionSetTemplateCreator(new TemplateBuilder());
            CreatorParameters creatorConfig = new CreatorParameters() { ApiVersionSets = new List<ApiVersionSetConfig>() };
            ApiVersionSetConfig apiVersionSet = new ApiVersionSetConfig()
            {
                Id = "id",
                Description = "description",
                DisplayName = "displayName",
                VersionHeaderName = "versionHeaderName",
                VersioningScheme = "versioningScheme",
                VersionQueryName = "versionQueryName"
            };
            creatorConfig.ApiVersionSets.Add(apiVersionSet);

            // act
            Template versionSetTemplate = apiVersionSetTemplateCreator.CreateAPIVersionSetTemplate(creatorConfig);
            var apiVersionSetTemplateResource = (ApiVersionSetTemplateResource)versionSetTemplate.Resources[0];

            // assert
            Assert.Equal(apiVersionSet.Description, apiVersionSetTemplateResource.Properties.Description);
            Assert.Equal(apiVersionSet.DisplayName, apiVersionSetTemplateResource.Properties.DisplayName);
            Assert.Equal(apiVersionSet.VersionHeaderName, apiVersionSetTemplateResource.Properties.VersionHeaderName);
            Assert.Equal(apiVersionSet.VersioningScheme, apiVersionSetTemplateResource.Properties.VersioningScheme);
            Assert.Equal(apiVersionSet.VersionQueryName, apiVersionSetTemplateResource.Properties.VersionQueryName);
        }

        [Fact]
        public void ShouldUseDefaultResourceNameWithoutProvidedId()
        {
            // arrange
            ApiVersionSetTemplateCreator apiVersionSetTemplateCreator = new ApiVersionSetTemplateCreator(new TemplateBuilder());
            CreatorParameters creatorConfig = new CreatorParameters() { ApiVersionSets = new List<ApiVersionSetConfig>() };
            ApiVersionSetConfig apiVersionSet = new ApiVersionSetConfig();
            creatorConfig.ApiVersionSets.Add(apiVersionSet);

            // act
            Template versionSetTemplate = apiVersionSetTemplateCreator.CreateAPIVersionSetTemplate(creatorConfig);
            var apiVersionSetTemplateResource = (ApiVersionSetTemplateResource)versionSetTemplate.Resources[0];

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/versionset')]", apiVersionSetTemplateResource.Name);
        }

        [Fact]
        public void ShouldUseProvidedIdInResourceName()
        {
            // arrange
            ApiVersionSetTemplateCreator apiVersionSetTemplateCreator = new ApiVersionSetTemplateCreator(new TemplateBuilder());
            CreatorParameters creatorConfig = new CreatorParameters() { ApiVersionSets = new List<ApiVersionSetConfig>() };
            ApiVersionSetConfig apiVersionSet = new ApiVersionSetConfig()
            {
                Id = "id"
            };
            creatorConfig.ApiVersionSets.Add(apiVersionSet);

            // act
            Template versionSetTemplate = apiVersionSetTemplateCreator.CreateAPIVersionSetTemplate(creatorConfig);
            var apiVersionSetTemplateResource = (ApiVersionSetTemplateResource)versionSetTemplate.Resources[0];

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiVersionSet.Id}')]", apiVersionSetTemplateResource.Name);
        }
    }
}
