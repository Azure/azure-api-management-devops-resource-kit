﻿using System.IO;
using Xunit;
using Newtonsoft.Json.Linq;
using apimtemplate.Common.FileHandlers;
using apimtemplate.Common.Templates.Abstractions;

namespace apimtemplate.test.Common.FileHandlerTests
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
            APITemplateResource testObject = new APITemplateResource() { apiVersion = "" };
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
