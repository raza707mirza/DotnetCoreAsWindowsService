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
        private static System.Diagnostics.Process _Process = null;
        public static void Main(string[] args)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.Verb = "runas";
            //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = @"C:\Windows\System32\cmd.exe";
            startInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            startInfo.Arguments = "/c dotnet " + string.Join(" ", args);
            process.StartInfo = startInfo;
            process.Start();
            _Process = process;
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();


            Console.CancelKeyPress += Console_CancelKeyPress;
            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_ErrorDataReceived;

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;



            process.WaitForExit();
        }

        private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                Console.WriteLine(e.Data);
                System.IO.File.AppendAllText("Logs.txt", "Error: " + e.Data + "\n");
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
                System.IO.File.AppendAllText("Logs.txt", "Output: " + e.Data + "\n");
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
            _Process.StandardInput.Close();
            Environment.Exit(0);
        }
    }
}
