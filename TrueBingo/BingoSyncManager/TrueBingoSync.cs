using BingoSyncAPI;
using Reptile.Phone;
using Reptile;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;
using static BingoSyncAPI.BingoSyncTypes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;

namespace TrueBingo.BingoSyncManager
{
    internal static class TrueBingoSync
    {
        public static readonly BingoSync bingoSync = new BingoSync();

        public static bool IsUpdatingColor = false;

        private static bool _paused = false;

        private static List<BingoSync.MessageReceived> receivers = new List<BingoSync.MessageReceived>();

        public enum ObjectiveType
        {
            ItemPickup,
            CharacterUnlock,
            TaxiDriver,
            Rep
        }

        private static readonly Dictionary<Stage, string> StageToName = new Dictionary<Stage, string>()
        {
            { Stage.hideout,    "Hideout"   },
            { Stage.tower,      "Brink"     },
            { Stage.Mall,       "Mall"      },
            { Stage.osaka,      "Mataan"    },
            { Stage.square,     "Square"    },
            { Stage.downhill,   "Versum"    },
            { Stage.pyramid,    "Pyramid"   },
        };

        private static void OnMessage(SocketMessage message)
        {
            if (message.type == "goal" && BingoConfig.phonenotification)
            {
                SendNotification(message);
            }
            else if (message.type == "chat")
            {
                string messageText = message.text;

                if (messageText == null || messageText == string.Empty)
                    return;

                messageText = messageText.Trim();

                if (messageText.StartsWith("!"))
                {
                    messageText = messageText.Substring(1).Trim(' ');

                    string[] fullCommand = new string[1] { messageText };

                    if (fullCommand[0].Contains(' '))
                        fullCommand = messageText.Split(' ');

                    switch (fullCommand[0].ToLower())
                    {
                        case "start":
                            StartCountdown();
                        break;

                        case "ping":
                            SendMessage($"pong", true);
                        break;

                        case "pause":
                            Core.Instance?.PauseCore(PauseType.Debug);
                            _paused = true;
                            BingoSyncGUI.Pause = true;
                        break;

                        case "resume":
                            StartCountdown(true);
                        break;

                        case "help":
                            if (message.player.name.ToLower().Trim() == bingoSync.CurrentRoomInfo.PlayerName.ToLower().Trim())
                                SendMessage("[!start] Start Countdown, [!pause] Pause all players, [!resume] Resume all players, [!ping] Test players connection");
                        break;
                    }
                }
            }
        }

        public static void Update()
        {
            if (_paused && !Core.Instance.IsCorePaused)
                Core.Instance.PauseCore(PauseType.Debug);
        }

        private static async void StartCountdown(bool resume = false)
        {
            if (_paused && !resume)
                return;

            if (resume)
                while (BingoSyncGUI.Countdown) await Task.Yield();

            if (!BingoSyncGUI.Countdown)
            {
                BingoSyncGUI.countdownMessage   = "";
                BingoSyncGUI.Countdown          = true;

                BingoSyncGUI.Pause = false;

                int countdown = 5;

                AudioManager    audioManager    = Core.Instance?.AudioManager;
                MethodInfo      PlaySfxGameplay = null;

                if (audioManager != null)
                {
                    PlaySfxGameplay =
                        audioManager.GetType()
                        .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                        .FirstOrDefault
                        (
                            x => x.Name == "PlaySfxGameplay" && x.GetParameters().Length == 3 && x.GetParameters()
                            .All(y => y.ParameterType == typeof(SfxCollectionID) || y.ParameterType == typeof(AudioClipID) || y.ParameterType == typeof(float))
                        );
                }

                for (int i = countdown; i >= 0; i--)
                {
                    if (i != 0)
                    {
                        PlaySfxGameplay?.Invoke(audioManager, new object[] { SfxCollectionID.CombatSfx, AudioClipID.MineBeep, 0f });
                        BingoSyncGUI.countdownMessage = i.ToString();
                    }
                    else
                    {
                        PlaySfxGameplay?.Invoke(audioManager, new object[] { SfxCollectionID.EnvironmentSfx, AudioClipID.MascotHit, 0f });
                        BingoSyncGUI.countdownMessage = "GO!";

                        if (resume)
                        {
                            _paused = false;
                            Core.Instance?.UnPauseCore(PauseType.Debug);
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }

                BingoSyncGUI.Countdown = false;
            }
        }

        public static async void JoinRoom(RoomInfo roomInfo)
        {
            if (await bingoSync.JoinRoom(roomInfo) == BingoSync.ConnectionStatus.Connected)
            {
                RemoveReceivers();
                AddReceiver(OnMessage);
            }
            else
            {
                BingoSyncGUI.errorMessage = "Failed Connection: Check Room ID / Password";
            }
        }

        public static async void Disconnect()
        {
            if (bingoSync.Status == BingoSync.ConnectionStatus.Connected)
            {
                await bingoSync.Disconnect();
                RemoveReceivers();
            }
        }

        private static void RemoveReceivers()
        {
            for (int i = 0; i < receivers.Count; i++)
            {
                BingoSync.MessageReceived receiver = receivers[i];
                bingoSync.OnMessageReceived -= receiver;
            }
        }

        private static void AddReceiver(BingoSync.MessageReceived messageReceiver)
        {
            bingoSync.OnMessageReceived += messageReceiver;
            receivers.Add(messageReceiver);
        }

        public static async void SendMessage(string message, bool networkReply = false)
        {
            if (bingoSync.Status == BingoSync.ConnectionStatus.Connected)
            {
                if (networkReply)
                {
                    float networkTime = Time.time;
                    await bingoSync.SendChatMessage("pong");
                    message = $"{Time.time - networkTime}";
                }

                await bingoSync.SendChatMessage(message);
            }
        }

        public static async void MarkObjective(string itemToSend, ObjectiveType objectiveType, Pickup.PickUpType? pickupType = null)
        {
            if (bingoSync.Status != BingoSync.ConnectionStatus.Connected || itemToSend == string.Empty)
                return;

            SlotInfo    itemSlot    = null;
            SlotInfo[]  itemSlots   = await bingoSync.GetBoardSlots();

            if (itemSlots == null || itemSlots.Length == 0)
                return;

            string stage = string.Empty;

            if (!StageToName.TryGetValue(Utility.GetCurrentStage(), out stage))
                return;

            switch (objectiveType)
            {
                case ObjectiveType.ItemPickup:
                    if (pickupType != Pickup.PickUpType.OUTFIT_UNLOCKABLE)
                    {
                        SlotInfo[] possibleSlots = itemSlots.Where(x => x.Info.ToLower().StartsWith(stage.ToLower()) && !x.Info.Contains(':') && x.Info.Contains(" - ")).ToArray();
                        itemSlot = possibleSlots.FirstOrDefault(x => x.Info != null && CompareItems(itemToSend, GetItemFromSlotName(x.Info)));
                    }
                    else
                    {
                        itemSlot = itemSlots.FirstOrDefault(x => x.Info != null && x.Info.Contains(':') && GetItemFromSlotName(x.Info).ToLower().Contains(itemToSend.ToLower()));
                    }
                break;

                case ObjectiveType.CharacterUnlock:
                    itemToSend = itemToSend.ToLower().Trim();

                    if (itemToSend.Contains(' '))
                        itemToSend = itemToSend.Split(' ')[0];

                    itemSlot = itemSlots.FirstOrDefault(x => x.Info.ToLower().Contains($"({stage.ToLower()})") && x.Info.ToLower().Contains($"unlock {itemToSend}"));
                break;

                case ObjectiveType.TaxiDriver:
                    itemSlot = itemSlots.FirstOrDefault(x => x.Info.ToLower().Contains($"({stage.ToLower()})") && x.Info.ToLower().Contains(itemToSend.ToLower().Trim()));
                break;

                case ObjectiveType.Rep:
                    itemToSend = itemToSend.ToLower().Trim();

                    if (itemToSend.Contains(' '))
                        itemToSend = itemToSend.Split(' ')[1];

                    itemSlot = itemSlots.FirstOrDefault(x => x.Info.ToLower().Contains(" rep ") && x.Info.ToLower().Contains(itemToSend));
                break;
            }

            if (itemSlot != null)
                await bingoSync.SelectSlot(itemSlot.ID);
        }
        
        public static async void SetPlayerColor(PlayerColors playerColor)
        {
            IsUpdatingColor = true;
            await bingoSync.SetPlayerColor(playerColor);
            IsUpdatingColor = false;
        }

        private static string GetItemFromSlotName(string name)
        {
            Match   regexMatch = Regex.Match(name, @"- (.+?) -");
            return  regexMatch.Success ? regexMatch.Groups[1].Value : string.Empty;
        }

        private static bool CompareItems(string pickup, string itemOnBoard)
        {
            pickup      = pickup        .ToLower();
            itemOnBoard = itemOnBoard   .ToLower();

            if (pickup.Replace(" ", string.Empty).Trim().Equals(itemOnBoard.Replace(" ", string.Empty).Trim()))
                return true;

            int total           = 0;
            int searchLength    = 2;
            int bonus           = 3;

            for (int i = 0; i < itemOnBoard.Length; i++)
            {
                if (i + (searchLength - 1) <= itemOnBoard.Length - 1)
                {
                    string search = itemOnBoard.Substring(i, searchLength);

                    if (pickup.Contains(search))
                        total += bonus;
                }
                else
                {
                    total--;
                }
            }

            int correction = Mathf.Abs(pickup.Length - itemOnBoard.Length);

            total -= correction;

            int expectedAmount = Mathf.FloorToInt(((itemOnBoard.Length * (bonus * 0.5f)) - correction));

            return total >= expectedAmount;
        }

        public static void SendNotification(SocketMessage response)
        {
            string colors       = response.square?.colors;
            string player       = response.player?.name;
            string collection   = response.square?.name;

            if (colors != null && player != null && collection != null && !response.remove)
            {
                string itemToSend = collection;

                string stage = StageToName.Values.ToArray().FirstOrDefault(x => collection.Trim().StartsWith(x) || collection.Trim().EndsWith($"({x})") || (collection.ToLower().Contains(" rep ") && collection.Contains(x)));

                if (collection.Contains(" - "))
                    itemToSend = GetItemFromSlotName(collection);
                else if (Regex.Match(collection, @"\((.+?)\)").Success)
                    itemToSend = collection.Substring(0, collection.IndexOf('(') - 1).Trim();
                else if (collection.ToLower().Contains(" rep "))
                    itemToSend = collection.Substring(collection.ToLower().IndexOf("rep "));

                if (player != string.Empty && itemToSend != string.Empty && player.ToLower().Replace(" ", "").Trim() != bingoSync.CurrentRoomInfo.PlayerName.ToLower().Replace(" ", "").Trim())
                {
                    string notification = $"{player}: \"{itemToSend}\"";

                    if (stage != null && stage != string.Empty)
                        notification = $"{notification} ({stage})";

                    WaitThenSendNotification(notification);
                }
            }
        }

        private static AppEmail EmailApp => WorldHandler.instance?.GetCurrentPlayer()?.GetValue<Phone>("phone")?.GetAppInstance<AppEmail>();

        private static async void WaitThenSendNotification(string notification)
        {
            BaseModule  baseModule  = Core.Instance?.BaseModule;
            AppEmail    emailApp    = null;

            if (baseModule == null)
                return;

            if (baseModule.IsLoading)
            {
                emailApp = await GetEmailApp(Time.time + (float)TimeSpan.FromSeconds(10).TotalSeconds);

                if (emailApp != null)
                    emailApp.PushNotification(notification, null);
            }
            else
            {
                EmailApp?.PushNotification(notification, null);
            }
        }

        private static async Task<AppEmail> GetEmailApp(float endAfter)
        {
            AppEmail appEmail = null;

            while (appEmail == null)
            {
                if (Time.time > endAfter)
                    break;

                appEmail = EmailApp;

                if (appEmail == null)
                    await Task.Yield();
            }

            return appEmail;
        }
    }
}
