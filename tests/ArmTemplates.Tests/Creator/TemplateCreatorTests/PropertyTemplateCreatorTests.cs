// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Xunit;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
{
    public class PropertyTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreatePropertyFromCreatorConfig()
        {
            // arrange
            var propertyTemplateCreator = new PropertyTemplateCreator(new TemplateBuilder());
            CreatorConfig creatorConfig = new CreatorConfig() { namedValues = new List<PropertyConfig>() };
            PropertyConfig property = new PropertyConfig()
            {
                DisplayName = "displayName",
                Value = "value"
            };
            creatorConfig.namedValues.Add(property);

            // act
            var propertyTemplate = propertyTemplateCreator.CreatePropertyTemplate(creatorConfig);
            var propertyTemplateResource = (NamedValueTemplateResource)propertyTemplate.Resources[0];

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{property.DisplayName}')]", propertyTemplateResource.Name);
            Assert.Equal(property.DisplayName, propertyTemplateResource.Properties.DisplayName);
            Assert.Equal(property.Value, propertyTemplateResource.Properties.Value);
        }
    }
}
