using System;
using System.Collections.Generic;
using UnityEngine;

namespace HorrorGame.Audio
{
    [Serializable]
    public class SoundEntry
    {
        public string id;
        public AudioClip clip;
    }

    /// <summary>DICTIONARY: menyimpan AudioClip berdasarkan id string.</summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private List<SoundEntry> sounds = new List<SoundEntry>();

        private readonly Dictionary<string, AudioClip> soundLookup = new Dictionary<string, AudioClip>();

        private void Awake()
        {
            Instance = this;

            foreach (SoundEntry entry in sounds)
            {
                if (entry.clip != null && !string.IsNullOrEmpty(entry.id))
                    soundLookup[entry.id] = entry.clip;
            }
        }

        public void Play(string id)
        {
            if (sfxSource != null && soundLookup.TryGetValue(id, out AudioClip clip))
                sfxSource.PlayOneShot(clip);
        }

        public void PlayAt(string id, Vector3 position)
        {
            if (soundLookup.TryGetValue(id, out AudioClip clip))
                AudioSource.PlayClipAtPoint(clip, position);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
