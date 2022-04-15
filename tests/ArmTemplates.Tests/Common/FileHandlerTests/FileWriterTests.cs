// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.IO;
using Xunit;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Common.FileHandlerTests
{
    public class FileWriterTests
    {
        [Fact]
        public void ShouldWriteJSONToFile()
        {
            // arrange
            string location = string.Concat("..", Path.DirectorySeparatorChar,
                   "..", Path.DirectorySeparatorChar,
                   "..", Path.DirectorySeparatorChar,
                   "Creator", Path.DirectorySeparatorChar,
                   "example.json");
            APITemplateResource testObject = new APITemplateResource() { ApiVersion = "" };
            JObject testJSON = JObject.FromObject(testObject);

            // delete existing file if exists
            if (File.Exists(location))
            {
                File.Delete(location);
            }

            // write new 
            FileWriter.WriteJSONToFile(testJSON, location);

            // assert
            Assert.True(File.Exists(location));
            File.Delete(location);
        }
    }
}
