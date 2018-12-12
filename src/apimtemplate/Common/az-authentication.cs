using Colors.Net;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class Authentication
    {
        public async Task<string> GetAccessToken()
        {
            (bool cliSucceeded, string cliToken) = await TryGetAzCliToken();

            if (cliSucceeded) return cliToken;

            throw new Exception("Unable to connect to Azure. Make sure you have the `az` CLI or Azure PowerShell installed and logged in and try again");
        }

        private async Task<(bool succeeded, string token)> TryGetAzCliToken()
        {
            var az = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new Executable("cmd", "/c az " + Constants.azParameters)
                : new Executable("az", Constants.azParameters);

            var stdout = new StringBuilder();
            var stderr = new StringBuilder();
            var completed = az.RunAsync(o => stdout.AppendLine(o), e => stderr.AppendLine(e));

            if (await completed == 0)
                return (true, stdout.ToString().Trim(' ', '\n', '\r', '"'));            
            else
            {
                ColoredConsole.WriteLine(($"Unable to fetch access token from az cli. Error: {stderr.ToString().Trim(' ', '\n', '\r')}"));
                return (false, stdout.ToString().Trim(' ', '\n', '\r', '"'));
            }
        }

    }

}
