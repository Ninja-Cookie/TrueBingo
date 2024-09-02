using BepInEx.Configuration;
using Reptile;
using Reptile.Phone;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace TrueBingo.BingoSync
{
    public class BingoSyncHandler
    {
        private static CookieContainer sessionCookies;

        private static bool debug = false;

        const string URL_BingoSync              = "https://bingosync.com/";
        const string URL_BignoSyncBoard         = "https://bingosync.com/room/{0}/board";
        const string URL_BignoSyncDisconnect    = "https://bingosync.com/room/{0}/disconnect";
        const string URL_API_JoinRoom           = "https://bingosync.com/api/join-room";
        const string URL_API_Select             = "https://bingosync.com/api/select";
        const string URL_API_Color              = "https://bingosync.com/api/color";

        private static RoomInfo roomInfo;
        private struct RoomInfo
        {
            public string       RoomID;
            public string       PlayerName;
            public string       RoomPassword;
            public PlayerColors PlayerColor;

            public RoomInfo(string RoomID, string PlayerName, string RoomPassword, PlayerColors PlayerColor)
            {
                this.RoomID         = RoomID;
                this.PlayerName     = PlayerName;
                this.RoomPassword   = RoomPassword;
                this.PlayerColor    = PlayerColor;
            }
        }

        const string blankColor = "blank";

        const int   retryFailAmount     = 3;
        const float retryFailDelay      = 1f;
        const float nextResponseDelay   = 1f;
        const float boardCacheTime      = 1f;

        private static string   previousBoardRequest = string.Empty;
        private static float    nextBoardRequestTime = 0f;

        public static bool      ConnectedToRoom  = false;
        public static bool      ConnectingToRoom { get; private set; }
        public static bool      DisconnectingFromRoom { get; private set; }
        public static string    ConnectionStatus => ConnectedToRoom ? "Connected" : ConnectingToRoom ? "Connecting..." : "Disconnected";

        public enum PlayerColors
        {
            Orange,
            Red,
            Blue,
            Green,
            Purple,
            Navy,
            Teal,
            Brown,
            Pink,
            Yellow
        }

        private static void SendDebugMessage(string message)
        {
            if (debug)
                Console.WriteLine(message);
        }

        public static async void ConnectToRoom(string room, string password, string name, PlayerColors playerColor)
        {
            if (!ConnectedToRoom && !ConnectingToRoom)
            {
                BingoSyncGUI.errorMessage = string.Empty;

                ConnectingToRoom = true;
                await JoinRoom(room, password, name, playerColor);
            }
        }

        private static async Task JoinRoom(string room, string roomPassword, string name, PlayerColors playerColor)
        {
            SendDebugMessage("Joining...");

            if (ConnectedToRoom)
                return;

            roomInfo = new RoomInfo(room, name, roomPassword, playerColor);

            SendDebugMessage("Set Room Info...");

            sessionCookies = await GetBingoSyncCookies();

            SendDebugMessage("Cookies Created...");

            if (sessionCookies == null)
                return;

            SendDebugMessage("Cookies Passed...");

            HttpWebResponse response = await TryGetResponse(URL_API_JoinRoom, JsonUtility.ToJson(new Room(roomInfo.RoomID, roomInfo.PlayerName, roomInfo.RoomPassword)));

            SendDebugMessage("Response Found...");

            if (response != null)
            {
                string responseString = string.Empty;

                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    responseString = await reader.ReadToEndAsync();

                    SendDebugMessage("Connected to BingoSync!");
                    SendDebugMessage(responseString);
                }

                response.Dispose();

                ConnectedToRoom     = true;
                ConnectingToRoom    = false;

                await SetPlayerColor(roomInfo.PlayerColor);

                UpdateConfig();

                if (responseString != string.Empty)
                    await BingoSyncSocket.OpenSocket(responseString);
            }
            else
            {
                BingoSyncGUI.errorMessage = "Failed To Connect - Check Room ID / Password";
            }

            ConnectingToRoom = false;
        }

        private static void UpdateConfig()
        {
            BingoConfig.config_bingosync.SetConfigValue<string>(BingoConfig.config_selection_bingosync, BingoConfig.bingoSyncEntry_roomID,      roomInfo.RoomID);
            BingoConfig.config_bingosync.SetConfigValue<string>(BingoConfig.config_selection_bingosync, BingoConfig.bingoSyncEntry_password,    roomInfo.RoomPassword);
            BingoConfig.config_bingosync.SetConfigValue<string>(BingoConfig.config_selection_bingosync, BingoConfig.bingoSyncEntry_name,        roomInfo.PlayerName);
            BingoConfig.config_bingosync.SetConfigValue<PlayerColors>(BingoConfig.config_selection_bingosync, BingoConfig.bingoSyncEntry_color, roomInfo.PlayerColor);
        }

        public static async void Disconnect()
        {
            if (!DisconnectingFromRoom)
            {
                DisconnectingFromRoom = true;
                await LeaveRoom();
            }
        }

        private static async Task LeaveRoom()
        {
            if (ConnectedToRoom)
            {
                await TryGetResponse(string.Format(URL_BignoSyncDisconnect, roomInfo.RoomID), dispose: true);

                ConnectedToRoom         = false;
                DisconnectingFromRoom   = false;

                await BingoSyncSocket.CloseSocket();
            }
        }

        private static async Task<HttpWebResponse> TryGetResponse(string URL, string post = null, bool dispose = false)
        {
            int     attempt = 1;

            HttpWebResponse response        = null;
            HttpWebResponse attemptResponse = null;

            while (attempt <= retryFailAmount)
            {
                try
                {
                    HttpWebRequest webRequest   = (HttpWebRequest)WebRequest.Create(URL);
                    webRequest.CookieContainer  = sessionCookies;

                    if (post != null)
                    {
                        webRequest.Method       = "POST";
                        webRequest.ContentType  = "application/json; charset=UTF-8";
                        webRequest.Accept       = "application/json";

                        using (var stream = new StreamWriter(await webRequest.GetRequestStreamAsync()))
                        {
                            await stream.WriteAsync(post);
                        }
                    }

                    attemptResponse = (HttpWebResponse)await webRequest.GetResponseAsync();

                    if (attemptResponse != null)
                    {
                        response = attemptResponse;
                        break;
                    }
                }
                catch
                {
                    SendDebugMessage("- RETRYING CONNECTION FOR POST REQUEST -");
                    SendDebugMessage(URL);

                    if (attemptResponse != null)
                        attemptResponse.Dispose();

                    await Task.Delay(Mathf.FloorToInt(retryFailDelay * 1000));
                }

                attempt++;
            }

            if (dispose && response != null)
                response.Dispose();
            else if (response != null)
                HandleDispose(response);

            return response;
        }

        private static async void HandleDispose(HttpWebResponse response)
        {
            await DisposeAfter(response, 10);
        }

        private static async Task DisposeAfter(HttpWebResponse response, int seconds)
        {
            await Task.Delay(TimeSpan.FromSeconds(seconds));

            if (response != null)
            {
                try
                {
                    SendDebugMessage($"Disposed - Response from ({response.ResponseUri.AbsoluteUri})");
                    response.Dispose();
                }
                catch
                {
                    SendDebugMessage("Response Correctly Disposed Previously");
                }
            }
        }

        private static async Task<CookieContainer> GetBingoSyncCookies()
        {
            HttpWebResponse bingoSyncResponse = await TryGetResponse(URL_BingoSync);

            if (bingoSyncResponse != null)
            {
                return GetCookies(bingoSyncResponse, URL_BingoSync);
            }

            return null;
        }

        private static CookieContainer GetCookies(HttpWebResponse response, string host)
        {
            String getCookieHeader = response.Headers[HttpResponseHeader.SetCookie];
            response.Dispose();

            CookieContainer cookieContainier = new CookieContainer();

            if (!(getCookieHeader is null) && getCookieHeader != String.Empty)
            {
                foreach (var cookie in getCookieHeader.Split(';'))
                {
                    string[] values = cookie.Split('=');

                    if (values.Length > 1)
                    {
                        Cookie cookieTrim = new Cookie(values[0].Trim(), values[1].Trim()) { Domain = host };
                        try { cookieContainier.Add(cookieTrim); } catch { }
                    }
                }
            }

            return cookieContainier;
        }

        public static async Task SendNotification(string response)
        {
            await Task.Run(() =>
            {
                if (BingoSyncSocket.GetJsonValue(response, "type") == "goal" && BingoSyncSocket.GetJsonValue(response, "square", "colors") != "blank")
                {
                    string player       = BingoSyncSocket.GetJsonValue(response, "player", "name");
                    string collection   = BingoSyncSocket.GetJsonValue(response, "square", "name");

                    string itemToSend = collection;

                    string stage = StageToName.Values.ToArray().FirstOrDefault(x => collection.Trim().StartsWith(x) || collection.Trim().EndsWith($"({x})") || (collection.ToLower().Contains(" rep ") && collection.Contains(x)));

                    if (collection.Contains(" - "))
                        itemToSend = Regex.Match(collection, @" - (.+?) - ").Value.Replace(" - ", "").Trim();
                    else if (Regex.Match(collection, @"\((.+?)\)").Success)
                        itemToSend = collection.Substring(0, collection.IndexOf('(') - 1).Trim();
                    else if (collection.ToLower().Contains(" rep "))
                        itemToSend = collection.Substring(collection.ToLower().IndexOf("rep "));

                    if (player != string.Empty && player.ToLower().Replace(" ", "").Trim() != roomInfo.PlayerName.ToLower().Replace(" ", "").Trim())
                    {
                        string notification = $"{player}: \"{itemToSend}\"";

                        if (stage != null && stage != string.Empty)
                            notification = $"{notification} ({stage})";

                        SendPhoneNotification(notification);
                        SendDebugMessage(notification);
                    }
                }
            });
        }

        private static void SendPhoneNotification(string notification)
        {
            WorldHandler.instance?.GetCurrentPlayer()?.GetValue<Phone>("phone")?.GetAppInstance<AppEmail>().PushNotification(notification, null);
        }

        private static async Task<string> GetBoard(string room)
        {
            if (nextBoardRequestTime > Time.time)
            {
                if (previousBoardRequest == string.Empty)
                    await Task.Delay(Mathf.FloorToInt(nextResponseDelay * 1000));
                else
                    return previousBoardRequest;
            }

            nextBoardRequestTime = Time.time + boardCacheTime;

            string responseString = string.Empty;

            var response = await TryGetResponse(string.Format(URL_BignoSyncBoard, room));

            SendDebugMessage($"{response}");

            if (response != null)
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    responseString = await reader.ReadToEndAsync();
                }
                response.Dispose();
            }

            previousBoardRequest = responseString;

            return responseString;
        }

        private static async Task<Slot> GetSlotInfo(string name, ObjectiveType objectiveType, Pickup.PickUpType? pickupType)
        {
            string board = await GetBoard(roomInfo.RoomID);

            if (board == string.Empty)
                return null;

            string stage = string.Empty;
            StageToName.TryGetValue(Utility.GetCurrentStage(), out stage);

            if (stage == string.Empty)
                return null;

            List<Slot> slots = BuildBoardSlots(board);

            Slot slot = null;

            switch (objectiveType)
            {
                case ObjectiveType.ItemPickup:
                    if (pickupType != null)
                    {
                        string itemPickedUp = name.ToLower();

                        if (pickupType != Pickup.PickUpType.OUTFIT_UNLOCKABLE)
                        {
                            Slot[] possibleSlots = slots.Where(x => x.colors == blankColor && x.name.ToLower().StartsWith(stage.ToLower()) && !x.name.Contains(':') && x.name.ToCharArray().Where(y => y.Equals('-')).Count() >= 2).ToArray();

                            slot = possibleSlots.FirstOrDefault(x => x.name.ToLower().Replace(" ", string.Empty).Equals(itemPickedUp.Replace(" ", string.Empty)));

                            if (slot == default)
                                slot = slots.Where(x => x.colors == blankColor && x.name.ToLower().StartsWith(stage.ToLower()) && !x.name.Contains(':') && x.name.ToCharArray().Where(y => y.Equals('-')).Count() >= 2).FirstOrDefault(z => CompareItems(itemPickedUp, GetItemFromSlotName(z.name)));
                        }
                        else
                        {
                            slot = slots.Where(x => x.colors == blankColor && x.name.Contains(':')).FirstOrDefault(x => GetItemFromSlotName(x.name).Contains(itemPickedUp));
                        }
                    }
                break;

                case ObjectiveType.CharacterUnlock:
                    name = name.ToLower().Trim();

                    if (name.Contains(' '))
                        name = name.Split(' ')[0];

                    SendDebugMessage($"unlock {name}");
                    slot = slots.FirstOrDefault(x => x.colors == blankColor && x.name.ToLower().Contains($"({stage.ToLower()})") && x.name.ToLower().Contains($"unlock {name}"));
                break;

                case ObjectiveType.TaxiDriver:
                    slot = slots.FirstOrDefault(x => x.colors == blankColor && x.name.ToLower().Contains($"({stage.ToLower()})") && x.name.ToLower().Contains(name.ToLower()));
                break;

                case ObjectiveType.Rep:
                    name = name.ToLower().Trim();

                    if (name.Contains(' '))
                        name = name.Split(' ')[0];

                    slot = slots.FirstOrDefault(x => x.colors == blankColor && x.name.ToLower().Contains(" rep ") && x.name.ToLower().Contains(name));
                break;
            }
            return slot;
        }

        private static List<Slot> BuildBoardSlots(string board)
        {
            string[] squares = board.Substring(1, board.Length - 2).Replace(", {", "{").Split('}');

            List<Slot> slots = new List<Slot>();

            foreach (var square in squares)
            {
                if (square != string.Empty)
                    slots.Add(JsonUtility.FromJson<Slot>(square + "}"));
            }

            return slots;
        }

        private static string GetItemFromSlotName(string name)
        {
            string regexMatch = Regex.Match(name, @"- (.+?) -").Value;
            return regexMatch.Length > 0 ? regexMatch.Substring(1, regexMatch.Length - 2).Trim().ToLower() : string.Empty;
        }

        private static bool CompareItems(string pickup, string board)
        {
            pickup  = pickup.ToLower();
            board   = board .ToLower();

            if (pickup.Replace(" ", string.Empty).Trim().Equals(board.Replace(" ", string.Empty).Trim()))
            {
                SendDebugMessage($"Exact Match: {pickup} [VS] {board}");
                return true;
            }

            int total           = 0;
            int searchLength    = 2;
            int bonus           = 3;

            for(int i = 0; i < board.Length; i++)
            {
                if (i + (searchLength - 1) <= board.Length - 1)
                {
                    string search = board.Substring(i, searchLength);

                    if (pickup.Contains(search))
                        total += bonus;
                }
                else
                {
                    total--;
                }
            }

            int correction = Mathf.Abs(pickup.Length - board.Length);

            total -= correction;

            int expectedAmount = Mathf.FloorToInt( ( (board.Length * (bonus * 0.5f)) - correction ) );

            SendDebugMessage("-----");
            SendDebugMessage($"{pickup} [VS] {board}");
            SendDebugMessage($"Score: {total}");
            SendDebugMessage($"Expected: {expectedAmount}");
            SendDebugMessage("-----");

            return total >= expectedAmount;
        }

        public enum ObjectiveType
        {
            ItemPickup,
            CharacterUnlock,
            TaxiDriver,
            Rep
        }

        private static Dictionary<Stage, string> StageToName = new Dictionary<Stage, string>()
        {
            { Stage.hideout,    "Hideout"   },
            { Stage.tower,      "Brink"     },
            { Stage.Mall,       "Mall"      },
            { Stage.osaka,      "Mataan"    },
            { Stage.square,     "Square"    },
            { Stage.downhill,   "Versum"    },
            { Stage.pyramid,    "Pyramid"   },
        };

        public static async Task MarkSquare(string name, ObjectiveType objectiveType, Pickup.PickUpType? pickupType = null)
        {
            Slot slot = await GetSlotInfo(name, objectiveType, pickupType);

            if (slot == null || slot.colors != blankColor)
                return;

            var response = await TryGetResponse(URL_API_Select, JsonUtility.ToJson(new Select(roomInfo.RoomID, slot.slot.Replace("slot", ""), roomInfo.PlayerColor.ToString().ToLower(), false)));

            if (response != null)
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string responseString = await reader.ReadToEndAsync();
                    SendDebugMessage(responseString);
                }
                response.Dispose();
            }
        }

        public static bool SettingColor = false;

        public static async void UpdatePlayerColor(PlayerColors color)
        {
            if (ConnectedToRoom && !DisconnectingFromRoom && !SettingColor)
            {
                SettingColor = true;
                await SetPlayerColor(color);
            }
            SettingColor = false;
        }

        private static async Task SetPlayerColor(PlayerColors newColor)
        {
            await TryGetResponse(URL_API_Color, JsonUtility.ToJson(new Color(roomInfo.RoomID, newColor.ToString().ToLower())), dispose: true);
            roomInfo.PlayerColor = newColor;

            BingoConfig.config_bingosync.SetConfigValue<PlayerColors>(BingoConfig.config_selection_bingosync, BingoConfig.bingoSyncEntry_color, roomInfo.PlayerColor);
        }

        [Serializable]
        public class Room
        {
            public string room;
            public string nickname;
            public string password;

            public Room(string room, string nickname, string password)
            {
                this.room       = room;
                this.nickname   = nickname;
                this.password   = password;
            }
        }

        [Serializable]
        public class Select
        {
            public string   room;
            public string   slot;
            public string   color;
            public bool     remove_color;

            public Select(string room, string slot, string color, bool remove_color)
            {
                this.room           = room;
                this.slot           = slot;
                this.color          = color;
                this.remove_color   = remove_color;
            }
        }

        [Serializable]
        public class Color
        {
            public string   room;
            public string   color;

            public Color(string room, string color)
            {
                this.room   = room;
                this.color  = color;
            }
        }

        [Serializable]
        public class Slot
        {
            public string name;
            public string slot;
            public string colors;

            public Slot(string name, string slot, string colors)
            {
                this.name   = name;
                this.slot   = slot;
                this.colors = colors;
            }
        }
    }
}
