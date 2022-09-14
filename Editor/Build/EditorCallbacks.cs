using UnityEditor;
using UnityEngine;

namespace SuperUnityBuild.BuildTool
{
    using System;

    [InitializeOnLoad]
    public static class EditorCallbacks
    {
        static EditorCallbacks()
        {
            EditorApplication.playModeStateChanged += PlaymodeStateChanged;
        }

        private static void PlaymodeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                BuildProject.GenerateVersionString(BuildSettings.productParameters, DateTime.Now, false);
            }
        }
    }
}
