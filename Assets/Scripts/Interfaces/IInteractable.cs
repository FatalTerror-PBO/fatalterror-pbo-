using UnityEngine;

namespace HorrorGame.Interfaces
{
    /// <summary>INTERFACE: kontrak untuk semua objek yang bisa ditekan tombol E.</summary>
    public interface IInteractable
    {
        string Prompt { get; }
        bool CanInteract { get; }
        void Interact(GameObject interactor);
    }
}
