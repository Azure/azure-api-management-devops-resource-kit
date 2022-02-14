using Xunit;
using System.IO;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Common.FileHandlerTests
{
    public class OpenAPISpecReaderTests
    {
        private string openAPISpecFolder;

        public OpenAPISpecReaderTests()
        {
            openAPISpecFolder = string.Concat("..", Path.DirectorySeparatorChar,
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
            // arrangeW
            OpenAPISpecReader openAPISpecReader = new OpenAPISpecReader();

            // act
            bool shouldOutputVersionTwo = await openAPISpecReader.isJSONOpenAPISpecVersionThreeAsync(string.Concat(openAPISpecFolder, "swaggerPetstore.json"));
            bool shouldOutputVersionThree = await openAPISpecReader.isJSONOpenAPISpecVersionThreeAsync(string.Concat(openAPISpecFolder, "swaggerPetstorev3.json"));

            // assert
            Assert.False(shouldOutputVersionTwo);
            Assert.True(shouldOutputVersionThree);
        }
    }
}
