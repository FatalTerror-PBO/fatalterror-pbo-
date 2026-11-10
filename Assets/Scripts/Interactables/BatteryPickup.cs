using UnityEngine;
using HorrorGame.Items;
using HorrorGame.Player;

namespace HorrorGame.Interactables
{
    public class BatteryPickup : PickupBase
    {
        [SerializeField] private float rechargeAmount = 40f;

        protected override bool OnPickedUp(Inventory inventory, GameObject interactor)
        {
            Flashlight flashlight = interactor.GetComponentInChildren<Flashlight>();
            if (flashlight == null) return false;

            flashlight.Recharge(rechargeAmount);
            ShowMessage("Baterai senter terisi.");
            return true;
        }
    }
}
