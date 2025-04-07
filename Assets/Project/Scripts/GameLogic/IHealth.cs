using System;
using UnityEngine;

namespace Project.Scripts.GameLogic
{
    public interface IHealth
    {
        public int MaxHealth { get; }
        public float CurrentHealth { get; }
        public GameObject GO { get; }
        public OnHealthChangeArgs LastHealthChangeArgs { get; }
        
        public event Action<OnHealthChangeArgs> OnHealthChange; 

        public void TakeDamage(float dmg);
        public void Heal(float heal);
        public void IncreaseMaxHealth(int value);
    }
    
    public struct OnHealthChangeArgs
    {
        public float PreviousHealth;
        public float CurrentHealth;
        public HeathChangeType Type;
        public GameObject Object;
    }
    
    public enum HeathChangeType
    {
        Damage,
        Death,
        Heal
    }
}