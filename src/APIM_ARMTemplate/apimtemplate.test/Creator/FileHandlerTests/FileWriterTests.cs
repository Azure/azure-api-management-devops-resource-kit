using System.IO;
using Xunit;
using Newtonsoft.Json.Linq;
using System;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Test
{
    public class FileWriterTests
    {
        [Fact]
        public void ShouldWriteJSONToFile()
        {
            // arrange
            FileWriter fileWriter = new FileWriter();
            string location = String.Concat("..", Path.DirectorySeparatorChar,
                   "..", Path.DirectorySeparatorChar,
                   "..", Path.DirectorySeparatorChar,
                   "Creator", Path.DirectorySeparatorChar,
                   "example.json");
            APITemplateResource testObject = new APITemplateResource() { apiVersion = "" };
            JObject testJSON = JObject.FromObject(testObject);

            // delete existing file if exists
            if (File.Exists(location))
            {
                File.Delete(location);
            }
            // write new 
            fileWriter.WriteJSONToFile(testJSON, location);

            // assert
            Assert.True(File.Exists(location));
            File.Delete(location);
        }
    }
}
