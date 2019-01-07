using System.IO;
using Xunit;
using Newtonsoft.Json.Linq;
using System;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class ARMTemplateWriterTests
    {
        [Fact]
        public void ShouldWriteJSONToFile()
        {
            // arrange
            ARMTemplateWriter armTemplateWriter = new ARMTemplateWriter();
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
            armTemplateWriter.WriteJSONToFile(testJSON, location);

            // assert
            Assert.True(File.Exists(location));
            File.Delete(location);
        }
    }
}
