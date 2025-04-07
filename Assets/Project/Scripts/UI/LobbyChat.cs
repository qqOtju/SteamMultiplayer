using System;
using System.Collections.Generic;
using Mirror;
using Project.Scripts.GameLogic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.UI
{
    public class LobbyChat: NetworkBehaviour
    {
        [SerializeField] private TMP_Text _chatHistory;
        [SerializeField] private Scrollbar _scrollbar;
        [SerializeField] private TMP_InputField _chatMessage;
        [SerializeField] private Button _sendButton;

        private readonly Dictionary<NetworkConnectionToClient, ChatPlayer> _connNames = new ();
        
        private void Awake()
        {
            _sendButton.onClick.AddListener(SendMessage);
            _chatMessage.onValueChanged.AddListener(ToggleButton);
            _chatMessage.onEndEdit.AddListener(OnEndEdit);
        }

        private void OnDestroy()
        {
            _sendButton.onClick.RemoveListener(SendMessage);
            _chatMessage.onValueChanged.RemoveListener(ToggleButton);
            _chatMessage.onEndEdit.RemoveListener(OnEndEdit);
        }

        [Command(requiresAuthority = false)]
        private void CmdSend(string message, NetworkConnectionToClient sender = null)
        {
            if (sender == null)
            {
                RpcReceive("Server", message.Trim(), Color.red);
                return;
            }
            if (!_connNames.ContainsKey(sender))
            {
                var player = sender.identity.GetComponent<Player>();
                var chatPlayer = new ChatPlayer(player.PlayerName, player.PlayerColor);
                _connNames.Add(sender, chatPlayer);
            }
            if (string.IsNullOrWhiteSpace(message)) return;
            {
                var chatPlayer = _connNames[sender];
                RpcReceive(chatPlayer.PlayerName, message.Trim(), chatPlayer.PlayerColor);
            }
        }
        
        [ClientRpc]
        public void RpcReceive(string playerName, string message, Color color)
        {
            var prettyMessage = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{playerName}:</color> {message}";
            AppendMessage(prettyMessage);
        }

        private void AppendMessage(string message)
        {
            var time = $"[{DateTime.Now:HH:mm}] ";
            _chatHistory.text += time + message + "\n";
            Canvas.ForceUpdateCanvases();
            _scrollbar.value = 0;
        }

        private void ToggleButton(string input)
        {
            _sendButton.interactable = !string.IsNullOrWhiteSpace(input);
        }

        private void OnEndEdit(string input)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetButtonDown("Submit"))
                SendMessage();
        }

        private void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(_chatMessage.text)) return;
            CmdSend(_chatMessage.text.Trim());
            _chatMessage.text = string.Empty;
            _chatMessage.ActivateInputField();
        }
    }

    public struct ChatPlayer
    {
        public string PlayerName;
        public Color PlayerColor;

        public ChatPlayer(string playerName, Color playerColor)
        {
            PlayerName = playerName;
            PlayerColor = playerColor;
        }
    }
}