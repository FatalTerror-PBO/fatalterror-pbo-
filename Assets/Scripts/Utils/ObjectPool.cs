using System.Collections.Generic;
using UnityEngine;

namespace HorrorGame.Utils
{
    /// <summary>
    /// GENERIC CLASS + COLLECTION (Queue): object pool untuk komponen apa pun.
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Queue<T> available = new Queue<T>();

        public ObjectPool(T prefab, int prewarm, Transform parent = null)
        {
            this.prefab = prefab;
            this.parent = parent;

            for (int i = 0; i < prewarm; i++)
            {
                T item = Create();
                item.gameObject.SetActive(false);
                available.Enqueue(item);
            }
        }

        public T Get()
        {
            T item = available.Count > 0 ? available.Dequeue() : Create();
            item.gameObject.SetActive(true);
            return item;
        }

        public void Release(T item)
        {
            item.gameObject.SetActive(false);
            available.Enqueue(item);
        }

        private T Create()
        {
            return Object.Instantiate(prefab, parent);
        }
    }
}
