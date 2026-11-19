using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectTools.Editor.AssetTools
{
    public abstract class AssetProcessingWindow : EditorWindow
    {
        protected Vector2 _scroll;
        protected string _status = "";

        protected virtual void OnEnable()
        {
        }

        protected void ShowStatus()
        {
            if (!string.IsNullOrEmpty(_status))
            {
                EditorGUILayout.HelpBox(_status, MessageType.Info);
            }

            if (GUILayout.Button("Open reports"))
            {
                Directory.CreateDirectory(AssetProcessing.ReportRoot);
                EditorUtility.RevealInFinder(AssetProcessing.ReportRoot);
            }
        }
    }
}
