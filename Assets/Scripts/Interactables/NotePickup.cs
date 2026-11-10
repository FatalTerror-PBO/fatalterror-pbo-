using UnityEngine;
using HorrorGame.Items;

namespace HorrorGame.Interactables
{
    public class NotePickup : PickupBase
    {
        [TextArea(2, 6)]
        [SerializeField] private string noteText = "Jangan percaya suara di lorong itu...";

        protected override bool OnPickedUp(Inventory inventory, GameObject interactor)
        {
            inventory.AddNote(noteText);
            ShowMessage(noteText);
            return true;
        }
    }
}
