// See https://aka.ms/new-console-template for more information
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

class Program
{
    static async Task Main()
    {
        await SubscribeToTickers(new string[] { "ethusdt", "btcusdt" }, OnTickersUpdate);
    }

    static async Task SubscribeToTickers(string[] symbols, Action<JObject> onTickersUpdate)
    {
        var ws = new ClientWebSocket();
        var uri = new Uri($"wss://stream.binance.com:9443/ws/{string.Join("/", symbols)}@ticker");

        await ws.ConnectAsync(uri, CancellationToken.None);

        while (true)
        {
            var buffer = new byte[1024];
            var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var data = JObject.Parse(json);
                onTickersUpdate(data);
            }
        }
    }

    static void OnTickersUpdate(JObject data)
    {
        var symbol = data["s"].Value<string>();
        var price = data["c"].Value<string>();
        Console.WriteLine($"{symbol}: {price}");
    }
}