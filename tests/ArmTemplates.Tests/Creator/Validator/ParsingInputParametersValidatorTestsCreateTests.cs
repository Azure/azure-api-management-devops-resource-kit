// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Xunit;
using System.IO;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Applications;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Configurations;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Exceptions;
using System;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.Validator
{
    public class ParsingInputParametersValidatorTests : TestsBase
    {
        string invalidConfigurationFolder;

        public ParsingInputParametersValidatorTests()
        {
            this.invalidConfigurationFolder = string.Concat(
                "Resources", Path.DirectorySeparatorChar,
                "InvalidConfigurations", Path.DirectorySeparatorChar);
        }

        CreateApplicationCommand GetCreateApplicationCommandTestInstance()
        {
            var logger = this.GetTestLogger<CreateApplicationCommand>();
            return new CreateApplicationCommand(logger, CreatorExecutor.BuildCreatorExecutor(null, null), new FileReader());
        }

        [Fact]
        public async Task ShouldFailWithUnknownCommand()
        {
            // arrange
            var createApplicationCommand = this.GetCreateApplicationCommandTestInstance();

            // act
            Func<Task> act = async () => await createApplicationCommand.ParseInputConfigurationAsync(new CreateConsoleAppConfiguration());

            // assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task ShouldFailWithUnknownOption()
        {
            // arrange
            var createApplicationCommand = this.GetCreateApplicationCommandTestInstance();

            // act
            Func<Task> act = async () => await createApplicationCommand.ParseInputConfigurationAsync(new CreateConsoleAppConfiguration
            {
                ConfigFile = string.Concat(this.invalidConfigurationFolder, "invalidVersionSetDisplayName.yml")
            });

            // assert
            await act.Should().ThrowAsync<CreatorConfigurationIsInvalidException>();
        }

        [Fact]
        public async Task ShouldFailWithInvalidOutputLocation()
        {
            // arrange
            var createApplicationCommand = this.GetCreateApplicationCommandTestInstance();

            // act
            Func<Task> act = async () => await createApplicationCommand.ParseInputConfigurationAsync(new CreateConsoleAppConfiguration
            {
                ConfigFile = string.Concat(this.invalidConfigurationFolder, "invalidOutputLocation.yml")
            });

            // assert
            await act.Should().ThrowAsync<CreatorConfigurationIsInvalidException>();
        }

        [Fact]
        public async Task ShouldFailWithInvalidVersion()
        {
            // arrange
            var createApplicationCommand = this.GetCreateApplicationCommandTestInstance();

            // act
            Func<Task> act = async () => await createApplicationCommand.ParseInputConfigurationAsync(new CreateConsoleAppConfiguration
            {
                ConfigFile = string.Concat(this.invalidConfigurationFolder, "invalidVersion.yml")
            });

            // assert
            await act.Should().ThrowAsync<CreatorConfigurationIsInvalidException>();
        }

        [Fact]
        public async Task ShouldFailWithInvalidAPIMServiceName()
        {
            // arrange
            var createApplicationCommand = this.GetCreateApplicationCommandTestInstance();

            // act
            Func<Task> act = async () => await createApplicationCommand.ParseInputConfigurationAsync(new CreateConsoleAppConfiguration
            {
                ConfigFile = string.Concat(this.invalidConfigurationFolder, "invalidAPIMServiceName.yml")
            });

            // assert
            await act.Should().ThrowAsync<CreatorConfigurationIsInvalidException>();
        }

        [Fact]
        public async Task ShouldFailWithInvalidLinking()
        {
            // arrange
            var createApplicationCommand = this.GetCreateApplicationCommandTestInstance();

            // act
            Func<Task> act = async () => await createApplicationCommand.ParseInputConfigurationAsync(new CreateConsoleAppConfiguration
            {
                ConfigFile = string.Concat(this.invalidConfigurationFolder, "invalidLinking.yml")
            });

            // assert
            await act.Should().ThrowAsync<CreatorConfigurationIsInvalidException>();
        }

        [Fact]
        public async Task ShouldFailWithInvalidAPIConfiguration()
        {
            // arrange
            var createApplicationCommand = this.GetCreateApplicationCommandTestInstance();

            // act
            Func<Task> act = async () => await createApplicationCommand.ParseInputConfigurationAsync(new CreateConsoleAppConfiguration
            {
                ConfigFile = string.Concat(this.invalidConfigurationFolder, "invalidAPI.yml")
            });

            // assert
            await act.Should().ThrowAsync<CreatorConfigurationIsInvalidException>();
        }

        [Fact]
        public async Task ShouldFailWithInvalidOpenAPISpec()
        {
            // arrange
            var createApplicationCommand = this.GetCreateApplicationCommandTestInstance();

            // act
            Func<Task> act = async () => await createApplicationCommand.ParseInputConfigurationAsync(new CreateConsoleAppConfiguration
            {
                ConfigFile = string.Concat(this.invalidConfigurationFolder, "invalidOpenAPISpec.yml")
            });

            // assert
            await act.Should().ThrowAsync<CreatorConfigurationIsInvalidException>();
        }

        [Fact]
        public async Task ShouldFailWithInvalidSuffix()
        {
            // arrange
            var createApplicationCommand = this.GetCreateApplicationCommandTestInstance();

            // act
            Func<Task> act = async () => await createApplicationCommand.ParseInputConfigurationAsync(new CreateConsoleAppConfiguration
            {
                ConfigFile = string.Concat(this.invalidConfigurationFolder, "invalidSuffix.yml")
            });

            // assert
            await act.Should().ThrowAsync<CreatorConfigurationIsInvalidException>();
        }

        [Fact]
        public async Task ShouldFailWithInvalidAPIName()
        {
            // arrange
            var createApplicationCommand = this.GetCreateApplicationCommandTestInstance();

            // act
            Func<Task> act = async () => await createApplicationCommand.ParseInputConfigurationAsync(new CreateConsoleAppConfiguration
            {
                ConfigFile = string.Concat(this.invalidConfigurationFolder, "invalidAPIName.yml")
            });

            // assert
            await act.Should().ThrowAsync<CreatorConfigurationIsInvalidException>();
        }

        [Fact]
        public async Task ShouldFailWithInvalidOperationPolicy()
        {
            // arrange
            var createApplicationCommand = this.GetCreateApplicationCommandTestInstance();

            // act
            Func<Task> act = async () => await createApplicationCommand.ParseInputConfigurationAsync(new CreateConsoleAppConfiguration
            {
                ConfigFile = string.Concat(this.invalidConfigurationFolder, "invalidOperationPolicy.yml")
            });

            // assert
            await act.Should().ThrowAsync<CreatorConfigurationIsInvalidException>();
        }

        [Fact]
        public async Task ShouldFailWithInvalidDiagnosticLoggerId()
        {
            // arrange
            var createApplicationCommand = this.GetCreateApplicationCommandTestInstance();

            // act
            Func<Task> act = async () => await createApplicationCommand.ParseInputConfigurationAsync(new CreateConsoleAppConfiguration
            {
                ConfigFile = string.Concat(this.invalidConfigurationFolder, "invalidDiagnosticLoggerId.yml")
            });

            // assert
            await act.Should().ThrowAsync<CreatorConfigurationIsInvalidException>();
        }

        [Fact]
        public async Task ShouldFailWithInvalidVersionSetDisplayName()
        {
            // arrange
            var createApplicationCommand = this.GetCreateApplicationCommandTestInstance();

            // act
            Func<Task> act = async () => await createApplicationCommand.ParseInputConfigurationAsync(new CreateConsoleAppConfiguration
            {
                ConfigFile = string.Concat(this.invalidConfigurationFolder, "invalidVersionSetDisplayName.yml")
            });

            // assert
            await act.Should().ThrowAsync<CreatorConfigurationIsInvalidException>();
        }

        [Fact]
        public async Task ShouldFailWithInvalidProductDisplayName()
        {
            // arrange
            var createApplicationCommand = this.GetCreateApplicationCommandTestInstance();

            // act
            Func<Task> act = async () => await createApplicationCommand.ParseInputConfigurationAsync(new CreateConsoleAppConfiguration
            {
                ConfigFile = string.Concat(this.invalidConfigurationFolder, "invalidProductDisplayName.yml")
            });

            // assert
            await act.Should().ThrowAsync<CreatorConfigurationIsInvalidException>();
        }

        [Fact]
        public async Task ShouldFailWithInvalidLoggerDisplayName()
        {
            // arrange
            var createApplicationCommand = this.GetCreateApplicationCommandTestInstance();

            // act
            Func<Task> act = async () => await createApplicationCommand.ParseInputConfigurationAsync(new CreateConsoleAppConfiguration
            {
                ConfigFile = string.Concat(this.invalidConfigurationFolder, "invalidLoggerName.yml")
            });

            // assert
            await act.Should().ThrowAsync<CreatorConfigurationIsInvalidException>();
        }

        [Fact]
        public async Task ShouldFailWithInvalidBackendDisplayName()
        {
            // arrange
            var createApplicationCommand = this.GetCreateApplicationCommandTestInstance();

            // act
            Func<Task> act = async () => await createApplicationCommand.ParseInputConfigurationAsync(new CreateConsoleAppConfiguration
            {
                ConfigFile = string.Concat(this.invalidConfigurationFolder, "invalidBackendTitle.yml")
            });

            // assert
            await act.Should().ThrowAsync<CreatorConfigurationIsInvalidException>();
        }

        [Fact]
        public async Task ShouldFailWithInvalidAuthorizationServerDisplayName()
        {
            // arrange
            var createApplicationCommand = this.GetCreateApplicationCommandTestInstance();

            // act
            Func<Task> act = async () => await createApplicationCommand.ParseInputConfigurationAsync(new CreateConsoleAppConfiguration
            {
                ConfigFile = string.Concat(this.invalidConfigurationFolder, "invalidAuthorizationServerDisplayName.yml")
            });

            // assert
            await act.Should().ThrowAsync<CreatorConfigurationIsInvalidException>();
        }
    }
}
