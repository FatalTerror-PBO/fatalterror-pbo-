using System.Collections.Generic;
using UnityEngine;
using HorrorGame.Audio;
using HorrorGame.Core;
using HorrorGame.Utils;

namespace HorrorGame.Effects
{
    /// <summary>Memunculkan efek seram secara acak memakai ObjectPool generik + List titik spawn.</summary>
    public class ScareManager : MonoBehaviour
    {
        [SerializeField] private ScareEffect scarePrefab;
        [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
        [SerializeField] private float minInterval = 20f;
        [SerializeField] private float maxInterval = 45f;
        [SerializeField] private float effectLifetime = 1.5f;

        private ObjectPool<ScareEffect> pool;
        private float timer;

        private void Awake()
        {
            if (scarePrefab != null)
                pool = new ObjectPool<ScareEffect>(scarePrefab, 3, transform);

            ResetTimer();
        }

        private void Update()
        {
            if (pool == null || spawnPoints.Count == 0 || !GameManager.IsPlaying) return;

            timer -= Time.deltaTime;
            if (timer > 0f) return;

            Transform point = spawnPoints[Random.Range(0, spawnPoints.Count)];
            ScareEffect effect = pool.Get();
            effect.transform.position = point.position;
            effect.Init(pool, effectLifetime);

            if (AudioManager.Instance != null) AudioManager.Instance.PlayAt("scare", point.position);

            ResetTimer();
        }

        private void ResetTimer()
        {
            timer = Random.Range(minInterval, maxInterval);
        }
    }
}
