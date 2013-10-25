using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DotSmart
{
    class ProcessUtil
    {
        public static int Exec(string filename, string args, TextWriter stdOut, TextWriter stdErr, Encoding encoding = null, string workingDirectory = null)
        {
            using (Process process = new Process())
            {
                ProcessStartInfo psi = process.StartInfo;
                psi.RedirectStandardError = psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.FileName = filename;
                psi.Arguments = args;
                if (workingDirectory != null)
                    psi.WorkingDirectory = workingDirectory;

                if (encoding != null)
                {
                    psi.StandardOutputEncoding = encoding;
                    psi.StandardErrorEncoding = encoding;
                }

                if (stdOut != null)
                {
                    process.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
                    {
                        stdOut.WriteLine(e.Data);
                    };
                }
                if (stdErr != null)
                {
                    process.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e)
                    {
                        stdErr.WriteLine(e.Data);
                    };
                }
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();
                return process.ExitCode;
            }
        }

    }
}
