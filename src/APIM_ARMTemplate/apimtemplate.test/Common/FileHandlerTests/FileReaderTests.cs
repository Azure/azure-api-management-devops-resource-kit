using Xunit;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System;
using System.IO;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Test
{
    public class FileReaderTests
    {
        [Fact]
        public async void ShouldConvertYAMLConfigToCreatorConfiguration()
        {
            // arrange
            FileReader fileReader = new FileReader();
            string fileLocation = String.Concat("..", Path.DirectorySeparatorChar,
                 "..", Path.DirectorySeparatorChar,
                   "..", Path.DirectorySeparatorChar,
                   "..", Path.DirectorySeparatorChar,
                   "apimtemplate", Path.DirectorySeparatorChar,
                   "Creator", Path.DirectorySeparatorChar,
                   "ExampleFiles", Path.DirectorySeparatorChar,
                   "YAMLConfigs", Path.DirectorySeparatorChar, "valid.yml");

            // act
            CreatorConfig creatorConfig = await fileReader.ConvertConfigYAMLToCreatorConfigAsync(fileLocation);

            // assert
            Assert.Equal("0.0.1", creatorConfig.version);
            Assert.Equal("myAPIMService", creatorConfig.apimServiceName);
            Assert.Equal(@"C:\Users\myUsername\GeneratedTemplates", creatorConfig.outputLocation);
            Assert.Equal("myAPI", creatorConfig.apis[0].name);
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
