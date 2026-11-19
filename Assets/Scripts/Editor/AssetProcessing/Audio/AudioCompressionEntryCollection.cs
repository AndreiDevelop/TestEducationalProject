using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectTools.Editor.AssetTools
{
    [Serializable]
    public sealed class AudioCompressionEntryCollection
    {
        [SerializeField] private List<AudioCompressionEntry> _entries = new List<AudioCompressionEntry>();

        public int Count => _entries.Count;

        public AudioCompressionEntry GetEntry(int index)
        {
            return _entries[index];
        }

        public AudioCompressionEntry Add()
        {
            var entry = new AudioCompressionEntry();
            _entries.Add(entry);
            return entry;
        }

        public void RemoveAt(int index)
        {
            _entries.RemoveAt(index);
        }
    }
}
