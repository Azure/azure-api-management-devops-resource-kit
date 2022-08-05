// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Xunit;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using FluentAssertions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Common.FileHandlerTests
{
    public class FileNameGeneratorTests
    {
        [Fact]
        public void ShouldGenerateApiFileName_GivenSingleApiName()
        {
            // arrange
            string apiName = "apiName";
            string baseFileName = "base-";

            // act
            var fileNameLocation = FileNameGenerator.GenerateExtractorAPIFileName(apiName, baseFileName);

            // assert
            fileNameLocation.Should().BeEquivalentTo($"{baseFileName}{apiName}-api.template.json");
        }

        [Fact]
        public void ShouldGenerateApiFileName_GivenSingleApiNameIsNull()
        {
            // arrange
            string baseFileName = "base-";

            // act
            var fileNameLocation = FileNameGenerator.GenerateExtractorAPIFileName(null, baseFileName);

            // assert
            fileNameLocation.Should().BeEquivalentTo($"{baseFileName}apis.template.json");
        }

        [Fact]
        public void ShouldGenerateApiBaseFileName_GivenSingleApiNameIsNull()
        {
            // act
            var apiFileNameBase = FileNameGenerator.GenerateApiFileNameBase(null);

            // assert
            apiFileNameBase.Should().BeEquivalentTo("apis");
        }

        [Fact]
        public void ShouldGenerateApiBaseFileName_GivenSingleApiName()
        {
            var apiName = "apiName";
            var apiFileNameBase = FileNameGenerator.GenerateApiFileNameBase(apiName);

            // assert
            apiFileNameBase.Should().BeEquivalentTo($"{apiName}-api");
        }
    }
}
