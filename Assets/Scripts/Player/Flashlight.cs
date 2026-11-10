using UnityEngine;
using HorrorGame.Core;

namespace HorrorGame.Player
{
    public class Flashlight : MonoBehaviour
    {
        [SerializeField] private Light lightSource;
        [SerializeField] private float maxBattery = 100f;
        [SerializeField] private float drainPerSecond = 2f;
        [SerializeField] private KeyCode toggleKey = KeyCode.F;

        private float battery;
        private float baseIntensity = 1f;

        public float Battery
        {
            get => battery;
            private set
            {
                battery = Mathf.Clamp(value, 0f, maxBattery);
                GameEvents.BatteryChanged.Raise(battery / maxBattery);
            }
        }

        public bool IsOn { get; private set; }

        private void Start()
        {
            if (lightSource != null) baseIntensity = lightSource.intensity;

            Battery = maxBattery;
            SetState(true);
        }

        private void Update()
        {
            if (!GameManager.IsPlaying) return;

            if (Input.GetKeyDown(toggleKey) && Battery > 0f)
                SetState(!IsOn);

            if (!IsOn) return;

            Battery -= drainPerSecond * Time.deltaTime;

            if (Battery <= 0f)
            {
                SetState(false);
                return;
            }

            // Efek berkedip saat baterai hampir habis
            if (Battery < maxBattery * 0.15f && lightSource != null)
                lightSource.intensity = baseIntensity * (Random.value > 0.1f ? 1f : 0.2f);
        }

        public void Recharge(float amount)
        {
            Battery += amount;
        }

        private void SetState(bool on)
        {
            IsOn = on;
            if (lightSource != null)
            {
                lightSource.enabled = on;
                lightSource.intensity = baseIntensity;
            }
        }
    }
}
