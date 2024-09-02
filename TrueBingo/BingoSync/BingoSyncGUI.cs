using BepInEx.Configuration;
using Reptile;
using System;
using System.Collections.Generic;
using UnityEngine;
using static TrueBingo.BingoSync.BingoSyncGUIConfig;

namespace TrueBingo.BingoSync
{
    internal class BingoSyncGUI : MonoBehaviour
    {
        public static bool GUIOpen = false;

        private Dictionary<string, string> textFields = new Dictionary<string, string>();

        private float windowX => (Screen.width * 0.5f) - (windowW * 0.5f);
        private const float windowY = 60;
        private const float windowW = 500;
        private const float windowH = 200;

        private float elementY => 20 + (elementH * (index - 1)) + (elementPadding * index);
        public static float elementW => windowW - (elementX * 2);
        private const float elementX = 20;
        private const float elementH = 20;

        private const float elementPadding = 4;

        public const float labelSpace = 110f;

        private Rect elementRect => new Rect(elementX, elementY, elementW, elementH);

        private int index = 0;

        private const string windowName = "BingoSync";

        public static string errorMessage = string.Empty;

        private void Awake()
        {
            UpdateBingoSyncConfig(BingoConfig.bingoSyncEntry_roomID,    ref RoomID);
            UpdateBingoSyncConfig(BingoConfig.bingoSyncEntry_password,  ref Password);
            UpdateBingoSyncConfig(BingoConfig.bingoSyncEntry_name,      ref PlayerName);
            UpdateBingoSyncConfig(BingoConfig.bingoSyncEntry_color,     ref PlayerColor);

            if (BingoConfig.autoconnect && RoomID != string.Empty && PlayerName != string.Empty)
                BingoSyncHandler.ConnectToRoom(RoomID, Password, PlayerName, PlayerColor);
        }

        private void UpdateBingoSyncConfig<T>(string key, ref T reference)
        {
            if (BingoConfig.config_bingosync.TryGetEntry(BingoConfig.config_selection_bingosync, key, out ConfigEntry<T> configEntry))
                reference = configEntry.Value;
        }

        private void OnGUI()
        {
            if (GUIOpen)
            {
                SetupStyles();
                GUIWindow(0, windowName, new Color(0, 0, 0, 1f));
            }
        }

        private readonly int possibleColors = Enum.GetValues(typeof(BingoSyncHandler.PlayerColors)).Length;

        private Color PlayerColorToColor(BingoSyncHandler.PlayerColors playerColor)
        {
            switch (playerColor)
            {
                case BingoSyncHandler.PlayerColors.Orange:  return Colors.Orange;
                case BingoSyncHandler.PlayerColors.Red:     return Colors.Red;
                case BingoSyncHandler.PlayerColors.Blue:    return Colors.Blue;
                case BingoSyncHandler.PlayerColors.Green:   return Colors.Green;
                case BingoSyncHandler.PlayerColors.Purple:  return Colors.Purple;
                case BingoSyncHandler.PlayerColors.Navy:    return Colors.Navy;
                case BingoSyncHandler.PlayerColors.Teal:    return Colors.Teal;
                case BingoSyncHandler.PlayerColors.Brown:   return Colors.Brown;
                case BingoSyncHandler.PlayerColors.Pink:    return Colors.Pink;
                case BingoSyncHandler.PlayerColors.Yellow:  return Colors.Yellow;
            }
            return Color.black;
        }

        private BingoSyncHandler.PlayerColors PlayerColor = BingoSyncHandler.PlayerColors.Red;
        private string RoomID       = string.Empty;
        private string Password     = string.Empty;
        private string PlayerName   = string.Empty;

        private string ConnectionColor      => BingoSyncHandler.ConnectedToRoom ? "lime" : BingoSyncHandler.ConnectingToRoom ? "orange" : "red";
        private string ConnectColor         => BingoSyncHandler.ConnectedToRoom ? "grey" : "white";
        private string DisconnectionColor   => BingoSyncHandler.ConnectedToRoom && !BingoSyncHandler.DisconnectingFromRoom ? "red" : "grey";

        private void HandleGUI(int windowID)
        {
            index = 0;

            if (GUIButton($"Color: {PlayerColor.ToString()}", PlayerColorToColor(PlayerColor)) && !BingoSyncHandler.SettingColor && !BingoSyncHandler.ConnectingToRoom)
            {
                PlayerColor = (int)PlayerColor >= possibleColors - 1 ? BingoSyncHandler.PlayerColors.Orange : (BingoSyncHandler.PlayerColors)((int)PlayerColor + 1);
                BingoSyncHandler.UpdatePlayerColor(PlayerColor);
            }

            RoomID      = GUITextField("RoomID",    RoomID,     Color.white, Color.white, "Room ID:",       true);
            Password    = GUITextField("Password",  Password,   Color.white, Color.white, "Password:",      true);
            PlayerName  = GUITextField("Name",      PlayerName, Color.white, Color.white, "Player Name:");

            GUILabel(errorMessage, Color.red);

            if (GUIButton($"<color={ConnectColor}>Connect</color> <color={ConnectionColor}>({BingoSyncHandler.ConnectionStatus})</color>", Color.gray) && !BingoSyncHandler.ConnectedToRoom && !BingoSyncHandler.ConnectingToRoom)
                BingoSyncHandler.ConnectToRoom(RoomID, Password, PlayerName, PlayerColor);

            if (GUIButton($"<color={DisconnectionColor}>Disconnect</color>", Color.gray) && BingoSyncHandler.ConnectedToRoom)
                BingoSyncHandler.Disconnect();
        }

        private Rect GUIWindow(int ID, string name, Color color)
        {
            GUI.backgroundColor = color;

            index++;
            return GUI.Window(ID, new Rect(windowX, windowY, windowW, windowH), HandleGUI, windowName);
        }

        private void GUILabel(string text, Color color, bool updateIndex = true, Rect customRect = default)
        {
            if (updateIndex)
                index++;

            GUI.contentColor = color;
            GUI.Label(updateIndex ? elementRect : customRect, text, GetLabelStyle(!updateIndex));
        }

        private bool GUIButton(string text, Color color)
        {
            index++;

            Color newColor = new Color(color.r / 255, color.g / 255, color.b / 255, 0.7f);

            Textures.Button_Background.SetPixel(0, 0, newColor);
            Textures.Button_Background.Apply();

            Textures.Button_Hover.SetPixel(0, 0, newColor * 1.1f);
            Textures.Button_Hover.Apply();

            Textures.Button_Click.SetPixel(0, 0, newColor * 0.8f);
            Textures.Button_Click.Apply();

            return GUI.Button(elementRect, text, Styles.Button);
        }

        private string GUITextField(string ID, string initialText, Color textColor, Color outlineColor, string label = null, bool password = false)
        {
            index++;

            string text = initialText;

            if (!textFields.TryGetValue(ID, out text))
                textFields.Add(ID, text);

            if (text == null)
                text = initialText;

            GUI.contentColor    = textColor;
            GUI.backgroundColor = outlineColor;

            Rect rectToUse = elementRect;

            if (label != null && label != string.Empty)
            {
                rectToUse = new Rect(elementRect.x + labelSpace, elementRect.y, elementRect.width - labelSpace, elementRect.height);
                GUILabel(label, Color.white, false, new Rect(elementRect.x, elementRect.y, elementRect.width - rectToUse.width, elementRect.height));
            }

            string value = string.Empty;

            if (password)
                value = GUI.PasswordField(rectToUse, text, "*"[0], 64);
            else
                value = GUI.TextField(rectToUse, text);

            return textFields[ID] = value;
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(BingoConfig.menukey))
            {
                GUIOpen = !GUIOpen;

                Core        core        = Core.Instance;
                GameInput   gameInput   = core?.GameInput;

                if (GUIOpen)
                {
                    gameInput?.DisableMouse();
                }
                else
                {
                    core?.BaseModule?.RestoreMouseInputState();
                    gameInput?.EnableMouse();
                }
            }

            if (GUIOpen)
                Cursor.visible = true;
        }
    }
}
