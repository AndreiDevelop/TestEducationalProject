using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace ProjectTools.Editor.AssetTools
{
    public static class UncompressedAssetScanner
    {
        private const string PlatformName = "WebGL";

        public static List<AssetEntry> Scan(AssetKind kind)
        {
            var results = new List<AssetEntry>();
            foreach (AssetEntry entry in AssetProcessing.ScanAssets(new[] { "Assets" }, kind))
            {
                AssetImporter importer = AssetImporter.GetAtPath(entry.Path);
                string reason = GetUncompressedReason(importer);
                if (!string.IsNullOrEmpty(reason))
                {
                    results.Add(new AssetEntry(entry.Path, entry.Bytes, reason));
                }
            }

            return results;
        }

        private static string GetUncompressedReason(AssetImporter importer)
        {
            if (importer is TextureImporter texture)
            {
                TextureImporterPlatformSettings settings = texture.GetPlatformTextureSettings(PlatformName);
                TextureImporterFormat format = settings.overridden ? settings.format : TextureImporterFormat.Automatic;
                if (format == TextureImporterFormat.Automatic)
                {
                    format = texture.GetAutomaticFormat(PlatformName);
                }

                // Importer formats share native texture format values, including differently named packed formats.
                bool compressed = GraphicsFormatUtility.IsCompressedFormat((TextureFormat)(int)format);
                return compressed ? null : "WebGL texture format: " + format;
            }

            if (importer is AudioImporter audio)
            {
                AudioImporterSampleSettings settings = audio.ContainsSampleSettingsOverride(PlatformName)
                    ? audio.GetOverrideSampleSettings(PlatformName)
                    : audio.defaultSampleSettings;
                return settings.compressionFormat == AudioCompressionFormat.PCM ? "WebGL audio format: PCM" : null;
            }

            if (importer is ModelImporter model && model.meshCompression == ModelImporterMeshCompression.Off)
            {
                return "Mesh Compression: Off";
            }

            return null;
        }
    }
}
