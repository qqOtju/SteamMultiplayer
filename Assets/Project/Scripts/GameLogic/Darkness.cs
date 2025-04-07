using Mirror;
using UnityEngine;

namespace Project.Scripts.GameLogic
{
    public class Darkness: NetworkBehaviour
    {
        private Collider2D _collider;

        public override void OnStartServer()
        {
            Debug.Log("Darkness started");//
        }

        [Server]
        private void OnTriggerEnter2D(Collider2D other)
        {
            if(!isServer) return;
            if (!other.CompareTag("Player")) return;
            _collider = other;
            Attack();
        }

        [Server]
        private void Attack()
        {
            if (_collider == null || !_collider.TryGetComponent(out IHealth health)) 
                return;
            health.TakeDamage(health.MaxHealth);
        }
    }
}