using System;
using Xunit;
using McMaster.Extensions.CommandLineUtils;
using System.IO;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
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
                   "Configurations", Path.DirectorySeparatorChar,
                   "Examples", Path.DirectorySeparatorChar);
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
        public void ShouldNotFailWithValidConfig()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.configExamplesFolder, "valid.yml") };
            createCommand.Execute(args);
        }
    }
}
