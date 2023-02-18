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
        //await SubscribeToTickers(new string[] { "ethusdt" }, ShowAllData);
    }

    // 訂閱報價
    static async Task SubscribeToTickers(string[] symbols, Action<JObject> onTickersUpdate)
    {
        var ws = new ClientWebSocket();
        var uri = new Uri($"wss://stream.binance.com:9443/stream?streams={string.Join("/", symbols.Select(s => s + "@ticker"))}");
        //wss://stream.binance.com:9443/stream?streams=ethusdt@ticker/btcusdt@ticker

        await ws.ConnectAsync(uri, CancellationToken.None);

        var buffer = new byte[1024];
        var segment = new ArraySegment<byte>(buffer);

        while (ws.State == WebSocketState.Open)
        {
            var result = await ws.ReceiveAsync(segment, CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var obj = JObject.Parse(json);
                onTickersUpdate(obj);
            }
        }
    }

    //顯示Json全部資料
    static void ShowAllData(JObject data)
    {
        Console.WriteLine(data);
    }
    //報價資料處理
    static void OnTickersUpdate(JObject data)
    {
        var symbol = data["data"]["s"].Value<string>();
        var price = data["data"]["c"].Value<string>();
        Console.WriteLine($"{symbol}: {price}");
    }

}