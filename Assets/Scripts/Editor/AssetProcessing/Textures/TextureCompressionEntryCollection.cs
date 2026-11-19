using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectTools.Editor.AssetTools
{
    [Serializable]
    public sealed class TextureCompressionEntryCollection
    {
        [SerializeField] private List<TextureCompressionEntry> _entries = new List<TextureCompressionEntry>();

        public int Count => _entries.Count;

        public TextureCompressionEntry GetEntry(int index)
        {
            return _entries[index];
        }

        public TextureCompressionEntry Add()
        {
            var entry = new TextureCompressionEntry();
            _entries.Add(entry);
            return entry;
        }

        public void RemoveAt(int index)
        {
            _entries.RemoveAt(index);
        }
    }
}
