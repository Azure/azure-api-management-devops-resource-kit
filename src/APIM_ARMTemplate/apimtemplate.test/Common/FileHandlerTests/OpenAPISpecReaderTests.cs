using Xunit;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System;
using System.IO;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Test
{
    public class OpenAPISpecReaderTests
    {
        private string openAPISpecFolder;

        public OpenAPISpecReaderTests()
        {
            this.openAPISpecFolder = String.Concat("..", Path.DirectorySeparatorChar,
                 "..", Path.DirectorySeparatorChar,
                   "..", Path.DirectorySeparatorChar,
                   "..", Path.DirectorySeparatorChar,
                   "apimtemplate.test", Path.DirectorySeparatorChar,
                   "Common", Path.DirectorySeparatorChar,
                   "OpenAPISpecs", Path.DirectorySeparatorChar);
        }
        [Fact]
        public async void ShouldDetermineCorrectOpenAPISpecVersion()
        {
            // arrange
            OpenAPISpecReader openAPISpecReader = new OpenAPISpecReader();

            // act
            bool shouldOutputVersionTwo = await openAPISpecReader.isJSONOpenAPISpecVersionThreeAsync(String.Concat(this.openAPISpecFolder, "swaggerPetstore.json"));
            bool shouldOutputVersionThree = await openAPISpecReader.isJSONOpenAPISpecVersionThreeAsync(String.Concat(this.openAPISpecFolder, "swaggerPetstorev3.json"));

            // assert
            Assert.False(shouldOutputVersionTwo);
            Assert.True(shouldOutputVersionThree);
        }
    }
}
