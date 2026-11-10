using System;

namespace HorrorGame.Utils
{
    /// <summary>
    /// GENERIC CLASS: saluran event bertipe T (float, string, enum, dll).
    /// Dipakai agar script saling berkomunikasi tanpa saling bergantung langsung.
    /// </summary>
    public class EventChannel<T>
    {
        private event Action<T> Listeners;

        public void Subscribe(Action<T> listener)
        {
            Listeners += listener;
        }

        public void Unsubscribe(Action<T> listener)
        {
            Listeners -= listener;
        }

        public void Raise(T value)
        {
            Listeners?.Invoke(value);
        }
    }
}
