using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HorrorGame.Core;
using HorrorGame.Items;

namespace HorrorGame.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Bar")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider batteryBar;
        [SerializeField] private Slider staminaBar;

        [Header("Teks")]
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private TMP_Text inventoryText;
        [SerializeField] private float messageDuration = 4f;

        [Header("Panel")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject gameOverPanel;

        [Header("Referensi")]
        [SerializeField] private Inventory inventory;

        private Coroutine messageRoutine;

        private void OnEnable()
        {
            GameEvents.HealthChanged.Subscribe(OnHealthChanged);
            GameEvents.BatteryChanged.Subscribe(OnBatteryChanged);
            GameEvents.StaminaChanged.Subscribe(OnStaminaChanged);
            GameEvents.PromptChanged.Subscribe(OnPromptChanged);
            GameEvents.MessageShown.Subscribe(OnMessageShown);
            GameEvents.InventoryChanged.Subscribe(OnInventoryChanged);
            GameEvents.StateChanged.Subscribe(OnStateChanged);
        }

        private void OnDisable()
        {
            GameEvents.HealthChanged.Unsubscribe(OnHealthChanged);
            GameEvents.BatteryChanged.Unsubscribe(OnBatteryChanged);
            GameEvents.StaminaChanged.Unsubscribe(OnStaminaChanged);
            GameEvents.PromptChanged.Unsubscribe(OnPromptChanged);
            GameEvents.MessageShown.Unsubscribe(OnMessageShown);
            GameEvents.InventoryChanged.Unsubscribe(OnInventoryChanged);
            GameEvents.StateChanged.Unsubscribe(OnStateChanged);
        }

        private void Start()
        {
            if (promptText != null) promptText.text = string.Empty;
            if (messageText != null) messageText.text = string.Empty;
            OnInventoryChanged(ItemType.None);
            OnStateChanged(GameState.Playing);
        }

        private void OnHealthChanged(float value) { if (healthBar != null) healthBar.value = value; }
        private void OnBatteryChanged(float value) { if (batteryBar != null) batteryBar.value = value; }
        private void OnStaminaChanged(float value) { if (staminaBar != null) staminaBar.value = value; }

        private void OnPromptChanged(string text)
        {
            if (promptText != null) promptText.text = text;
        }

        private void OnMessageShown(string text)
        {
            if (messageText == null) return;

            if (messageRoutine != null) StopCoroutine(messageRoutine);
            messageRoutine = StartCoroutine(ShowMessageRoutine(text));
        }

        private IEnumerator ShowMessageRoutine(string text)
        {
            messageText.text = text;
            yield return new WaitForSeconds(messageDuration);
            messageText.text = string.Empty;
        }

        private void OnInventoryChanged(ItemType changed)
        {
            if (inventoryText == null || inventory == null) return;

            StringBuilder sb = new StringBuilder();
            foreach (var pair in inventory.Items)
                sb.AppendLine(ItemCatalog.GetName(pair.Key) + " x" + pair.Value);

            if (inventory.Notes.Count > 0)
                sb.AppendLine("Catatan x" + inventory.Notes.Count);

            inventoryText.text = sb.ToString();
        }

        private void OnStateChanged(GameState state)
        {
            SetPanel(pausePanel, state == GameState.Paused);
            SetPanel(winPanel, state == GameState.Won);
            SetPanel(gameOverPanel, state == GameState.GameOver);
        }

        private static void SetPanel(GameObject panel, bool active)
        {
            if (panel != null) panel.SetActive(active);
        }

        // Dipanggil dari OnClick() tombol UI
        public void OnRestartClicked() { if (GameManager.Instance != null) GameManager.Instance.Restart(); }
        public void OnQuitClicked() { if (GameManager.Instance != null) GameManager.Instance.Quit(); }
    }
}
