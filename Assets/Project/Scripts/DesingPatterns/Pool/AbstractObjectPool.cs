using UnityEngine;

namespace Project.Scripts.DesingPatterns.Pool
{
    public abstract class AbstractObjectPool<T> where T: class
    {
        protected readonly Transform Container;
        protected readonly T Obj;
        
        private ObjectPool<T> ObjPool { get; }
        
        protected AbstractObjectPool(T obj, Transform container)
        {
            this.Obj = obj;
            this.Container = container;
            ObjPool = new ObjectPool<T>(CreateFunc, ActionOnGet, ActionOnRelease);
        }

        public T Get() => ObjPool.Get();
        
        public void Release(T obj) => ObjPool.Release(obj);
        
        public void Initialize(int size) => ObjPool.InitializePool(size);
        
        protected abstract T CreateFunc();
        protected abstract void ActionOnGet(T obj);
        protected abstract void ActionOnRelease(T obj);
    }
}