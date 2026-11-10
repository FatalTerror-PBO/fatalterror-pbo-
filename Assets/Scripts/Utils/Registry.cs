using System.Collections.Generic;

namespace HorrorGame.Utils
{
    /// <summary>
    /// GENERIC CLASS dengan dua tipe (TKey, TValue) + DICTIONARY di dalamnya.
    /// Constraint "where TValue : class" membatasi TValue hanya untuk tipe reference.
    /// </summary>
    public class Registry<TKey, TValue> where TValue : class
    {
        private readonly Dictionary<TKey, TValue> entries = new Dictionary<TKey, TValue>();

        public int Count => entries.Count;

        public IEnumerable<KeyValuePair<TKey, TValue>> All => entries;

        public void Register(TKey key, TValue value)
        {
            entries[key] = value;
        }

        public bool TryGet(TKey key, out TValue value)
        {
            return entries.TryGetValue(key, out value);
        }
    }
}
