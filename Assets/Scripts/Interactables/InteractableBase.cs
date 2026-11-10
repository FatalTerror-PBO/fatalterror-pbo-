using UnityEngine;
using HorrorGame.Core;
using HorrorGame.Interfaces;

namespace HorrorGame.Interactables
{
    /// <summary>
    /// ABSTRACT CLASS yang mengimplementasikan INTERFACE IInteractable.
    /// Method Interact() wajib di-override oleh kelas turunan.
    /// </summary>
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [SerializeField] private string prompt = "Tekan E";

        public virtual string Prompt => prompt;

        public virtual bool CanInteract => true;

        public abstract void Interact(GameObject interactor);

        protected void ShowMessage(string message)
        {
            GameEvents.MessageShown.Raise(message);
        }
    }
}
