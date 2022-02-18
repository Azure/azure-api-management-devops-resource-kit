using McMaster.Extensions.CommandLineUtils;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Abstractions
{
    public abstract class CommandLineApplicationBase : CommandLineApplication
    {
        protected CommandLineApplicationBase()
        {
            SetupApplicationAndCommands();

            this.HelpOption();

            OnExecute(async () => await ExecuteCommand());
        }

        protected abstract void SetupApplicationAndCommands();

        protected abstract Task<int> ExecuteCommand();
    }
}
