// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
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
