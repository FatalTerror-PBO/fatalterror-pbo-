using UnityEngine;
using HorrorGame.Core;
using HorrorGame.Interfaces;

namespace HorrorGame.Player
{
    /// <summary>Raycast dari kamera; berinteraksi dengan apa pun yang mengimplementasikan IInteractable.</summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float interactDistance = 2.5f;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        private string lastPrompt = string.Empty;

        private void Update()
        {
            if (!GameManager.IsPlaying) return;

            IInteractable target = FindTarget();
            string prompt = (target != null && target.CanInteract) ? target.Prompt : string.Empty;

            if (prompt != lastPrompt)
            {
                lastPrompt = prompt;
                GameEvents.PromptChanged.Raise(prompt);
            }

            if (target != null && target.CanInteract && Input.GetKeyDown(interactKey))
                target.Interact(gameObject);
        }

        private IInteractable FindTarget()
        {
            if (playerCamera == null) return null;

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
                return hit.collider.GetComponentInParent<IInteractable>();

            return null;
        }
    }
}
