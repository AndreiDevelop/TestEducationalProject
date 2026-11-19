using System;
using UnityEditor;
using UnityEngine;

namespace ProjectTools.Editor.AssetTools
{
    [Serializable]
    public sealed class AudioCompressionEntry
    {
        [SerializeField] private string _folderGuid = "";
        [SerializeField] private AudioCompressionSettings _settings =
            new AudioCompressionSettings();
        [NonSerialized] private string _status = "";

        public DefaultAsset Folder
        {
            get
            {
                string path = FolderPath;
                return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<DefaultAsset>(path);
            }
        }
        public string FolderPath => AssetDatabase.GUIDToAssetPath(_folderGuid);
        public AudioCompressionSettings Settings => _settings;
        public string Status => _status;
        public bool HasFolder => IsProjectFolder(FolderPath);

        public bool SetFolder(DefaultAsset folder)
        {
            string path = folder == null ? "" : AssetDatabase.GetAssetPath(folder);
            if (folder != null && !IsProjectFolder(path))
            {
                _status = "Choose a folder inside this project's Assets.";
                return false;
            }

            _folderGuid = folder == null ? "" : AssetDatabase.AssetPathToGUID(path);
            _status = "";
            return true;
        }

        public string Optimize()
        {
            if (!HasFolder)
            {
                _status = "Choose an existing Assets folder before optimizing.";
                return _status;
            }

            try
            {
                _status = AudioCompressionProcessor.Process(new[] { FolderPath }, _settings,
                    Array.Empty<AudioClip>(), false, null);
            }
            catch (Exception exception)
            {
                _status = "Audio compression failed: " + exception.Message;
            }

            return _status;
        }

        private static bool IsProjectFolder(string path)
        {
            return !string.IsNullOrEmpty(path) && AssetDatabase.IsValidFolder(path) &&
                (path == "Assets" || path.StartsWith("Assets/", StringComparison.Ordinal));
        }
    }
}
