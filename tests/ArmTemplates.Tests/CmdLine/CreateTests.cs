using Xunit;
using System.IO;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Applications;
using McMaster.Extensions.CommandLineUtils;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.CmdLine
{
    public class CreateTests
    {
        private string invalidConfigurationFolder;

        public CreateTests()
        {
            invalidConfigurationFolder = string.Concat("..", Path.DirectorySeparatorChar,
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
            var createCommand = new CreateApplicationCommand();
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute("test"));
            Assert.Contains("Unrecognized command or argument 'test'", ex.Message);
        }

        [Fact]
        public void ShouldFailWithUnknownOption()
        {
            var createCommand = new CreateApplicationCommand();
            string[] args = new string[] { "--configurationFile", string.Concat(invalidConfigurationFolder, "invalidVersionSetDisplayName.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Unrecognized option '--configurationFile'", ex.Message);
        }
        #endregion

        #region BaseProperties
        [Fact]
        public void ShouldFailWithInvalidOutputLocation()
        {
            var createCommand = new CreateApplicationCommand();
            string[] args = new string[] { "--configFile", string.Concat(invalidConfigurationFolder, "invalidOutputLocation.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Output location is required", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidVersion()
        {
            var createCommand = new CreateApplicationCommand();
            string[] args = new string[] { "--configFile", string.Concat(invalidConfigurationFolder, "invalidVersion.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Version is required", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidAPIMServiceName()
        {
            var createCommand = new CreateApplicationCommand();
            string[] args = new string[] { "--configFile", string.Concat(invalidConfigurationFolder, "invalidAPIMServiceName.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("APIM service name is required", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidLinking()
        {
            var createCommand = new CreateApplicationCommand();
            string[] args = new string[] { "--configFile", string.Concat(invalidConfigurationFolder, "invalidLinking.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("LinkTemplatesBaseUrl is required for linked templates", ex.Message);
        }
        #endregion

        #region API
        [Fact]
        public void ShouldFailWithInvalidAPIConfiguration()
        {
            var createCommand = new CreateApplicationCommand();
            string[] args = new string[] { "--configFile", string.Concat(invalidConfigurationFolder, "invalidAPI.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("API configuration is required", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidOpenAPISpec()
        {
            var createCommand = new CreateApplicationCommand();
            string[] args = new string[] { "--configFile", string.Concat(invalidConfigurationFolder, "invalidOpenAPISpec.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Open API Spec is required", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidSuffix()
        {
            var createCommand = new CreateApplicationCommand();
            string[] args = new string[] { "--configFile", string.Concat(invalidConfigurationFolder, "invalidSuffix.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("API suffix is required", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidAPIName()
        {
            var createCommand = new CreateApplicationCommand();
            string[] args = new string[] { "--configFile", string.Concat(invalidConfigurationFolder, "invalidAPIName.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("API name is required", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidOperationPolicy()
        {
            var createCommand = new CreateApplicationCommand();
            string[] args = new string[] { "--configFile", string.Concat(invalidConfigurationFolder, "invalidOperationPolicy.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Policy XML is required if an API operation is provided", ex.Message);
        }

        [Fact]
        public void ShouldFailWithInvalidDiagnosticLoggerId()
        {
            var createCommand = new CreateApplicationCommand();
            string[] args = new string[] { "--configFile", string.Concat(invalidConfigurationFolder, "invalidDiagnosticLoggerId.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("LoggerId is required if an API diagnostic is provided", ex.Message);
        }
        #endregion

        #region APIVersionSet
        [Fact]
        public void ShouldFailWithInvalidVersionSetDisplayName()
        {
            var createCommand = new CreateApplicationCommand();
            string[] args = new string[] { "--configFile", string.Concat(invalidConfigurationFolder, "invalidVersionSetDisplayName.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Display name is required if an API Version Set is provided", ex.Message);
        }
        #endregion

        #region Product
        [Fact]
        public void ShouldFailWithInvalidProductDisplayName()
        {
            var createCommand = new CreateApplicationCommand();
            string[] args = new string[] { "--configFile", string.Concat(invalidConfigurationFolder, "invalidProductDisplayName.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Display name is required if an Product is provided", ex.Message);
        }
        #endregion

        #region Logger
        [Fact]
        public void ShouldFailWithInvalidLoggerDisplayName()
        {
            var createCommand = new CreateApplicationCommand();
            string[] args = new string[] { "--configFile", string.Concat(invalidConfigurationFolder, "invalidLoggerName.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Name is required if an Logger is provided", ex.Message);
        }
        #endregion

        #region Backend
        [Fact]
        public void ShouldFailWithInvalidBackendDisplayName()
        {
            var createCommand = new CreateApplicationCommand();
            string[] args = new string[] { "--configFile", string.Concat(invalidConfigurationFolder, "invalidBackendTitle.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Title is required if a Backend is provided", ex.Message);
        }
        #endregion

        #region AuthorizationServer
        [Fact]
        public void ShouldFailWithInvalidAuthorizationServerDisplayName()
        {
            var createCommand = new CreateApplicationCommand();
            string[] args = new string[] { "--configFile", string.Concat(invalidConfigurationFolder, "invalidAuthorizationServerDisplayName.yml") };
            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute(args));
            Assert.Contains("Display name is required if an Authorization Server is provided", ex.Message);
        }
        #endregion
    }
}
