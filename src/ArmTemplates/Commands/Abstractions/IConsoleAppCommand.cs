// --------------------------------------------------------------------------
//  <copyright file="ICommandLineApplication.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Abstractions
{
    interface IConsoleAppCommand<TConfiguration, TParameters>
    {
        public Task<TParameters> ParseInputConfigurationAsync(TConfiguration configuration);

        public Task ExecuteCommandAsync(TParameters parameters);
    }
}
