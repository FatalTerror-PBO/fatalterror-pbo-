using System.Collections.Generic;
using UnityEngine;
using HorrorGame.Core;

namespace HorrorGame.Items
{
    /// <summary>
    /// COLLECTION: Dictionary&lt;ItemType,int&gt; untuk jumlah item, List&lt;string&gt; untuk catatan.
    /// </summary>
    public class Inventory : MonoBehaviour
    {
        private readonly Dictionary<ItemType, int> items = new Dictionary<ItemType, int>();
        private readonly List<string> notes = new List<string>();

        public IReadOnlyDictionary<ItemType, int> Items => items;
        public IReadOnlyList<string> Notes => notes;

        public int GetCount(ItemType type)
        {
            return items.TryGetValue(type, out int count) ? count : 0;
        }

        public void Add(ItemType type, int amount = 1)
        {
            items[type] = GetCount(type) + amount;
            GameEvents.InventoryChanged.Raise(type);
        }

        public bool Consume(ItemType type, int amount = 1)
        {
            int current = GetCount(type);
            if (current < amount) return false;

            if (current - amount == 0) items.Remove(type);
            else items[type] = current - amount;

            GameEvents.InventoryChanged.Raise(type);
            return true;
        }

        public void AddNote(string text)
        {
            notes.Add(text);
            GameEvents.InventoryChanged.Raise(ItemType.Note);
        }
    }
}
