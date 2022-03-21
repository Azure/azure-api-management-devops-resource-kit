using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities
{
    class Executable
    {
        string _arguments;
        string _exeName;
        bool _shareConsole;
        bool _streamOutput;
        readonly bool _visibleProcess;
        readonly string _workingDirectory;

        public Executable(string exeName, string arguments = null, bool streamOutput = true, bool shareConsole = false, bool visibleProcess = false, string workingDirectory = null)
        {
            this._exeName = exeName;
            this._arguments = arguments;
            this._streamOutput = streamOutput;
            this._shareConsole = shareConsole;
            this._visibleProcess = visibleProcess;
            this._workingDirectory = workingDirectory;
        }

        public Process Process { get; private set; }

        public async Task<int> RunAsync(Action<string> outputCallback = null, Action<string> errorCallback = null, TimeSpan? timeout = null)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = _exeName,
                Arguments = _arguments,
                CreateNoWindow = !this._visibleProcess,
                UseShellExecute = _shareConsole,
                RedirectStandardError = _streamOutput,
                RedirectStandardInput = _streamOutput,
                RedirectStandardOutput = _streamOutput,
                WorkingDirectory = this._workingDirectory ?? Environment.CurrentDirectory
            };

            try
            {
                this.Process = Process.Start(processInfo);
            }
            catch (Win32Exception ex)
            {
                if (ex.Message == "The system cannot find the file specified")
                {
                    throw new FileNotFoundException(ex.Message, ex);
                }
                throw ex;
            }

            if (this._streamOutput)
            {
                this.Process.OutputDataReceived += (s, e) => outputCallback?.Invoke(e.Data);
                this.Process.BeginOutputReadLine();
                this.Process.ErrorDataReceived += (s, e) => errorCallback?.Invoke(e.Data);
                this.Process.BeginErrorReadLine();
                this.Process.EnableRaisingEvents = true;
            }

            var exitCodeTask = this.Process.WaitForExitAsync();

            if (timeout == null)
                return await exitCodeTask;
            else
            {
                await Task.WhenAny(exitCodeTask, Task.Delay(timeout.Value));

                if (exitCodeTask.IsCompleted)
                    return exitCodeTask.Result;
                else
                {
                    this.Process.Kill();
                    throw new Exception("Process didn't exit within specified timeout");
                }
            }
        }
    }
}