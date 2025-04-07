using System;
using System.Collections.Generic;

namespace Project.Scripts.DesingPatterns.Pool
{
    public class ObjectPool<T> where T : class
    {
        private readonly Stack<T> _stack;
        private readonly Func<T> _createFunc;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly Action<T> _onDestroy;
        private readonly int _maxSize;
        private readonly bool _collectionCheck;

        public int CountAll { get; private set; }
        public int CountInactive => _stack.Count;
        public int CountActive => CountAll - CountInactive;

        public ObjectPool(Func<T> createFunc,
            Action<T> actionOnGet = null,
            Action<T> actionOnRelease = null,
            Action<T> actionOnDestroy = null,
            bool collectionCheck = true,
            int defaultCapacity = 10,
            int maxSize = 10000)
        {
            if(createFunc == null)
                throw new ArgumentNullException(nameof(createFunc));
            if(maxSize <= 0)
                throw new ArgumentException("Max Size must be greater than 0", nameof(maxSize));
            _stack = new Stack<T>(defaultCapacity);
            _createFunc = createFunc;
            _maxSize = maxSize;
            _onGet = actionOnGet;
            _onRelease = actionOnRelease;
            _onDestroy = actionOnDestroy;
            _collectionCheck = collectionCheck;
        }
        
        
        public void InitializePool(int size)
        {
            for (var i = 0; i < size; i++)
            {
                var obj = _createFunc();
                ++CountAll;
                Release(obj);
            }
        }
        
        public T Get()
        {
            T obj;
            if (_stack.Count == 0)
            {
                obj = _createFunc();
                ++CountAll;
            }
            else
                obj = _stack.Pop();
            _onGet?.Invoke(obj);
            return obj;
        }

        public void Release(T element)
        {
            if (_collectionCheck && _stack.Count > 0 && _stack.Contains(element))
                throw new InvalidOperationException($"Trying to release an object that has already been released to the pool. {element}");
            _onRelease?.Invoke(element);
            if (CountInactive < _maxSize)
                _stack.Push(element);
            else
                _onDestroy?.Invoke(element);
        }

        public void Clear()
        {
            if (_onDestroy != null)
                foreach (T obj in _stack)
                    _onDestroy(obj);
            _stack.Clear();
            CountAll = 0;
        }

        public void Dispose() => Clear();
    }
}