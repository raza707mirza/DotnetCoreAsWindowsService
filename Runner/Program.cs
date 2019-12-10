using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runner
{
    public class Program
    {
        private static Process _Process = null;
        public static void Main(string[] args) // args = [xyz.dll]
        {
            // Create a new process in which dotnet core API will run
            _Process = new Process();

            // Configure process
            ProcessStartInfo startInfo = new ProcessStartInfo();

            startInfo.UseShellExecute = false; // disable shell execute to prevent cmd window
            startInfo.RedirectStandardInput = true; // enable forked process to redirect input to primary process
            startInfo.RedirectStandardOutput = true; // enable forked process to redirect output to primary process
            startInfo.RedirectStandardError = true; // enable forked process to redirect error to primary process

            startInfo.Verb = "runas"; // run as administrator
            startInfo.FileName = @"C:\Windows\System32\cmd.exe"; // run dotnet command on windows command line
            startInfo.WorkingDirectory = Directory.GetCurrentDirectory(); // place runner.exe in the same folder where .NET Core dll is
            startInfo.Arguments = "/c dotnet " + string.Join(" ", args); // e.g. dotnet xyz.dll

            _Process.StartInfo = startInfo;
            _Process.Start();

            _Process.BeginOutputReadLine(); // start redirecting forked process output to primary process
            _Process.BeginErrorReadLine(); // start redirecting forked process errors to primary process

            Console.CancelKeyPress += Console_CancelKeyPress; // if cancel key is pressed then stop the forked process
            _Process.OutputDataReceived += Process_OutputDataReceived;
            _Process.ErrorDataReceived += Process_ErrorDataReceived;

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            _Process.WaitForExit();
        }

        private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                Console.WriteLine(e.Data);
                File.AppendAllText("Logs.txt", "Error: " + e.Data + "\n");
            }
            catch (Exception)
            {
            }
        }

        private static void Process_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            try
            {
                Console.WriteLine(e.Data);
                File.AppendAllText("Logs.txt", "Output: " + e.Data + "\n"); // Log standard output to file.
            }
            catch (Exception)
            {
            }
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Exit();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Exit();
        }

        public static void Exit()
        {
            _Process.StandardInput.Close(); // this will kill forked process
            Environment.Exit(0);
        }
    }
}
