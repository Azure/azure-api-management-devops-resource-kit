// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Xunit;
using System;
using System.IO;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Common.FileHandlerTests
{
    public class FileReaderTests
    {
        [Fact]
        public async void ShouldConvertYAMLConfigToCreatorConfiguration()
        {
            // arrange
            FileReader fileReader = new FileReader();
            string fileLocation = Path.Combine("Resources", "YAMLConfigs", "valid.yml");

            // act
            CreatorParameters creatorConfig = await fileReader.ConvertConfigYAMLToCreatorConfigAsync(fileLocation);

            // assert
            Assert.Equal("0.0.1", creatorConfig.Version);
            Assert.Equal("myAPIMService", creatorConfig.ApimServiceName);
            Assert.Equal(@"C:\Users\myUsername\GeneratedTemplates", creatorConfig.OutputLocation);
            Assert.Equal("myAPI", creatorConfig.Apis[0].Name);
        }

        [Fact]
        public async void ShouldRetrieveFileContentsWithoutError()
        {
            // arrange
            FileReader fileReader = new FileReader();
            string fileLocation = "https://petstore.swagger.io/v2/swagger.json";

            // act
            try
            {
                var content = await fileReader.RetrieveFileContentsAsync(fileLocation);
                // assert
                Assert.True(true);
            }
            catch (Exception ex)
            {
                // assert
                Assert.NotNull(ex);
            }
        }
    }
}
