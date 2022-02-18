using System;
using Xunit;
using McMaster.Extensions.CommandLineUtils;
using System.IO;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Test
{
    public class CreateTests
    {
        private string invalidConfigurationFolder;

        public CreateTests()
        {
            this.invalidConfigurationFolder = String.Concat("..", Path.DirectorySeparatorChar,
                 "..", Path.DirectorySeparatorChar,
                   "..", Path.DirectorySeparatorChar,
                   "..", Path.DirectorySeparatorChar,
                   "apimtemplate.test", Path.DirectorySeparatorChar,
                   "CmdLine", Path.DirectorySeparatorChar,
                   "InvalidConfigurations", Path.DirectorySeparatorChar);
        }

        #region Unknown
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
            string[] args = new string[] { "--configurationFile", String.Concat(this.invalidConfigurationFolder, "invalidVersionSetDisplayName.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Unrecognized option '--configurationFile'", ex.Message);
        }
        #endregion

        #region BaseProperties
        [Fact]
        public void ShouldFailWithInvalidOutputLocation()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.invalidConfigurationFolder, "invalidOutputLocation.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Output location is required", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidVersion()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.invalidConfigurationFolder, "invalidVersion.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Version is required", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidAPIMServiceName()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.invalidConfigurationFolder, "invalidAPIMServiceName.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("APIM service name is required", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidLinking()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.invalidConfigurationFolder, "invalidLinking.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("LinkTemplatesBaseUrl is required for linked templates", ex.Message);
        }
        #endregion

        #region API
        [Fact]
        public void ShouldFailWithInvalidAPIConfiguration()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.invalidConfigurationFolder, "invalidAPI.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("API configuration is required", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidOpenAPISpec()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.invalidConfigurationFolder, "invalidOpenAPISpec.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Open API Spec is required", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidSuffix()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.invalidConfigurationFolder, "invalidSuffix.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("API suffix is required", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidAPIName()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.invalidConfigurationFolder, "invalidAPIName.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("API name is required", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidOperationPolicy()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.invalidConfigurationFolder, "invalidOperationPolicy.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Policy XML is required if an API operation is provided", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidDiagnosticLoggerId()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.invalidConfigurationFolder, "invalidDiagnosticLoggerId.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("LoggerId is required if an API diagnostic is provided", ex.Message);
        }
        #endregion

        #region APIVersionSet
        [Fact]
        public void ShouldFailWithInvalidVersionSetDisplayName()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.invalidConfigurationFolder, "invalidVersionSetDisplayName.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Display name is required if an API Version Set is provided", ex.Message);
        }
        #endregion

        #region Product
        [Fact]
        public void ShouldFailWithInvalidProductDisplayName()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.invalidConfigurationFolder, "invalidProductDisplayName.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Display name is required if an Product is provided", ex.Message);
        }
        #endregion

        #region Logger
        [Fact]
        public void ShouldFailWithInvalidLoggerDisplayName()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.invalidConfigurationFolder, "invalidLoggerName.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Name is required if an Logger is provided", ex.Message);
        }
        #endregion

        #region Backend
        [Fact]
        public void ShouldFailWithInvalidBackendDisplayName()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.invalidConfigurationFolder, "invalidBackendTitle.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Title is required if a Backend is provided", ex.Message);
        }
        #endregion

        #region AuthorizationServer
        [Fact]
        public void ShouldFailWithInvalidAuthorizationServerDisplayName()
        {
            var createCommand = new CreateCommand();
            string[] args = new string[] { "--configFile", String.Concat(this.invalidConfigurationFolder, "invalidAuthorizationServerDisplayName.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Display name is required if an Authorization Server is provided", ex.Message);
        }
        #endregion
    }
}
