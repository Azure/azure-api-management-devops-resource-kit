using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using apimtemplate.Creator.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;
using Xunit;

namespace apimtemplate.test.Creator.ExtensionsTests
{
    public class SpecificationCopyTests
    {
        [Fact]
        public async Task CanCopyFile()
        {
            var config = new APIConfig {openApiSpec = "https://petstore.swagger.io/v2/swagger.json", openApiSpecCopyToLocation = "./testFileCopied.json"};
            
            await SpecificationCopy.CopyOpenApiSpecification(config);

            var output = string.Empty;
            using (var reader = new StreamReader(File.OpenRead(config.openApiSpecCopyToLocation)))
            {
                output = await reader.ReadToEndAsync();
            }

            Assert.False(string.IsNullOrEmpty(output));
        }

        [Fact]
        public async Task CanCopyFile2()
        {
            var config = new APIConfig
            {
                openApiSpec = "https://webclientintegration.blob.core.windows.net/swagger/ClientApiV1.json", openApiSpecCopyToLocation = "./testFileCopied.json", 
                sasToken = "se=2019-07-23T15%3A41Z&sp=r&sv=2018-03-28&ss=bqtf&srt=sco&sig=bMpR0FqyNe/ij6Ol3KC02GyVw8Q%2BglkTecAel5ZW8uY%3D"
            };
            
            await SpecificationCopy.CopyOpenApiSpecification(config);

            var output = string.Empty;
            using (var reader = new StreamReader(File.OpenRead(config.openApiSpecCopyToLocation)))
            {
                output = await reader.ReadToEndAsync();
            }

            Assert.False(string.IsNullOrEmpty(output));
        }
    }
}
