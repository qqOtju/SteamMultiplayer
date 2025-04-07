using System.Collections;
using System.Collections.Generic;
using Mirror;
using Project.Scripts.GameLogic;
using TMPro;
using UnityEngine;

namespace Project.Scripts.UI
{
    public class UIGame: NetworkBehaviour
    {
        [SerializeField] private TMP_Text _score;
        [SerializeField] private TMP_Text _timer;
        [SerializeField] private Transform _playersContainer;
        [SerializeField] private UIPlayer _playerPrefab;//
        
        private readonly Dictionary<Player, UIPlayer> _uiPlayers = new();
        
        [ClientRpc]
        public void RpcUpdateScore(string score)
        {
            _score.text = score;
        }

        [ClientRpc]
        public void RpcStartTimer()
        {
            _timer.gameObject.SetActive(true);
            StartCoroutine(StartTimerCoroutine());
        }

        private IEnumerator StartTimerCoroutine()
        {
            _timer.text = "3";
            yield return new WaitForSeconds(1);
            _timer.text = "2";
            yield return new WaitForSeconds(1);
            _timer.text = "1";
            yield return new WaitForSeconds(1);
            _timer.text = "GO!";
            yield return new WaitForSeconds(1);
            _timer.gameObject.SetActive(false);
        }

        [ClientRpc]
        public void RpcUpdateHealth(Player player, int health)
        {
            if (!_uiPlayers.TryGetValue(player, out var uiPlayer)) return;
            uiPlayer.UpdatePlayerImage(player.PlayerAvatar);
            uiPlayer.UpdatePlayerName(player.PlayerName, player.PlayerColor);
            uiPlayer.UpdateHealth(health);
        }
        
        [ClientRpc]
        public void RpcUpdateDeathCount(Player player, int deathCount)
        {
            if (!_uiPlayers.TryGetValue(player, out var uiPlayer)) return;
            uiPlayer.UpdatePlayerImage(player.PlayerAvatar);
            uiPlayer.UpdatePlayerName(player.PlayerName, player.PlayerColor);
            uiPlayer.UpdateDeathCount(deathCount);
        }

        [ClientRpc]
        public void RpcUpdate(Player player, int deathCount, int health)
        {
            if (!_uiPlayers.TryGetValue(player, out var uiPlayer)) return;
            uiPlayer.UpdateHealth(health);
            uiPlayer.UpdatePlayerImage(player.PlayerAvatar);
            uiPlayer.UpdatePlayerName(player.PlayerName, player.PlayerColor);
            uiPlayer.UpdateDeathCount(deathCount);
            uiPlayer.UpdateHealth(health);
        }
        
        [ClientRpc]
        public void RpcAddPlayer(Player player, Texture2D playerImage, string playerName, 
            Color playerColor)
        {
            if(_uiPlayers.ContainsKey(player)) return;
            var ui = Instantiate(_playerPrefab, _playersContainer);
            _uiPlayers.Add(player, ui);
            ui.Initialize(playerImage, playerName, playerColor);
        }
    }
}