using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using WSWebApi;

namespace WSWebApi.Controllers;

public class MainController: ControllerBase
{
    private Dictionary<string, (WebSocket socket, Task task, CancellationTokenSource cancellationTokenSource)> sockets {get; set;} = new();
    private Random random = new();

    [HttpGet("/data")]
    public async Task<IActionResult> GetData()
    {
        var code = DateTime.Now.Ticks.ToString() + "_" + random.NextDouble().ToString();

        // new Task(ExecuteWsData(code))
        // Task.Run(async () => { await ExecuteWsData(code); });

        
        return Ok(new {
            foo = "bar",
            wsCode = code,
        });
    }

    private async Task ExecuteWsData(string code)
    {
        var wsData = sockets[code];
        for (var i = 0; (i < 5 && wsData.socket.State == WebSocketState.Open && !wsData.cancellationTokenSource.IsCancellationRequested); i++)
        {
            var segments = new ArraySegment<byte>(Encoding.UTF8.GetBytes("hello world"));
            await wsData.socket.SendAsync(segments, WebSocketMessageType.Text, true, new());
            await Task.Delay(1000);
        }

        await wsData.socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, new());
        wsData.cancellationTokenSource.Cancel(false);
        wsData.task.Dispose();
        sockets.Remove(code);
        System.Console.WriteLine($"WS {code} was cancelled");
    }


    [HttpGet("/ws")]
    public async Task GetWs(string code)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        var cancellationTokenSource = new CancellationTokenSource();

        System.Console.WriteLine($"WS {code} connected");

        var code2 = code;
        var task = Task.Run(() => { ExecuteWsData(code2); }, cancellationTokenSource.Token);

        this.sockets.Add(code, (webSocket, task, cancellationTokenSource));

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
        
        System.Console.WriteLine($"WS {code} disconnected");

        return;
        // if (HttpContext.WebSockets.IsWebSocketRequest)
        // {
        //     using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        //     // using (var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
        //     // {
        //     //     sockets.Add(code, webSocket);
        //     //     webSocket.
        //     // }
            
        //     // webSocket.
        //     // await Echo(webSocket);

            
            
            
        //     // using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        //     // var socketFinishedTcs = new TaskCompletionSource<object>();

        //     // BackgroundSocketProcessor.AddSocket(webSocket, socketFinishedTcs);

        //     // await socketFinishedTcs.Task;

        // }
        // else
        // {
        //     HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        // }
    }
}
