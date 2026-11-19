using System;
using UnityEditor;
using UnityEngine;

namespace ProjectTools.Editor.AssetTools
{
    [Serializable]
    public sealed class AudioCompressionSettings
    {
        [SerializeField] private bool _webOverride = true;
        [SerializeField] private float _quality = 0.75f;
        [SerializeField] private AudioSampleRateSetting _sampleRate = AudioSampleRateSetting.PreserveSampleRate;
        [SerializeField] private int _sampleRateOverride = 44100;
        [SerializeField] private bool _changeLoadType;
        [SerializeField] private AudioClipLoadType _loadType = AudioClipLoadType.CompressedInMemory;
        [SerializeField] private bool _forceMono;

        public void DrawSettings()
        {
            _webOverride = EditorGUILayout.Toggle("Override For Web", _webOverride);
            EditorGUILayout.LabelField("Target", _webOverride ? "Web only" : "Default sample settings");
            EditorGUILayout.LabelField("Compression format", _webOverride ? "AAC" : "Vorbis");
            _quality = EditorGUILayout.Slider("Quality", _quality, 0f, 1f);
            if (_webOverride)
            {
                EditorGUILayout.LabelField("Sample rate", "44,100 Hz (Unity Web)");
            }
            else
            {
                _sampleRate = (AudioSampleRateSetting)EditorGUILayout.EnumPopup("Sample rate", _sampleRate);
                if (_sampleRate == AudioSampleRateSetting.OverrideSampleRate)
                {
                    _sampleRateOverride = EditorGUILayout.IntPopup("Frequency", _sampleRateOverride,
                        new[] { "11025 Hz", "22050 Hz", "44100 Hz", "48000 Hz" },
                        new[] { 11025, 22050, 44100, 48000 });
                }
            }

            _changeLoadType = EditorGUILayout.Toggle("Change load type", _changeLoadType);
            using (new EditorGUI.DisabledScope(!_changeLoadType))
            {
                _loadType = (AudioClipLoadType)EditorGUILayout.EnumPopup("Load type", _loadType);
            }

            _forceMono = EditorGUILayout.Toggle("Force mono", _forceMono);
            EditorGUILayout.HelpBox("Load type and channel count are preserved unless enabled above. " +
                "Mono affects all platforms. Source files are never rewritten. Existing overrides on other " +
                "platforms are preserved. Default settings do not replace existing platform overrides.", MessageType.Info);
        }

        public AudioImporterSampleSettings BuildSettings(AudioImporter importer)
        {
            var settings = ReadSettings(importer);
            settings.compressionFormat = _webOverride ? AudioCompressionFormat.AAC : AudioCompressionFormat.Vorbis;
            settings.quality = Mathf.Clamp01(_quality);
            if (!_webOverride)
            {
                settings.sampleRateSetting = _sampleRate;
                if (_sampleRate == AudioSampleRateSetting.OverrideSampleRate)
                {
                    settings.sampleRateOverride = (uint)Mathf.Clamp(_sampleRateOverride, 8000, 192000);
                }
            }

            if (_changeLoadType)
            {
                settings.loadType = _loadType;
            }

            return settings;
        }

        public AudioImporterSampleSettings ReadSettings(AudioImporter importer)
        {
            return _webOverride ? importer.GetOverrideSampleSettings("WebGL") : importer.defaultSampleSettings;
        }

        public string DescribeChange(AudioImporter importer)
        {
            string target = _webOverride ? "Web" : "Default";
            return target + ": " + Describe(ReadSettings(importer)) + " -> " + Describe(BuildSettings(importer)) +
                " | Mono: " + importer.forceToMono + " -> " + (_forceMono || importer.forceToMono);
        }

        public bool NeedsChange(AudioImporter importer)
        {
            return (_webOverride && !importer.ContainsSampleSettingsOverride("WebGL")) ||
                !Matches(ReadSettings(importer), BuildSettings(importer)) || (_forceMono && !importer.forceToMono);
        }

        public bool Apply(AudioImporter importer)
        {
            if (!NeedsChange(importer))
            {
                return false;
            }

            var settings = BuildSettings(importer);
            if (_webOverride)
            {
                if (!importer.SetOverrideSampleSettings("WebGL", settings))
                {
                    throw new InvalidOperationException("Unity rejected Web audio settings.");
                }
            }
            else
            {
                importer.defaultSampleSettings = settings;
            }

            if (_forceMono)
            {
                importer.forceToMono = true;
            }

            return true;
        }

        private static bool Matches(AudioImporterSampleSettings current, AudioImporterSampleSettings desired)
        {
            bool sampleRateMatches = current.sampleRateSetting == desired.sampleRateSetting &&
                (desired.sampleRateSetting != AudioSampleRateSetting.OverrideSampleRate ||
                    current.sampleRateOverride == desired.sampleRateOverride);
            return current.compressionFormat == desired.compressionFormat &&
                Mathf.Approximately(current.quality, desired.quality) && current.loadType == desired.loadType &&
                sampleRateMatches;
        }

        public static string Describe(AudioImporterSampleSettings settings)
        {
            string rate = settings.sampleRateSetting == AudioSampleRateSetting.OverrideSampleRate
                ? settings.sampleRateOverride + " Hz"
                : settings.sampleRateSetting.ToString();
            return settings.compressionFormat + " q=" + settings.quality.ToString("F2") + " | " +
                settings.loadType + " | " + rate;
        }
    }
}
