using UnityEngine;
using HorrorGame.Utils;

namespace HorrorGame.Effects
{
    /// <summary>Objek jump-scare sementara yang dikembalikan ke ObjectPool setelah waktunya habis.</summary>
    public class ScareEffect : MonoBehaviour
    {
        private ObjectPool<ScareEffect> pool;
        private float timer;

        public void Init(ObjectPool<ScareEffect> ownerPool, float lifetime)
        {
            pool = ownerPool;
            timer = lifetime;
        }

        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f && pool != null) pool.Release(this);
        }
    }
}
