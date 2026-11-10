using UnityEngine;
using HorrorGame.Core;
using HorrorGame.Interfaces;
using HorrorGame.Audio;

namespace HorrorGame.Player
{
    /// <summary>Mengimplementasikan INTERFACE IDamageable. Memakai property get/set.</summary>
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 100;

        private int currentHealth;

        public int MaxHealth => maxHealth;

        public int CurrentHealth
        {
            get => currentHealth;
            private set
            {
                currentHealth = Mathf.Clamp(value, 0, maxHealth);
                GameEvents.HealthChanged.Raise((float)currentHealth / maxHealth);
            }
        }

        public bool IsDead => currentHealth <= 0;

        private void Start()
        {
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (IsDead) return;

            CurrentHealth -= amount;

            if (AudioManager.Instance != null) AudioManager.Instance.Play("hurt");

            if (IsDead && GameManager.Instance != null)
                GameManager.Instance.LoseGame();
        }

        /// <summary>Mengembalikan false jika nyawa sudah penuh.</summary>
        public bool Heal(int amount)
        {
            if (IsDead || CurrentHealth >= maxHealth) return false;

            CurrentHealth += amount;
            return true;
        }
    }
}
