﻿using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
{
    public class MasterTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateCorrectNumberOfDeploymentResources()
        {
            // arrange
            CreatorConfig creatorConfig = new CreatorConfig() { apimServiceName = "apimService", linked = true };
            MasterTemplateCreator masterTemplateCreator = new MasterTemplateCreator();
            Template apiVersionSetsTemplate = new Template();
            Template globalServicePolicyTemplate = new Template();
            Template productsTemplate = new Template();
            Template productAPIsTemplate = new Template();
            Template propertyTemplate = new Template();
            Template tagTemplate = new Template();
            Template loggersTemplate = new Template();
            List<LinkedMasterTemplateAPIInformation> apiInfoList = new List<LinkedMasterTemplateAPIInformation>() { new LinkedMasterTemplateAPIInformation() { name = "api", isSplit = true } };
            FileNames creatorFileNames = FileNameGenerator.GenerateFileNames(creatorConfig.apimServiceName);

            // should create 8 resources (globalServicePolicy, apiVersionSet, product, property, tag, logger, both api templates)
            int count = 9;

            // act
            Template masterTemplate = masterTemplateCreator.CreateLinkedMasterTemplate(creatorConfig, globalServicePolicyTemplate, apiVersionSetsTemplate, productAPIsTemplate, productsTemplate, propertyTemplate, loggersTemplate, null, null, tagTemplate, apiInfoList, creatorFileNames, creatorConfig.apimServiceName);

            // assert
            Assert.Equal(count, masterTemplate.resources.Length);
        }

        [Fact]
        public void ShouldCreateCorrectNumberOfParameterValuesWhenLinked()
        {
            // arrange
            MasterTemplateCreator masterTemplateCreator = new MasterTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig()
            {
                apimServiceName = "apimServiceName",
                linked = true,
                linkedTemplatesBaseUrl = "linkedTemplatesBaseUrl"
            };
            // linked templates result in 2 values
            int count = 2;

            // act
            Template masterTemplate = masterTemplateCreator.CreateMasterTemplateParameterValues(creatorConfig);

            // assert
            Assert.Equal(count, masterTemplate.parameters.Count);
        }

        [Fact]
        public void ShouldCreateCorrectNumberOfParametersWhenUnlinked()
        {
            // arrange
            CreatorConfig creatorConfig = new CreatorConfig() { apimServiceName = "apimService", linked = false };
            MasterTemplateCreator masterTemplateCreator = new MasterTemplateCreator();
            // unlinked templates result in 1 value
            int count = 1;

            // act
            Dictionary<string, TemplateParameterProperties> masterTemplateParameters = masterTemplateCreator.CreateMasterTemplateParameters(creatorConfig);

            // assert
            Assert.Equal(count, masterTemplateParameters.Keys.Count);
        }

        [Fact]
        public void ShouldCreateLinkedMasterTemplateResourceFromValues()
        {
            // arrange
            MasterTemplateCreator masterTemplateCreator = new MasterTemplateCreator();
            string name = "name";
            string uriLink = "uriLink";
            string[] dependsOn = new string[] { "dependsOn" };

            // act
            var masterTemplateResource = masterTemplateCreator.CreateLinkedMasterTemplateResource(name, uriLink, dependsOn, null, false);

            // assert
            Assert.Equal(name, masterTemplateResource.name);
            Assert.Equal(uriLink, masterTemplateResource.properties.templateLink.uri);
            Assert.Equal(dependsOn, masterTemplateResource.dependsOn);
        }

        [Fact]
        public void ShouldCreateCorrectLinkedUri()
        {
            // arrange
            MasterTemplateCreator masterTemplateCreator = new MasterTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { apimServiceName = "apimService", linked = true, linkedTemplatesBaseUrl = "http://someurl.com", linkedTemplatesUrlQueryString = "?param=1" };
            string apiVersionSetFileName = "/versionSet1-apiVersionSets.template.json";

            // act
            string linkedResourceUri = masterTemplateCreator.GenerateLinkedTemplateUri(creatorConfig, apiVersionSetFileName);

            // assert
            Assert.Equal($"[concat(parameters('LinkedTemplatesBaseUrl'), '{apiVersionSetFileName}', parameters('LinkedTemplatesUrlQueryString'))]", linkedResourceUri);
        }
    }
}
