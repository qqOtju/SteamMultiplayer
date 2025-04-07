using System.Collections;
using System.Collections.Generic;
using Mirror;
using Project.Scripts.UI;
using UnityEngine;

namespace Project.Scripts.GameLogic
{
    public class MatchController: NetworkBehaviour
    {
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private LobbyChat _lobbyChat;
        [SerializeField] private UIGame _uiGame;
        
        private readonly List<Player> _players = new();
        private readonly Dictionary<Player, int> _deathCounts = new();
        private int _deathCount;
        private bool _restarting;

        public override void OnStartServer()
        {
            base.OnStartServer();
            _deathCount = 0;
            _players.Clear();
            _deathCounts.Clear();
        }
        
        public override void OnStopServer()
        {
            base.OnStopServer();
            _deathCount = 0;
            _players.Clear();
            _deathCounts.Clear();
        }

        [Server]
        public void RegisterPlayer(Player player)
        {
            Debug.Log($"Registering player {player.name}");
            _players.Add(player);
            _deathCounts.Add(player, 0);
            foreach (var pl in _players)
                _uiGame.RpcAddPlayer(pl,pl.PlayerAvatar, pl.PlayerName, pl.PlayerColor);
            player.OnHealthChange += _ => OnPlayerDeath(player);
        }

        [Server]
        private void OnPlayerDeath(Player player)
        {
            if(!isServer || _restarting) return;
            _uiGame.RpcUpdateHealth(player, (int)player.CurrentHealth);
            if(player.CurrentHealth > 0) return;
            Debug.Log($"Player {player.PlayerName} died!");
            _deathCounts[player]++;
            _lobbyChat.RpcReceive("SERVER", $"{player.PlayerName} died!", Color.red);
            _deathCount++;
            _uiGame.RpcUpdateDeathCount(player, _deathCounts[player]);
            if(_deathCount >= _players.Count - 1)
                StartCoroutine(StartNextRound());
        }

        [Server]
        private IEnumerator StartNextRound()
        {
            _restarting = true;
            Debug.Log("Starting next round...");
            var scoreText = "";
            foreach (var player in _players)
                scoreText += $"{_deathCounts[player]} - ";
            scoreText = scoreText.Remove(scoreText.Length - 2, 2);
            _uiGame.RpcUpdateScore(scoreText);
            _uiGame.RpcStartTimer();
            yield return new WaitForSeconds(3f);
            _lobbyChat.RpcReceive("SERVER", $"Next round!", Color.red);
            foreach (var player in _players)
            {
                player.Heal(player.MaxHealth);
                var spawnPoint = GetRandomSpawnPoint();
                _uiGame.RpcUpdate(player, _deathCounts[player], (int)player.CurrentHealth);
                player.GetComponent<NetworkTransformReliable>().RpcTeleport(spawnPoint, Quaternion.identity);
            }
            _deathCount = 0;
            _restarting = false;
        }

        private Vector3 GetRandomSpawnPoint()
        {
            return _spawnPoints[Random.Range(0, _spawnPoints.Length)].position;
        }
    }
}