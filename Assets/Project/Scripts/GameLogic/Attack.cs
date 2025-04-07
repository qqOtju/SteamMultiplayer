using UnityEngine;

namespace Project.Scripts.GameLogic
{
    public class Attack: MonoBehaviour
    {
        [SerializeField] private Animator _attackAnimator;
        [SerializeField] private ParticleSystem _attackEffect;
        
        private static readonly int AttackAnimation = Animator.StringToHash("Attack");

        public void PlayAnimation()
        {
            _attackAnimator.SetTrigger(AttackAnimation);
            _attackEffect.Play();
        }
    }
}