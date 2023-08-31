using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace WebShell.Controllers;

public class WebSocketController : ControllerBase
{
    private WebSocketReceiveResult _receiveResult = null!;
    private byte[] _buffer = new byte[1024 * 4];
    private string _canWrite = "CAN_WRITE_TRUE";
    public WebShell shell;


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
            Console.WriteLine("BAD_REQUEST");
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private async Task Echo(WebSocket webSocket)
    {
        Console.WriteLine("BEGIN ECHO");
        /* PENDING RECEIVE */
        _receiveResult = await WS_RECEIVE(webSocket);

        Console.WriteLine("FIRST_RECEIVE WS <- command");

        /* CONNECT */
        while (!_receiveResult.CloseStatus.HasValue)
        {
            var command = Encoding.UTF8.GetString(_buffer, 0, _receiveResult.Count);

            try
            {
                if (command == "cancel")
                {
                    /* Cancel process */
                    Console.WriteLine("CANCEL");
                    
                    try
                    {
                        KeyHandleCtrlC.StopProcess(shell.GetIdProcess());

                        // shell.CmdProcess.CancelOutputRead();
                        // shell.CmdProcess.CancelErrorRead();

                        _canWrite = "CAN_WRITE_TRUE";
                        WS_SEND(webSocket, _canWrite);
                        Console.WriteLine("WS_SEND_CANCEL_STREAM -> cawWrite");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }

                    /* Pending new command */
                    _receiveResult = await WS_RECEIVE(webSocket);
                }
                else
                {
                    shell = new WebShell();
                    var result = "";


                    Console.WriteLine($"SERVER COMMAND : {command}");

                    shell.CommandRequest(command);

                    _canWrite = "CAN_WRITE_FALSE";
                    WS_SEND(webSocket, _canWrite);


                    shell.CmdProcess.OutputDataReceived += (s, e) =>
                    {
                        if (e.Data != null)
                        {
                            result = e.Data + "\n";
                            Console.WriteLine("WS_SEND -> output");
                            WS_SEND(webSocket, result);
                        }
                        else
                        {
                            _canWrite = "CAN_WRITE_TRUE";
                            WS_SEND(webSocket, _canWrite);
                            Console.WriteLine("WS_SEND_END_STREAM -> cawWrite");
                        }
                    };
                    shell.CmdProcess.ErrorDataReceived += (s, e) =>
                    {
                        if (e.Data == null) return;
                        result = e.Data + "\n";
                        WS_SEND(webSocket, result);
                    };

                    Console.WriteLine("CAN_WRITE : " + _canWrite);
                    shell.Run();

                    _receiveResult = await WS_RECEIVE(webSocket);
                    Console.WriteLine("SECOND_RECEIVE WS <- command");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
                throw;
            }
        }

        /* CLOSE */
        WS_CLOSE(webSocket);
        Console.WriteLine("CLOSE");
    }

    private async Task<WebSocketReceiveResult> WS_RECEIVE(WebSocket webSocket)
    {
        return await webSocket.ReceiveAsync(
            new ArraySegment<byte>(_buffer), CancellationToken.None);
    }

    private async void WS_SEND(WebSocket webSocket, string output)
    {
        var mess = Encoding.UTF8.GetBytes(output);

        await webSocket.SendAsync(
            new ArraySegment<byte>(mess, 0, mess.Length),
            _receiveResult.MessageType,
            _receiveResult.EndOfMessage,
            CancellationToken.None);
    }

    private async void WS_CLOSE(WebSocket webSocket)
    {
        if (_receiveResult.CloseStatus != null)
            await webSocket.CloseAsync(
                _receiveResult.CloseStatus.Value,
                _receiveResult.CloseStatusDescription,
                CancellationToken.None);
    }
}