using System.Collections;
using UnityEngine;
using HorrorGame.Audio;
using HorrorGame.Core;
using HorrorGame.Items;

namespace HorrorGame.Interactables
{
    public class Door : InteractableBase
    {
        [Header("Syarat")]
        [SerializeField] private ItemType requiredItem = ItemType.None;
        [SerializeField] private bool consumeItem = true;

        [Header("Animasi")]
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float openSpeed = 1.5f;

        [Header("Pintu keluar?")]
        [SerializeField] private bool isExit = false;

        private bool isOpen;
        private bool busy;

        public override bool CanInteract => !isOpen && !busy;

        public override string Prompt
        {
            get
            {
                if (requiredItem == ItemType.None) return base.Prompt;
                return base.Prompt + " (butuh " + ItemCatalog.GetName(requiredItem) + ")";
            }
        }

        public override void Interact(GameObject interactor)
        {
            Inventory inventory = interactor.GetComponentInParent<Inventory>();

            if (requiredItem != ItemType.None)
            {
                bool ok = inventory != null &&
                          (consumeItem ? inventory.Consume(requiredItem) : inventory.GetCount(requiredItem) > 0);

                if (!ok)
                {
                    ShowMessage("Terkunci. Kamu butuh " + ItemCatalog.GetName(requiredItem) + ".");
                    if (AudioManager.Instance != null) AudioManager.Instance.Play("locked");
                    return;
                }
            }

            if (isExit)
            {
                if (GameManager.Instance != null) GameManager.Instance.WinGame();
                return;
            }

            if (AudioManager.Instance != null) AudioManager.Instance.Play("door");
            StartCoroutine(OpenRoutine());
        }

        private IEnumerator OpenRoutine()
        {
            busy = true;

            Quaternion start = transform.localRotation;
            Quaternion end = start * Quaternion.Euler(0f, openAngle, 0f);
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime * openSpeed;
                transform.localRotation = Quaternion.Slerp(start, end, t);
                yield return null;
            }

            isOpen = true;
            busy = false;
        }
    }
}
