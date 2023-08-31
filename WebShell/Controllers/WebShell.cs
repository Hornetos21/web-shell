using System.Diagnostics;

// TODO Create Dictionary for id process and id session
namespace WebShell.Controllers;

public class WebShell
{
    
    public Process CmdProcess;

    public void CommandRequest(string command)
    {
        // Console.CancelKeyPress += Console_CancelKeyPress;

        CmdProcess = new Process();
        CmdProcess.StartInfo = new ProcessStartInfo()
        {
            FileName = "cmd.exe",
            Arguments = @$"/c {command}",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
        };
    }

    public uint GetIdProcess()
    {
        return (uint)CmdProcess.Id;
    }
    
    public void Run()
    {
        CmdProcess.Start();
        CmdProcess.BeginOutputReadLine();
        CmdProcess.BeginErrorReadLine();
        CmdProcess.WaitForExitAsync();
        
    }
    
}