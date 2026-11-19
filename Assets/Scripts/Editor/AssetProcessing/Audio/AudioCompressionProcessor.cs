using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectTools.Editor.AssetTools
{
    public static class AudioCompressionProcessor
    {
        public static string Process(string[] roots, AudioCompressionSettings settings,
            IReadOnlyList<AudioClip> exclusions, bool preview, Action<string, string> onAsset)
        {
            var entries = AssetProcessing.ScanAssets(roots, AssetKind.Audio);
            var excludedPaths = new HashSet<string>(StringComparer.Ordinal);
            foreach (var clip in exclusions)
            {
                if (clip != null)
                {
                    excludedPaths.Add(AssetDatabase.GetAssetPath(clip));
                }
            }

            var lines = new List<string>();
            int changed = 0;
            int unchanged = 0;
            int excluded = 0;
            int failed = 0;
            bool cancelled = false;
            try
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    string path = entries[i].Path;
                    if (EditorUtility.DisplayCancelableProgressBar("Compress audio", path, (float)i / entries.Count))
                    {
                        cancelled = true;
                        break;
                    }

                    try
                    {
                        string outcome;
                        if (excludedPaths.Contains(path))
                        {
                            excluded++;
                            outcome = "Excluded";
                        }
                        else
                        {
                            var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                            if (importer == null)
                            {
                                throw new InvalidOperationException("Audio importer is unavailable.");
                            }

                            outcome = settings.DescribeChange(importer);
                            if (!settings.NeedsChange(importer))
                            {
                                unchanged++;
                                outcome = "Already configured. " + outcome;
                            }
                            else
                            {
                                if (!preview)
                                {
                                    settings.Apply(importer);
                                    importer.SaveAndReimport();
                                    var imported = AssetImporter.GetAtPath(path) as AudioImporter;
                                    if (imported == null || settings.NeedsChange(imported))
                                    {
                                        throw new InvalidOperationException("Audio settings failed read-back verification.");
                                    }
                                }

                                changed++;
                                outcome = (preview ? "Would update. " : "Updated and verified. ") + outcome;
                            }
                        }

                        lines.Add(path + ": " + outcome);
                        onAsset?.Invoke(path, outcome);
                    }
                    catch (Exception exception)
                    {
                        failed++;
                        string error = "ERROR: " + exception.Message;
                        lines.Add(path + ": " + error);
                        onAsset?.Invoke(path, error);
                        Debug.LogError("Audio compression failed for " + path + ": " + exception.Message);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            string summary = (preview ? "Analysis (no import changes)" : "Completed") +
                ". " + (preview ? "Candidates: " : "Updated: ") + changed + "; unchanged: " + unchanged +
                "; excluded: " + excluded + "; errors: " + failed + "; cancelled: " + cancelled + ".";
            if (cancelled)
            {
                summary += " Completed changes are retained.";
            }

            return AssetProcessing.WriteReport("Audio", summary, lines);
        }
    }
}
