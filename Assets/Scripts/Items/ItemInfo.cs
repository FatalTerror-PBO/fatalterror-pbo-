namespace HorrorGame.Items
{
    /// <summary>Data deskripsi item. Property dengan get dan private set.</summary>
    public class ItemInfo
    {
        public string DisplayName { get; private set; }
        public string Description { get; private set; }

        public ItemInfo(string displayName, string description)
        {
            DisplayName = displayName;
            Description = description;
        }
    }
}
