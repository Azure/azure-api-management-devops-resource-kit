// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Xunit;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiVersionSet;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
{
    public class APIVersionSetTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateAPIVersionSetTemplateFromCreatorConfig()
        {
            // arrange
            APIVersionSetTemplateCreator apiVersionSetTemplateCreator = new APIVersionSetTemplateCreator(new TemplateBuilder());
            CreatorConfig creatorConfig = new CreatorConfig() { apiVersionSets = new List<APIVersionSetConfig>() };
            APIVersionSetConfig apiVersionSet = new APIVersionSetConfig()
            {
                id = "id",
                Description = "description",
                DisplayName = "displayName",
                VersionHeaderName = "versionHeaderName",
                VersioningScheme = "versioningScheme",
                VersionQueryName = "versionQueryName"
            };
            creatorConfig.apiVersionSets.Add(apiVersionSet);

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
            APIVersionSetTemplateCreator apiVersionSetTemplateCreator = new APIVersionSetTemplateCreator(new TemplateBuilder());
            CreatorConfig creatorConfig = new CreatorConfig() { apiVersionSets = new List<APIVersionSetConfig>() };
            APIVersionSetConfig apiVersionSet = new APIVersionSetConfig();
            creatorConfig.apiVersionSets.Add(apiVersionSet);

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
            APIVersionSetTemplateCreator apiVersionSetTemplateCreator = new APIVersionSetTemplateCreator(new TemplateBuilder());
            CreatorConfig creatorConfig = new CreatorConfig() { apiVersionSets = new List<APIVersionSetConfig>() };
            APIVersionSetConfig apiVersionSet = new APIVersionSetConfig()
            {
                id = "id"
            };
            creatorConfig.apiVersionSets.Add(apiVersionSet);

            // act
            Template versionSetTemplate = apiVersionSetTemplateCreator.CreateAPIVersionSetTemplate(creatorConfig);
            var apiVersionSetTemplateResource = (ApiVersionSetTemplateResource)versionSetTemplate.Resources[0];

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiVersionSet.id}')]", apiVersionSetTemplateResource.Name);
        }
    }
}
