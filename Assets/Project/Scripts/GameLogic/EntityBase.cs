using System;
using Mirror;
using UnityEngine;

namespace Project.Scripts.GameLogic
{
    public abstract class EntityBase: NetworkBehaviour, IHealth
    {
        protected const int DefaultMaxHealth = 3;
        
        [SyncVar(hook = nameof(OnMaxHealthChanged))]
        private int _maxHealth;
        [SyncVar(hook = nameof(OnCurrentHealthChanged))]
        private float _currentHealth;

        public GameObject GO => gameObject;
        public OnHealthChangeArgs LastHealthChangeArgs { get; private set; }
        public int MaxHealth => _maxHealth;
        public float CurrentHealth => _currentHealth;

        public event Action<OnHealthChangeArgs> OnHealthChange;

        //Only on Client
        private void OnMaxHealthChanged(int oldValue, int newValue)
        {
            Debug.Log($"Max health changed from {oldValue} to {newValue}");
        }
        
        private void OnCurrentHealthChanged(float oldValue, float newValue)
        {
            Debug.Log($"Current health changed from {oldValue} to {newValue}");
            OnHealthChange?.Invoke(CreateHealthChangeArgs(oldValue, newValue));
        }
        
        private OnHealthChangeArgs CreateHealthChangeArgs(float previousHealth, float currentHealth)
        {
            var type = HeathChangeType.Damage;
            if (currentHealth > previousHealth)
                type = HeathChangeType.Heal;
            else if (currentHealth <= 0)
                type = HeathChangeType.Death;
            var args = new OnHealthChangeArgs
            {
                Object = gameObject,
                CurrentHealth = currentHealth,
                PreviousHealth = previousHealth,
                Type = type
            };
            LastHealthChangeArgs = args;
            return args;
        }

        [Server]
        public virtual void TakeDamage(float dmg)
        {
            var health = Mathf.Clamp(_currentHealth - dmg, 0, _maxHealth);
            if (health == _currentHealth) return;
            _currentHealth = health;
        }
        
        [Server]
        public virtual void Heal(float heal)
        {
            var health = Mathf.Clamp(_currentHealth + heal, 0, _maxHealth);
            if (health == _currentHealth) return;
            _currentHealth = health;
        }
        
        [Server]
        public virtual void IncreaseMaxHealth(int value)
        {
            if (_maxHealth + value <= 0)
                _maxHealth = 10;
            else
                _maxHealth += value;

            var relative = _currentHealth / (_maxHealth - value); // попередній % здоров'я
            _currentHealth = _maxHealth * relative;
        }
        

        [Server]
        protected void SetInitialHealth(int maxHealth)
        {
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
        }
    }
}