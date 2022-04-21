// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities
{
    class Executable
    {
        readonly string arguments;
        readonly string exeName;
        bool shareConsole;
        bool streamOutput;
        readonly bool visibleProcess;
        readonly string workingDirectory;

        public Executable(string exeName, string arguments = null, bool streamOutput = true, bool shareConsole = false, bool visibleProcess = false, string workingDirectory = null)
        {
            this.exeName = exeName;
            this.arguments = arguments;
            this.streamOutput = streamOutput;
            this.shareConsole = shareConsole;
            this.visibleProcess = visibleProcess;
            this.workingDirectory = workingDirectory;
        }

        public Process Process { get; private set; }

        public async Task<int> RunAsync(Action<string> outputCallback = null, Action<string> errorCallback = null, TimeSpan? timeout = null)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = exeName,
                Arguments = arguments,
                CreateNoWindow = !this.visibleProcess,
                UseShellExecute = shareConsole,
                RedirectStandardError = streamOutput,
                RedirectStandardInput = streamOutput,
                RedirectStandardOutput = streamOutput,
                WorkingDirectory = this.workingDirectory ?? Environment.CurrentDirectory
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

            if (this.streamOutput)
            {
                this.Process.OutputDataReceived += (s, e) => outputCallback?.Invoke(e.Data);
                this.Process.BeginOutputReadLine();
                this.Process.ErrorDataReceived += (s, e) => errorCallback?.Invoke(e.Data);
                this.Process.BeginErrorReadLine();
                this.Process.EnableRaisingEvents = true;
            }

            var exitCodeTask = this.Process.WaitForExitAsync();

            if (timeout == null)
            {
                await exitCodeTask;
                return exitCodeTask.IsCompletedSuccessfully ? 0 : -1;
            }
            else
            {
                await Task.WhenAny(exitCodeTask, Task.Delay(timeout.Value));

                if (exitCodeTask.IsCompleted)
                    return exitCodeTask.IsCompletedSuccessfully ? 0 : -1;
                else
                {
                    this.Process.Kill();
                    throw new Exception("Process didn't exit within specified timeout");
                }
            }
        }
    }
}