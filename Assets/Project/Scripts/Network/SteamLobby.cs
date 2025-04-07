using System;
using Mirror;
using Mirror.Steamworks.NET;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.Network
{
    public class SteamLobby : MonoBehaviour
    {
        [SerializeField] private GameObject _buttons;
        [SerializeField] private Button _hostButton; 
        
        private const string HostAddressKey = "HostAddress";

        protected Callback<LobbyCreated_t> LobbyCreated;
        protected Callback<GameLobbyJoinRequested_t> GameLobbyJoinRequested;
        protected Callback<LobbyEnter_t> LobbyEntered;

        private NetworkManager _networkManager;

        public static CSteamID LobbyID { get; private set; }
        
        private void Awake()
        {
            _hostButton.onClick.AddListener(HostLobby);
        }

        private void Start()
        {
            _networkManager = GetComponent<NetworkManager>();

            if (!SteamManager.Initialized) { return; }

            LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            GameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        }
        
        private void OnDestroy()
        {
            _hostButton.onClick.RemoveListener(HostLobby);
        }

        private void HostLobby()
        {
            _buttons.SetActive(false);

            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, _networkManager.maxConnections);
        }

        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult != EResult.k_EResultOK)
            {
                _buttons.SetActive(true);
                return;
            }
            LobbyID = new CSteamID(callback.m_ulSteamIDLobby);
            _networkManager.StartHost();

            SteamMatchmaking.SetLobbyData(
                LobbyID,
                HostAddressKey,
                SteamUser.GetSteamID().ToString());
        }

        private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
        {
            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }

        private void OnLobbyEntered(LobbyEnter_t callback)
        {
            if (NetworkServer.active) { return; }

            string hostAddress = SteamMatchmaking.GetLobbyData(
                new CSteamID(callback.m_ulSteamIDLobby),
                HostAddressKey);

            _networkManager.networkAddress = hostAddress;
            _networkManager.StartClient();

            _buttons.SetActive(false);
        }
    }
}