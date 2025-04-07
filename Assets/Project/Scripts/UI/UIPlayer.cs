using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.UI
{
    public class UIPlayer: MonoBehaviour
    {
        [SerializeField] private RawImage _playerImage;
        [SerializeField] private TMP_Text _playerName;
        [SerializeField] private Image[] _hearts;
        [SerializeField] private TMP_Text _deathCount;

        public void Initialize(Texture2D playerImage, string playerName, Color playerColor)
        {
            _playerImage.texture = playerImage;
            _playerName.text = playerName;
            _playerName.color = playerColor;
            foreach (var heart in _hearts)
            {
                heart.gameObject.SetActive(true);
            }
            _deathCount.text = "0";
        } 
        
        public void UpdateHealth(int currentHealth)
        {
            for (var i = 0; i < _hearts.Length; i++)
                _hearts[i].gameObject.SetActive(i < currentHealth);
        }

        public void UpdateDeathCount(int deathCount)
        {
            _deathCount.text = deathCount.ToString();
        }
        
        public void UpdatePlayerName(string playerName, Color color)
        {
            _playerName.text = playerName;
            _playerName.color = color;
        }
        
        public void UpdatePlayerImage(Texture2D playerImage)
        {
            _playerImage.texture = playerImage;
        }
    }
}