using System;
using Xunit;
using McMaster.Extensions.CommandLineUtils;
using System.IO;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Test
{
    public class CreateTests
    {

        private string configExamplesFolder;

        public CreateTests()
        {
            this.configExamplesFolder = String.Concat("..", Path.DirectorySeparatorChar,
                   "..", Path.DirectorySeparatorChar,
                   "..", Path.DirectorySeparatorChar,
                   "..", Path.DirectorySeparatorChar,
                   "apimtemplate", Path.DirectorySeparatorChar,
                   "Creator", Path.DirectorySeparatorChar,
                   "ExampleFiles", Path.DirectorySeparatorChar,
                   "YAMLConfigs", Path.DirectorySeparatorChar);
        }

        [Fact]
        public void ShouldFailWithUnknownCommand()
        {
            var createCommand = new CreateCommand();
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute("test"));
            Assert.Contains("Unrecognized command or argument 'test'", ex.Message);
        }

        [Fact]
        public void ShouldFailWithUnknownOption()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configurationFile", String.Concat(this.configExamplesFolder, "valid.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Unrecognized option '--configurationFile'", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidOutputLocation()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.configExamplesFolder, "invalidOutputLocation.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Output location is required", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidVersion()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.configExamplesFolder, "invalidVersion.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Version is required", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidAPIMServiceName()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.configExamplesFolder, "invalidAPIMServiceName.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("APIM service name is required", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidAPIConfiguration()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.configExamplesFolder, "invalidAPI.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("API configuration is required", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidOpenAPISpec()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.configExamplesFolder, "invalidOpenAPISpec.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Open API Spec is required", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidSuffix()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.configExamplesFolder, "invalidSuffix.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("API suffix is required", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidAPIName()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.configExamplesFolder, "invalidAPIName.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("API name is required", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidLinking()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.configExamplesFolder, "invalidLinking.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("LinkTemplatesBaseUrl is required for linked templates", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidVersionSetDisplayName()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.configExamplesFolder, "invalidVersionSetDisplayName.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Display name is required if an API Version Set is provided", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidVersionSetVersioningScheme()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.configExamplesFolder, "invalidVersionSetVersioningScheme.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Versioning scheme is required if an API Version Set is provided", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidOperationPolicy()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.configExamplesFolder, "invalidOperationPolicy.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Policy XML is required if an API operation is provided", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidDiagnosticLoggerId()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.configExamplesFolder, "invalidDiagnosticLoggerId.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("LoggerId is required if an API diagnostic is provided", ex.Message);
        }
    }
}
