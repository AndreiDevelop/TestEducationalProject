using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectTools.Editor.AssetTools
{
    public static class AssetProcessing
    {
        public static string ReportRoot => Path.Combine(Application.dataPath, "..", "Reports", "AssetProcessing");

        public static string NormalizeFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Choose an existing folder inside this project's Assets.");
            }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string full = Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(projectRoot, path));
            string assets = Path.GetFullPath(Application.dataPath);
            if (full == assets)
            {
                return "Assets";
            }

            if (!full.StartsWith(assets + Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
                !Directory.Exists(full))
            {
                throw new ArgumentException("Choose an existing folder inside this project's Assets.");
            }

            return "Assets/" + full.Substring(assets.Length + 1).Replace('\\', '/');
        }

        public static List<AssetEntry> ScanAssets(string[] roots, AssetKind kind)
        {
            var paths = new SortedSet<string>(StringComparer.Ordinal);
            foreach (string root in roots)
            {
                string normalized = NormalizeFolder(root);
                foreach (string guid in AssetDatabase.FindAssets("", new[] { normalized }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!path.Contains("/Editor/") && !AssetDatabase.IsValidFolder(path))
                    {
                        paths.Add(path);
                    }
                }
            }

            var entries = new List<AssetEntry>();
            foreach (string path in paths)
            {
                AssetImporter importer = AssetImporter.GetAtPath(path);
                string description;
                if (kind == AssetKind.Textures && importer is TextureImporter texture)
                {
                    var web = texture.GetPlatformTextureSettings("WebGL");
                    description = texture.textureType + " | Web: " + web.format + ", max " + web.maxTextureSize;
                }
                else if (kind == AssetKind.Models && importer is ModelImporter model)
                {
                    description = "Mesh optimization: " + model.optimizeMeshVertices + "/" +
                        model.optimizeMeshPolygons + " | Read/Write: " + model.isReadable;
                }
                else if (kind == AssetKind.Audio && importer is AudioImporter audio)
                {
                    description = "Default: " + AudioCompressionSettings.Describe(audio.defaultSampleSettings);
                    if (audio.ContainsSampleSettingsOverride("WebGL"))
                    {
                        description += " | Web: " +
                            AudioCompressionSettings.Describe(audio.GetOverrideSampleSettings("WebGL"));
                    }

                    description += " | Mono: " + audio.forceToMono;
                }
                else
                {
                    continue;
                }

                entries.Add(new AssetEntry(path, new FileInfo(path).Length, description));
            }

            return entries;
        }

        public static string OptimizeModels(string[] roots, bool preview, bool disableReadWrite,
            Action<string, string> onAsset)
        {
            var entries = ScanAssets(roots, AssetKind.Models);
            var lines = new List<string>();
            int changed = 0;
            int failed = 0;
            bool cancelled = false;
            try
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    string path = entries[i].Path;
                    if (EditorUtility.DisplayCancelableProgressBar("Optimize models", path, (float)i / entries.Count))
                    {
                        cancelled = true;
                        break;
                    }

                    try
                    {
                        var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                        if (importer == null)
                        {
                            throw new InvalidOperationException("Model importer is unavailable.");
                        }

                        bool dirty = !importer.optimizeMeshVertices || !importer.optimizeMeshPolygons ||
                            (disableReadWrite && importer.isReadable);
                        string outcome = "Already configured";
                        if (dirty)
                        {
                            outcome = preview ? "Would optimize mesh" : "Optimized mesh";
                            if (!preview)
                            {
                                importer.optimizeMeshVertices = true;
                                importer.optimizeMeshPolygons = true;
                                if (disableReadWrite)
                                {
                                    importer.isReadable = false;
                                }

                                importer.SaveAndReimport();
                            }

                            changed++;
                        }

                        lines.Add(path + ": " + outcome);
                        onAsset?.Invoke(path, outcome);
                    }
                    catch (Exception exception)
                    {
                        failed++;
                        lines.Add(path + ": ERROR " + exception.Message);
                        onAsset?.Invoke(path, "ERROR: " + exception.Message);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            string summary = (preview ? "Analysis" : "Completed") + ": " + changed +
                " candidates/updated; errors: " + failed + "; cancelled: " + cancelled + ".";
            return WriteReport("Models", summary, lines);
        }

        public static string WriteReport(string label, string summary, List<string> lines)
        {
            Directory.CreateDirectory(ReportRoot);
            string file = Path.Combine(ReportRoot, DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-" +
                label + "-" + Guid.NewGuid().ToString("N").Substring(0, 6) + ".txt");
            lines.Insert(0, summary);
            File.WriteAllLines(file, lines);
            return summary + "\nReport: " + Path.GetFullPath(file);
        }
    }
}
