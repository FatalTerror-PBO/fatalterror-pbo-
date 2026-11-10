using HorrorGame.Core;
using HorrorGame.Utils;

namespace HorrorGame.Items
{
    /// <summary>Katalog item memakai Registry generik (Registry&lt;ItemType, ItemInfo&gt;).</summary>
    public static class ItemCatalog
    {
        public static readonly Registry<ItemType, ItemInfo> Items = new Registry<ItemType, ItemInfo>();

        static ItemCatalog()
        {
            Items.Register(ItemType.Key, new ItemInfo("Kunci", "Untuk membuka pintu yang terkunci."));
            Items.Register(ItemType.Battery, new ItemInfo("Baterai", "Mengisi daya senter."));
            Items.Register(ItemType.Medkit, new ItemInfo("Medkit", "Memulihkan nyawa pemain."));
            Items.Register(ItemType.Note, new ItemInfo("Catatan", "Petunjuk dari penghuni sebelumnya."));
        }

        public static string GetName(ItemType type)
        {
            return Items.TryGet(type, out ItemInfo info) ? info.DisplayName : type.ToString();
        }
    }
}
