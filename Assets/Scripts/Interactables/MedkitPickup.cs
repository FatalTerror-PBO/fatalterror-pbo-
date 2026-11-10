using UnityEngine;
using HorrorGame.Items;
using HorrorGame.Player;

namespace HorrorGame.Interactables
{
    public class MedkitPickup : PickupBase
    {
        [SerializeField] private int healAmount = 40;

        protected override bool OnPickedUp(Inventory inventory, GameObject interactor)
        {
            PlayerHealth health = interactor.GetComponent<PlayerHealth>();
            if (health == null) return false;

            if (!health.Heal(healAmount))
            {
                ShowMessage("Nyawamu sudah penuh.");
                return false;
            }

            ShowMessage("Nyawa dipulihkan.");
            return true;
        }
    }
}
