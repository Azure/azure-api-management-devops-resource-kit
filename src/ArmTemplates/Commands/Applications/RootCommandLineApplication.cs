// --------------------------------------------------------------------------
//  <copyright file="RootCommandLineApplication.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Applications
{
    /// <summary>
    /// Built into DI registrar of command line commands.
    /// </summary>
    class RootCommandLineApplication : CommandLineApplication
    {
        readonly IServiceProvider serviceProvider;

        public RootCommandLineApplication(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.RegisterCommandLineCommands();
        }

        void RegisterCommandLineCommands()
        {
            foreach (var command in this.serviceProvider.GetServices<ICommand>())
            {
                var commandLineApp = command as CommandLineApplication;

                if (commandLineApp == null)
                {
                    throw new InvalidCastException("Commands must inherit from ICommand and CommandLineApplication");
                }

                this.Commands.Add(commandLineApp);
            }
        }
    }
}
