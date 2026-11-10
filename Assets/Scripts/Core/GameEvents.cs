using HorrorGame.Utils;

namespace HorrorGame.Core
{
    /// <summary>Kumpulan EventChannel generik yang dipakai seluruh game.</summary>
    public static class GameEvents
    {
        public static readonly EventChannel<float> HealthChanged = new EventChannel<float>();
        public static readonly EventChannel<float> BatteryChanged = new EventChannel<float>();
        public static readonly EventChannel<float> StaminaChanged = new EventChannel<float>();
        public static readonly EventChannel<string> PromptChanged = new EventChannel<string>();
        public static readonly EventChannel<string> MessageShown = new EventChannel<string>();
        public static readonly EventChannel<ItemType> InventoryChanged = new EventChannel<ItemType>();
        public static readonly EventChannel<GameState> StateChanged = new EventChannel<GameState>();
    }
}
