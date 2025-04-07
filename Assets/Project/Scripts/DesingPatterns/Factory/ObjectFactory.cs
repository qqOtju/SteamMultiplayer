using Project.Scripts.DesingPatterns.Pool;
using UnityEngine;

namespace Project.Scripts.DesingPatterns.Factory
{
    public abstract class ObjectFactory<T> where T: MonoBehaviour
    {
        protected readonly MonoBehaviourPool<T> Pool;

        protected ObjectFactory(T prefab, Transform container)
        {
            Pool = new MonoBehaviourPool<T>(prefab, container);
        }

        public abstract T Get();
        public abstract void Release(T obj);
    }
}