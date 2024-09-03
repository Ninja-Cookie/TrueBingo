using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace TrueBingo.BingoSync
{
    internal class BingoSyncSocket
    {
        private static readonly Uri BingoSync = new Uri("wss://sockets.bingosync.com/broadcast");

        private static bool ConnectedToSocket = false;

        private static ClientWebSocket webSocket;

        public static async Task OpenSocket(string socket_key)
        {
            if (!ConnectedToSocket)
            {
                byte[] socketData = Encoding.ASCII.GetBytes(socket_key);

                using (webSocket = new ClientWebSocket())
                {
                    await webSocket.ConnectAsync(BingoSync, CancellationToken.None);
                    await webSocket.SendAsync(new ArraySegment<byte>(socketData), WebSocketMessageType.Text, true, CancellationToken.None);

                    Console.WriteLine("Connected to Socket!");
                    ConnectedToSocket = true;

                    await ReceiveMessage(webSocket);
                }
            }
        }

        private static async Task ReceiveMessage(ClientWebSocket webSocket)
        {
            byte[] buffer = new byte[1024];

            int attempts = 1;

            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = null;

                try
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    attempts = 1;
                }
                catch
                {
                    if (attempts <= 5)
                    {
                        attempts++;
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        continue;
                    }
                    else
                    {
                        ConnectedToSocket = false;
                    }
                }

                if (!BingoSyncHandler.ConnectedToRoom || (result != null && result.MessageType == WebSocketMessageType.Close))
                {
                    await CloseSocket();
                    Console.WriteLine("Socket was Closed.");
                }
                else if (result != null)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    if (message != null && message != string.Empty)
                        BingoSyncHandler.SendNotification(message);
                }
            }

            ConnectedToSocket = false;
        }

        public static async Task CloseSocket()
        {
            if (webSocket != null && webSocket.State == WebSocketState.Open)
            {
                await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                await webSocket.CloseAsync      (WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                ConnectedToSocket = false;
            }
        }

        public static string GetJsonValue(string json, string value, string subvalue = null)
        {
            json = json.Trim();

            if (json.StartsWith("{") && json.EndsWith("}"))
                json = json.Substring(1, json.Length - 1).Trim();

            List<string> jsonEntries = new List<string>() { json };

            if (json.Contains(","))
                jsonEntries = json.Split(',').ToList();

            if (subvalue != null)
                jsonEntries = jsonEntries.SkipWhile(x => !x.ToLower().Replace(" ", "").Replace("\"", "").Trim().Contains($"{value.ToLower()}:{{")).ToList();
            else
                subvalue = value;

            if (jsonEntries.Count > 0)
            {
                string entry = jsonEntries.FirstOrDefault(x => x.ToLower().Contains(subvalue.ToLower()));

                if (entry != null && entry != string.Empty)
                {
                    if (entry.Contains("{"))
                        entry = entry.Substring(entry.IndexOf('{'));

                    if (entry.EndsWith("}"))
                        entry = entry.Substring(0, entry.Length - 1);

                    int startIndex = entry.IndexOf(':');

                    if (startIndex != -1)
                        return entry.Substring(startIndex + 1).Replace("\"", "").Trim();
                }
            }
            return string.Empty;
        }
    }
}
