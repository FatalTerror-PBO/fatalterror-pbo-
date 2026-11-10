using UnityEngine;

namespace HorrorGame.Effects
{
    /// <summary>Membuat lampu berkedip acak (suasana horor).</summary>
    [RequireComponent(typeof(Light))]
    public class LightFlicker : MonoBehaviour
    {
        [SerializeField] private float minFactor = 0.15f;
        [SerializeField] private float speed = 6f;
        [SerializeField] private float blackoutChance = 0.02f;

        private Light lamp;
        private float baseIntensity;
        private float seed;
        private float blackoutTimer;

        private void Awake()
        {
            lamp = GetComponent<Light>();
            baseIntensity = lamp.intensity;
            seed = Random.value * 100f;
        }

        private void Update()
        {
            if (blackoutTimer > 0f)
            {
                blackoutTimer -= Time.deltaTime;
                lamp.intensity = 0f;
                return;
            }

            if (Random.value < blackoutChance * Time.deltaTime * 60f * 0.1f)
                blackoutTimer = Random.Range(0.05f, 0.35f);

            float noise = Mathf.PerlinNoise(Time.time * speed, seed);
            lamp.intensity = baseIntensity * Mathf.Lerp(minFactor, 1f, noise);
        }
    }
}
