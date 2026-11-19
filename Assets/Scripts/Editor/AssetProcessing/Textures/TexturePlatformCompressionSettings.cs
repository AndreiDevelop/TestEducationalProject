using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectTools.Editor.AssetTools
{
    [Serializable]
    public sealed class TexturePlatformCompressionSettings
    {
        private const string WebPlatformName = "WebGL";
        [SerializeField] private int _maxSize = 2048;
        [SerializeField] private TextureResizeAlgorithm _resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
        [SerializeField] private TextureImporterFormat _format = TextureImporterFormat.Automatic;
        [SerializeField] private TextureImporterCompression _compression = TextureImporterCompression.CompressedHQ;
        [SerializeField] private int _quality = 100;

        private static readonly int[] MaxSizes = { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };
        private static readonly string[] MaxSizeLabels = { "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192" };
        private static TextureImporterFormat[] _webFormats;
        private static string[] _webFormatLabels;

        private static void InitializeFormats()
        {
            if (_webFormats != null)
            {
                return;
            }

            _webFormats = GetWebFormats();
            _webFormatLabels = GetWebFormatLabels();
        }

        private static TextureImporterFormat[] GetWebFormats()
        {
            var formats = new List<TextureImporterFormat> { TextureImporterFormat.Automatic };
            foreach (TextureImporterFormat format in Enum.GetValues(typeof(TextureImporterFormat)))
            {
                if ((int)format < 0 || formats.Contains(format))
                {
                    continue;
                }

                if (TextureImporter.IsPlatformTextureFormatValid(TextureImporterType.Default, BuildTarget.WebGL, format))
                {
                    formats.Add(format);
                }
            }

            return formats.ToArray();
        }

        private static string[] GetWebFormatLabels()
        {
            var labels = new string[_webFormats.Length];
            for (int i = 0; i < _webFormats.Length; i++)
            {
                switch (_webFormats[i])
                {
                    case TextureImporterFormat.Automatic:
                        labels[i] = "Automatic";
                        break;
                    case TextureImporterFormat.DXT1:
                        labels[i] = "RGB Compressed DXT1 | BC1";
                        break;
                    case TextureImporterFormat.DXT5:
                        labels[i] = "RGBA Compressed DXT5 | BC3";
                        break;
                    default:
                        labels[i] = _webFormats[i].ToString().Replace("_", " ");
                        break;
                }
            }

            return labels;
        }

        public void DrawSettings()
        {
            InitializeFormats();
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.Toggle(new GUIContent("Override For Web",
                    "Optimize always enables this override on every processed texture."), true);
            }

            _maxSize = EditorGUILayout.IntPopup("Max Size", _maxSize, MaxSizeLabels, MaxSizes);
            _resizeAlgorithm = (TextureResizeAlgorithm)EditorGUILayout.EnumPopup("Resize Algorithm", _resizeAlgorithm);
            int selectedFormat = Math.Max(0, Array.IndexOf(_webFormats, _format));
            _format = _webFormats[EditorGUILayout.Popup("Format", selectedFormat, _webFormatLabels)];
            using (new EditorGUI.DisabledScope(_format != TextureImporterFormat.Automatic))
            {
                _compression = (TextureImporterCompression)EditorGUILayout.EnumPopup("Compression", _compression);
            }

            _quality = EditorGUILayout.IntSlider("Compression Quality", _quality, 0, 100);
        }

        public bool Apply(TextureImporter importer)
        {
            var settings = importer.GetPlatformTextureSettings(WebPlatformName);
            bool changed = !settings.overridden || settings.maxTextureSize != _maxSize ||
                settings.resizeAlgorithm != _resizeAlgorithm || settings.format != _format ||
                settings.textureCompression != _compression || settings.compressionQuality != _quality ||
                settings.crunchedCompression;
            if (!changed)
            {
                return false;
            }

            settings.name = WebPlatformName;
            settings.overridden = true;
            settings.maxTextureSize = _maxSize;
            settings.resizeAlgorithm = _resizeAlgorithm;
            settings.format = _format;
            settings.textureCompression = _compression;
            settings.compressionQuality = _quality;
            settings.crunchedCompression = false;
            importer.SetPlatformTextureSettings(settings);
            return true;
        }
    }
}
