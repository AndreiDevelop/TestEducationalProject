using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectTools.Editor.AssetTools
{
    public sealed class CompressAssetsWindow : AssetProcessingWindow
    {
        [SerializeField] private bool _entireProject;
        [SerializeField] private DefaultAsset _folder;
        private bool _forceReadWriteOff;
        [SerializeField] private TextureCompressionEntryCollection _textureEntries =
            new TextureCompressionEntryCollection();

        [SerializeField] private AudioCompressionEntryCollection _audioEntries =
            new AudioCompressionEntryCollection();

        [SerializeField]
        private int _activeTab;
        private AssetKind Kind => (AssetKind)_activeTab;

        private Vector2 _assetsScroll;
        private string _scannedScope = "";
        private List<AssetEntry> _foundAssets = new List<AssetEntry>();
        private readonly Dictionary<string, string> _outcomes = new Dictionary<string, string>();
        private string TextureEntriesKey => "ProjectTools.TextureCompression.Entries." + Application.dataPath;
        private string AudioEntriesKey => "ProjectTools.AudioCompression.Entries." + Application.dataPath;
        private string ProfileSuffix => _entireProject ? "" : ".Folder." + FolderGuid;
        private string FolderGuid => _folder == null ? "None" : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_folder));
        private bool HasScope => _entireProject || _folder != null;
        private string ModelPreferencesKey => "ProjectTools.ModelCompression." + Application.dataPath + ProfileSuffix;

        protected override void OnEnable()
        {
            base.OnEnable();
            LoadTextureEntries();
            LoadScopePreferences();
            LoadAudioEntries();
        }

        private void LoadScopePreferences()
        {
            _forceReadWriteOff = EditorPrefs.GetBool(ModelPreferencesKey, false);
        }

        private void OnDisable()
        {
            SaveTextureEntries();
            SaveAudioEntries();
            SaveScopePreferences();
        }

        private void SaveScopePreferences()
        {
            EditorPrefs.SetBool(ModelPreferencesKey, _forceReadWriteOff);
        }

        private void LoadTextureEntries()
        {
            string saved = EditorPrefs.GetString(TextureEntriesKey, "");
            if (!string.IsNullOrEmpty(saved))
            {
                JsonUtility.FromJsonOverwrite(saved, _textureEntries);
            }
            else if (_textureEntries.Count == 0)
            {
                _textureEntries.Add();
            }
        }

        private void SaveTextureEntries()
        {
            EditorPrefs.SetString(TextureEntriesKey, JsonUtility.ToJson(_textureEntries));
        }

        private void LoadAudioEntries()
        {
            string saved = EditorPrefs.GetString(AudioEntriesKey, "");
            if (!string.IsNullOrEmpty(saved))
            {
                JsonUtility.FromJsonOverwrite(saved, _audioEntries);
            }
            else if (_audioEntries.Count == 0)
            {
                var entry = _audioEntries.Add();
                entry.SetFolder(_entireProject ? AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets") : _folder);
                string legacyKey = "ProjectTools.AudioCompression." + Application.dataPath + ProfileSuffix;
                string legacySettings = EditorPrefs.GetString(legacyKey, "");
                if (!string.IsNullOrEmpty(legacySettings))
                {
                    JsonUtility.FromJsonOverwrite(legacySettings, entry.Settings);
                }
            }
        }

        private void SaveAudioEntries()
        {
            EditorPrefs.SetString(AudioEntriesKey, JsonUtility.ToJson(_audioEntries));
        }

        [MenuItem("Tools/Compress Assets")]
        public static void Open()
        {
            GetWindow<CompressAssetsWindow>("Compress Assets");
        }

        public static void OpenFolder(string path)
        {
            string normalized = AssetProcessing.NormalizeFolder(path);
            var folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(normalized);
            var window = GetWindow<CompressAssetsWindow>("Compress Assets");
            if (window._activeTab == 0)
            {
                window._textureEntries.Add().SetFolder(folder);
                window.SaveTextureEntries();
            }
            else if (window._activeTab == 2)
            {
                window._audioEntries.Add().SetFolder(folder);
                window.SaveAudioEntries();
            }
            else
            {
                window.SetFolder(folder);
            }
        }

        public bool SetFolder(DefaultAsset folder)
        {
            if (folder != null)
            {
                string path = AssetDatabase.GetAssetPath(folder);
                if (!AssetDatabase.IsValidFolder(path) || (path != "Assets" && !path.StartsWith("Assets/")))
                {
                    _status = "Drop a folder from this project's Assets.";
                    return false;
                }
            }

            SaveScopePreferences();
            _folder = folder;
            _entireProject = false;
            LoadScopePreferences();
            ResetScan();
            return true;
        }

        private string[] Roots
        {
            get
            {
                if (_entireProject)
                {
                    return new[] { "Assets" };
                }

                if (_folder == null)
                {
                    return Array.Empty<string>();
                }

                return new[] { AssetDatabase.GetAssetPath(_folder) };
            }
        }

        private void ResetScan()
        {
            _foundAssets.Clear();
            _outcomes.Clear();
            _scannedScope = "";
            _status = "";
            _assetsScroll = Vector2.zero;
            Repaint();
        }

        private void DrawScope()
        {
            EditorGUILayout.LabelField("Processing scope", EditorStyles.boldLabel);
            var folder = (DefaultAsset)EditorGUILayout.ObjectField(
                new GUIContent("Folder", "Drag an Assets folder here. Subfolders are included."),
                _folder, typeof(DefaultAsset), false);
            if (folder != _folder)
            {
                SetFolder(folder);
            }

            bool entireProject = EditorGUILayout.Toggle("All project Assets", _entireProject);
            if (entireProject != _entireProject)
            {
                SaveScopePreferences();
                _entireProject = entireProject;
                LoadScopePreferences();
                ResetScan();
            }

            if (!HasScope)
            {
                EditorGUILayout.HelpBox("Drop a project folder into Folder to configure its compression.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.LabelField("Applies recursively to", string.Join(", ", Roots), EditorStyles.wordWrappedMiniLabel);
            }

            EditorGUILayout.Space();
        }

        private string Scope => Kind + "|" + string.Join("|", Roots);

        private void Scan()
        {
            try
            {
                _foundAssets = AssetProcessing.ScanAssets(Roots, Kind);
                _scannedScope = Scope;
                _status = "Scan complete. Apply processes the assets in the selected scope.";
            }
            catch (Exception exception)
            {
                _status = exception.Message;
            }

            Repaint();
        }

        private void OnGUI()
        {
            using (new EditorGUI.DisabledScope(EditorApplication.isCompiling || EditorApplication.isUpdating))
            {
                if (GUILayout.Button("Show uncompressed assets", GUILayout.Height(28)))
                {
                    UncompressedAssetsWindow.Open();
                }
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            int nextTab = GUILayout.Toolbar(_activeTab, new[] { "Textures", "3D Models", "Audio" });
            if (nextTab != _activeTab)
            {
                _activeTab = nextTab;
                _assetsScroll = Vector2.zero;
                _outcomes.Clear();
                _foundAssets.Clear();
                _scannedScope = "";
                _status = "";
                if (_activeTab == 1 && HasScope)
                {
                    Scan();
                }
            }

            if (_activeTab == 0)
            {
                DrawTextures();
                EditorGUILayout.EndScrollView();
                return;
            }

            if (_activeTab == 2)
            {
                DrawAudio();
                EditorGUILayout.EndScrollView();
                return;
            }

            DrawScope();
            EditorGUILayout.HelpBox("Optimize imported 3D models. Rigs, animation clips, UVs and mesh compression are preserved.", MessageType.Info);

            _forceReadWriteOff = EditorGUILayout.Toggle("Disable Read/Write", _forceReadWriteOff);
            if (_forceReadWriteOff)
            {
                EditorGUILayout.HelpBox("Enable only if your scripts, mesh batching and collider generation do not need CPU asset access.", MessageType.Warning);
            }

            using (new EditorGUI.DisabledScope(EditorApplication.isCompiling || EditorApplication.isPlaying || EditorApplication.isUpdating || !HasScope))
            {
                if (GUILayout.Button("Analyze (no changes)"))
                {
                    Run(true);
                }

                if (GUILayout.Button("Apply settings and optimize"))
                {
                    Run(false);
                }
            }

            DrawAssets();
            ShowStatus();
            EditorGUILayout.EndScrollView();
        }

        private void DrawTextures()
        {
            EditorGUI.BeginChangeCheck();
            int removeIndex = -1;
            for (int i = 0; i < _textureEntries.Count; i++)
            {
                if (DrawTextureEntry(_textureEntries.GetEntry(i), i))
                {
                    removeIndex = i;
                }
            }

            if (removeIndex >= 0)
            {
                _textureEntries.RemoveAt(removeIndex);
            }

            bool added = GUILayout.Button("Add folder", GUILayout.Height(26));
            if (added)
            {
                _textureEntries.Add();
            }

            if (EditorGUI.EndChangeCheck() || added || removeIndex >= 0)
            {
                SaveTextureEntries();
            }
        }

        private bool DrawTextureEntry(TextureCompressionEntry entry, int index)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Folder " + (index + 1), EditorStyles.boldLabel);
            bool remove = GUILayout.Button("Remove", GUILayout.Width(70));
            EditorGUILayout.EndHorizontal();

            var folder = (DefaultAsset)EditorGUILayout.ObjectField(
                new GUIContent("Folder", "Drag an Assets folder here. Subfolders are included."),
                entry.Folder, typeof(DefaultAsset), false);
            if (folder != entry.Folder)
            {
                entry.SetFolder(folder);
            }

            entry.Settings.DrawSettings();
            bool busy = EditorApplication.isCompiling || EditorApplication.isPlaying || EditorApplication.isUpdating;
            using (new EditorGUI.DisabledScope(busy || !entry.HasFolder))
            {
                if (GUILayout.Button("Optimize", GUILayout.Height(30)))
                {
                    SaveTextureEntries();
                    entry.Optimize();
                }
            }

            if (!string.IsNullOrEmpty(entry.Status))
            {
                EditorGUILayout.HelpBox(entry.Status, MessageType.Info);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            return remove;
        }

        private void Run(bool preview)
        {
            _outcomes.Clear();
            _status = AssetProcessing.OptimizeModels(Roots, preview, _forceReadWriteOff, RecordOutcome);
            _foundAssets = AssetProcessing.ScanAssets(Roots, Kind);
            _scannedScope = Scope;
        }

        private void DrawAssets()
        {
            var visibleAssets = _foundAssets;
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Found " + Kind + " (" + visibleAssets.Count + ")", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(!HasScope))
            {
                if (GUILayout.Button("Refresh list", GUILayout.Width(100)))
                {
                    Scan();
                }
            }

            EditorGUILayout.EndHorizontal();
            if (_scannedScope != Scope)
            {
                EditorGUILayout.HelpBox("Folder selection changed. Refresh the list or click Analyze.", MessageType.Info);
                return;
            }

            if (visibleAssets.Count == 0)
            {
                EditorGUILayout.HelpBox("No matching assets found.", MessageType.Info);
                return;
            }

            _assetsScroll = EditorGUILayout.BeginScrollView(_assetsScroll, GUILayout.Height(280));
            foreach (AssetEntry entry in visibleAssets)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.SelectableLabel(entry.Path, GUILayout.Height(18));
                if (GUILayout.Button("Select", GUILayout.Width(55)))
                {
                    var asset = AssetDatabase.LoadMainAssetAtPath(entry.Path);
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField((entry.Bytes / (1024d * 1024d)).ToString("F2") + " MiB (" + entry.Bytes + " bytes)   " + entry.ImportSettings, EditorStyles.wordWrappedMiniLabel);
                if (_outcomes.TryGetValue(entry.Path, out var outcome))
                {
                    EditorGUILayout.LabelField(outcome, EditorStyles.wordWrappedMiniLabel);
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();
        }

        private void RecordOutcome(string path, string outcome)
        {
            _outcomes[path] = outcome;
        }

        private void DrawAudio()
        {
            EditorGUI.BeginChangeCheck();
            int removeIndex = -1;
            for (int i = 0; i < _audioEntries.Count; i++)
            {
                if (DrawAudioEntry(_audioEntries.GetEntry(i), i))
                {
                    removeIndex = i;
                }
            }

            if (removeIndex >= 0)
            {
                _audioEntries.RemoveAt(removeIndex);
            }

            bool added = GUILayout.Button("Add folder", GUILayout.Height(26));
            if (added)
            {
                _audioEntries.Add();
            }

            if (EditorGUI.EndChangeCheck() || added || removeIndex >= 0)
            {
                SaveAudioEntries();
            }
        }

        private bool DrawAudioEntry(AudioCompressionEntry entry, int index)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Folder " + (index + 1), EditorStyles.boldLabel);
            bool remove = GUILayout.Button("Remove", GUILayout.Width(70));
            EditorGUILayout.EndHorizontal();

            var folder = (DefaultAsset)EditorGUILayout.ObjectField(
                new GUIContent("Folder", "Drag an Assets folder here. Subfolders are included."),
                entry.Folder, typeof(DefaultAsset), false);
            if (folder != entry.Folder)
            {
                entry.SetFolder(folder);
            }

            entry.Settings.DrawSettings();
            bool busy = EditorApplication.isCompiling || EditorApplication.isPlaying || EditorApplication.isUpdating;
            using (new EditorGUI.DisabledScope(busy || !entry.HasFolder))
            {
                if (GUILayout.Button("Optimize", GUILayout.Height(30)))
                {
                    SaveAudioEntries();
                    entry.Optimize();
                }
            }

            if (!string.IsNullOrEmpty(entry.Status))
            {
                EditorGUILayout.HelpBox(entry.Status, MessageType.Info);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            return remove;
        }
    }
}
