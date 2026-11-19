using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectTools.Editor.AssetTools
{
    public static class TextureCompressionProcessor
    {
        public static string Optimize(string[] roots, TexturePlatformCompressionSettings webSettings,
            IReadOnlyList<Texture> exclusions)
        {
            var textures = AssetProcessing.ScanAssets(roots, AssetKind.Textures);
            return OptimizeTextures(textures, webSettings, exclusions);
        }

        internal static string OptimizeTextures(IReadOnlyList<AssetEntry> textures,
            TexturePlatformCompressionSettings webSettings, IReadOnlyList<Texture> exclusions)
        {
            var excludedPaths = new HashSet<string>(StringComparer.Ordinal);
            foreach (var texture in exclusions)
            {
                if (texture != null)
                {
                    excludedPaths.Add(AssetDatabase.GetAssetPath(texture));
                }
            }

            int changed = 0;
            int excluded = 0;
            int unchanged = 0;
            int failed = 0;
            bool cancelled = false;
            try
            {
                for (int i = 0; i < textures.Count; i++)
                {
                    var path = textures[i].Path;
                    if (excludedPaths.Contains(path))
                    {
                        excluded++;
                        continue;
                    }

                    if (EditorUtility.DisplayCancelableProgressBar("Optimize textures", path,
                        (float)i / textures.Count))
                    {
                        cancelled = true;
                        break;
                    }

                    try
                    {
                        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                        if (importer == null)
                        {
                            unchanged++;
                            continue;
                        }

                        bool dirty = webSettings.Apply(importer);

                        if (dirty)
                        {
                            importer.SaveAndReimport();
                            changed++;
                        }
                        else
                        {
                            unchanged++;
                        }
                    }
                    catch (Exception exception)
                    {
                        failed++;
                        Debug.LogError("Texture compression failed for " + path + ": " + exception.Message);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            string result = cancelled ? "Cancelled. Completed changes are retained." : "Completed.";
            return result + " Updated: " + changed + "; excluded: " + excluded +
                "; unchanged: " + unchanged + "; errors: " + failed + ".";
        }
    }
}
