using Mirror;
using Mirror.Steamworks.NET;
using Project.Scripts.GameLogic;
using Steamworks;
using UnityEngine;

namespace Project.Scripts.Network
{
    public class CustomNetworkManager: NetworkManager
    {
        [SerializeField] private MatchController _matchController;


        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("Server started");
        }
        
        public override void OnStopServer()
        {
            base.OnStopServer();
            Debug.Log("Server stopped");
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
            Debug.Log("Server connected");
        }
        
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            // LobbyChat.ConnNames.Remove(conn);
            base.OnServerDisconnect(conn);
            Debug.Log("Server disconnected");
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            Debug.Log("Client connected");
        }
        
        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            Debug.Log("Client disconnected");
        }
        
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            Debug.Log($"Trying to add player: {conn.connectionId}"); 
            var startPos = GetStartPosition();
            var player = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);
            player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
            NetworkServer.AddPlayerForConnection(conn, player);
            //new code
            var steamId = SteamMatchmaking.GetLobbyMemberByIndex(SteamLobby.LobbyID, numPlayers - 1);
            player.GetComponent<Player>().SetupPlayer(steamId);
            _matchController.RegisterPlayer(player.GetComponent<Player>());
            Debug.Log($"Added player: {conn.connectionId}");
        }
    }
}