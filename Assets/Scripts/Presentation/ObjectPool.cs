using System.Collections.Generic;
using UnityEngine;

namespace KidQuiz.Presentation
{
    public sealed class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _inactive = new();

        public ObjectPool(T prefab, Transform parent, int prewarmCount = 0)
        {
            _prefab = prefab;
            _parent = parent;

            for (int i = 0; i < prewarmCount; i++)
            {
                Release(CreateInstance());
            }
        }

        public T Get()
        {
            T instance = _inactive.Count > 0 ? _inactive.Pop() : CreateInstance();
            instance.gameObject.SetActive(true);
            return instance;
        }

        public void Release(T instance)
        {
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(_parent, false);
            _inactive.Push(instance);
        }

        private T CreateInstance()
        {
            return Object.Instantiate(_prefab, _parent);
        }
    }
}
