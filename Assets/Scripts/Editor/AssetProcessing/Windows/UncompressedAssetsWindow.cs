using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectTools.Editor.AssetTools
{
    public sealed class UncompressedAssetsWindow : EditorWindow
    {
        [SerializeField] private AssetKind _kind;
        private List<AssetEntry> _entries = new List<AssetEntry>();
        private ListView _list;
        private Label _summary;
        private TextField _search;

        public static void Open()
        {
            var window = GetWindow<UncompressedAssetsWindow>("Uncompressed Assets");
            window.minSize = new Vector2(560, 320);
            window.Show();
        }

        public void CreateGUI()
        {
            rootVisualElement.Clear();
            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            var kindField = new EnumField("Asset type", _kind);
            kindField.style.flexGrow = 1;
            kindField.RegisterValueChangedCallback(OnKindChanged);
            toolbar.Add(kindField);
            toolbar.Add(new Button(RefreshAssets) { text = "Refresh list" });
            rootVisualElement.Add(toolbar);
            rootVisualElement.Add(new HelpBox(
                "Scans project Assets, excluding Editor folders. Textures and audio use effective WebGL import settings. " +
                "Models show Mesh Compression Off; the existing Optimize action preserves mesh compression. " +
                "This list includes intentionally uncompressed assets and does not change import settings.",
                HelpBoxMessageType.Info));

            _search = new TextField("Filter by path");
            _search.RegisterValueChangedCallback(OnSearchChanged);
            rootVisualElement.Add(_search);
            _summary = new Label();
            rootVisualElement.Add(_summary);
            _list = new ListView();
            _list.fixedItemHeight = 52;
            _list.selectionType = SelectionType.None;
            _list.makeItem = MakeRow;
            _list.bindItem = BindRow;
            _list.style.flexGrow = 1;
            rootVisualElement.Add(_list);
            RefreshAssets();
        }

        private void OnKindChanged(ChangeEvent<Enum> change)
        {
            _kind = (AssetKind)change.newValue;
            RefreshAssets();
        }

        private void OnSearchChanged(ChangeEvent<string> change)
        {
            FilterEntries();
        }

        private void RefreshAssets()
        {
            try
            {
                _entries = UncompressedAssetScanner.Scan(_kind);
                FilterEntries();
            }
            catch (Exception exception)
            {
                _entries.Clear();
                FilterEntries();
                _summary.text = "Scan failed: " + exception.Message;
            }
        }

        private void FilterEntries()
        {
            var visible = new List<AssetEntry>();
            foreach (AssetEntry entry in _entries)
            {
                if (entry.Path.IndexOf(_search.value, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    visible.Add(entry);
                }
            }

            _list.itemsSource = visible;
            _list.Rebuild();
            _summary.text = "Uncompressed " + _kind + ": " + visible.Count + " / " + _entries.Count;
        }

        private VisualElement MakeRow()
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            var details = new VisualElement();
            details.style.flexGrow = 1;
            details.style.flexShrink = 1;
            var path = new Label { name = "path" };
            path.style.overflow = Overflow.Hidden;
            path.style.textOverflow = TextOverflow.Ellipsis;
            details.Add(path);
            details.Add(new Label { name = "reason" });
            row.Add(details);
            var select = new Button(() => SelectAsset((string)row.userData)) { text = "Select" };
            select.style.width = 65;
            row.Add(select);
            return row;
        }

        private void BindRow(VisualElement row, int index)
        {
            var entry = (AssetEntry)_list.itemsSource[index];
            row.userData = entry.Path;
            var path = row.Q<Label>("path");
            path.text = entry.Path;
            path.tooltip = entry.Path;
            row.Q<Label>("reason").text = entry.ImportSettings;
        }

        internal static void SelectAsset(string path)
        {
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (asset == null)
            {
                return;
            }

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}
