using McMaster.Extensions.CommandLineUtils;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Abstractions
{
    public abstract class CommandLineApplicationBase : CommandLineApplication
    {
        protected CommandLineApplicationBase()
        {
            this.SetupApplicationAndCommands();

            this.HelpOption();

            this.OnExecute(async () => await this.ExecuteCommand());
        }

        protected abstract void SetupApplicationAndCommands();

        protected abstract Task<int> ExecuteCommand();
    }
}
