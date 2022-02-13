using McMaster.Extensions.CommandLineUtils;
using System.Threading.Tasks;

namespace apimtemplate.Commands.Abstractions
{
    public abstract class CommandLineApplicationBase : CommandLineApplication
    {
        protected CommandLineApplicationBase()
        {
            SetupApplicationAndCommands();

            this.HelpOption();

            this.OnExecute(async () => await ExecuteCommand());
        }

        protected abstract void SetupApplicationAndCommands();

        protected abstract Task<int> ExecuteCommand();
    }
}
