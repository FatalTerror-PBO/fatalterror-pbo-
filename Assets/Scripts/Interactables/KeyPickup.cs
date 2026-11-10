using UnityEngine;
using HorrorGame.Items;

namespace HorrorGame.Interactables
{
    public class KeyPickup : PickupBase
    {
        protected override bool OnPickedUp(Inventory inventory, GameObject interactor)
        {
            inventory.Add(itemType, amount);
            ShowMessage("Kamu menemukan " + ItemCatalog.GetName(itemType) + ".");
            return true;
        }
    }
}
