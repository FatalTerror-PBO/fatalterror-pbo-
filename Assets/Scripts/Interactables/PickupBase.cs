using UnityEngine;
using HorrorGame.Audio;
using HorrorGame.Core;
using HorrorGame.Items;

namespace HorrorGame.Interactables
{
    /// <summary>
    /// INHERITANCE bertingkat: InteractableBase -> PickupBase -> (KeyPickup, BatteryPickup, ...).
    /// </summary>
    public abstract class PickupBase : InteractableBase
    {
        [SerializeField] protected ItemType itemType = ItemType.None;
        [SerializeField] protected int amount = 1;
        [SerializeField] private string pickupSoundId = "pickup";

        public override void Interact(GameObject interactor)
        {
            Inventory inventory = interactor.GetComponentInParent<Inventory>();
            if (inventory == null) return;

            if (OnPickedUp(inventory, interactor))
            {
                if (AudioManager.Instance != null) AudioManager.Instance.Play(pickupSoundId);
                Destroy(gameObject);
            }
        }

        /// <summary>Return true jika item berhasil diambil.</summary>
        protected abstract bool OnPickedUp(Inventory inventory, GameObject interactor);
    }
}
