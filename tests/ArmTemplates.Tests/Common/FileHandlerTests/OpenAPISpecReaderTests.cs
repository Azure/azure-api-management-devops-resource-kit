// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Xunit;
using System.IO;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Common.FileHandlerTests
{
    public class OpenAPISpecReaderTests
    {
        string openAPISpecFolder;

        public OpenAPISpecReaderTests()
        {
            this.openAPISpecFolder = string.Concat(
                "Resources", Path.DirectorySeparatorChar,
               "OpenAPISpecs", Path.DirectorySeparatorChar);
        }
        [Fact]
        public async void ShouldDetermineCorrectOpenAPISpecVersion()
        {
            // arrangeW
            OpenAPISpecReader openAPISpecReader = new OpenAPISpecReader();

            // act
            bool shouldOutputVersionTwo = await openAPISpecReader.IsJSONOpenAPISpecVersionThreeAsync(string.Concat(this.openAPISpecFolder, "swaggerPetstore.json"));
            bool shouldOutputVersionThree = await openAPISpecReader.IsJSONOpenAPISpecVersionThreeAsync(string.Concat(this.openAPISpecFolder, "swaggerPetstorev3.json"));

            // assert
            Assert.False(shouldOutputVersionTwo);
            Assert.True(shouldOutputVersionThree);
        }
    }
}
