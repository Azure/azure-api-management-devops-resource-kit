// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests
{
    public abstract class TestsBase
    {
        protected ILogger<T> GetTestLogger<T>()
        {
            var serilogConsoleLogger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            return new SerilogLoggerFactory(serilogConsoleLogger)
                .CreateLogger<T>();
        }
    }
}
