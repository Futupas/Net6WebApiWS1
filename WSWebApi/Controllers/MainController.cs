using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using WSWebApi;

namespace WSWebApi.Controllers;

public class MainController: ControllerBase
{
    private (WebSocket socket, Task task, CancellationTokenSource cancellationTokenSource) socketData {get; set;} = new();

    private async Task ExecuteWsData()
    {
        for (var i = 0; (i < 20 && socketData.socket.State == WebSocketState.Open && !socketData.cancellationTokenSource.IsCancellationRequested); i++)
        {
            var segments = new ArraySegment<byte>(Encoding.UTF8.GetBytes("hello world"));
            await socketData.socket.SendAsync(segments, WebSocketMessageType.Text, true, new());
            await Task.Delay(1000);
        }

        if (socketData.socket.State != WebSocketState.Open || socketData.cancellationTokenSource.IsCancellationRequested)
        {
            socketData.cancellationTokenSource.Cancel(false);
            socketData.task.Dispose();

            System.Console.WriteLine("WS was cancelled from out");
            return;
        }

        await socketData.socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, new());
        socketData.cancellationTokenSource.Cancel(false);
        socketData.task.Dispose();
        System.Console.WriteLine($"WS was cancelled");
    }


    [HttpGet("/ws")]
    public async Task GetWs()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        var cancellationTokenSource = new CancellationTokenSource();

        System.Console.WriteLine($"WS connected (HashCode: {this.GetHashCode()})");

        var task = Task.Run(() => { ExecuteWsData(); }, cancellationTokenSource.Token);

        socketData = (webSocket, task, cancellationTokenSource);

        while (!cancellationTokenSource.Token.IsCancellationRequested && webSocket.State == WebSocketState.Open)
        {
            var segments = new ArraySegment<byte>(new byte[100], 0, 100);
            var receiveResult = await webSocket.ReceiveAsync(segments, cancellationTokenSource.Token);
            if (receiveResult.MessageType == WebSocketMessageType.Text)
            {
                System.Console.WriteLine("Received webhook: " + Encoding.UTF8.GetString(segments));
            }
            else
            {
                System.Console.WriteLine("Received webhook message type: " + receiveResult.MessageType);
            }
            
        }
        
        System.Console.WriteLine($"WS disconnected");
    }
}
