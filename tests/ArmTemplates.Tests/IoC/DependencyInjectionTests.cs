// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.IoC
{
    [Trait("Category", "Dependency Injection")]
    public class DependencyInjectionTests
    {
        [Fact]
        public void PresetServiceCollection_ResolvesExtractorExecutorCorrectly()
        {
            var consoleLogger = Program.SetupApplicationLoggingToConsole();
            var serviceProvider = Program.CreateServiceProvider(consoleLogger);

            var resolveExtractorExecutorAction = () =>
            {
                var extractorExecutor = serviceProvider.GetRequiredService<ExtractorExecutor>();
            };

            resolveExtractorExecutorAction.Should().NotThrow();
        }
    }
}
