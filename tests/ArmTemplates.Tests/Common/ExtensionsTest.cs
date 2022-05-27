// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Xunit;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using FluentAssertions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Common
{
    public class ExtensionsTest
    {
        [Fact]
        public void GenerateValidResourceNameFromDisplayName_ShouldReturnNull_GivenEmptyString() 
        {
            var resourceName = ResourceNamingHelper.GenerateValidResourceNameFromDisplayName(string.Empty);

            resourceName.Should().BeNull();
        }

        [Fact]
        public void GenerateValidResourceNameFromDisplayName_ShouldReturnSameValue_GivenValidResourceName()
        {
            var displayName = "valid-resource-name";
            var resourceName = ResourceNamingHelper.GenerateValidResourceNameFromDisplayName(displayName);

            resourceName.Equals(displayName).Should().BeTrue();
        }

        [Fact]
        public void GenerateValidResourceNameFromDisplayName_ShouldReturValidaResourceName_GivenNonResoucreNameDisplayName()
        {
            var displayName = "   valid %%resource $$name-   ";
            var expectedResourceName = "valid-resource-name-";
            
            var resourceName = ResourceNamingHelper.GenerateValidResourceNameFromDisplayName(displayName);

            resourceName.Equals(expectedResourceName).Should().BeTrue();
        }
    }
}
