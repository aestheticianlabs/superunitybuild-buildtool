using System;
using System.Linq;
using UnityEngine;

namespace SuperUnityBuild.BuildTool
{
    public static class BuildCLI
    {
        /// <summary>
        /// Runs the provided SuperUnityBuild configuration keychains separated by semicolons
        /// (i.e. "Release/PC/Windows x86 (App); Release/macOS/macOS (Intelx64,App)")
        /// </summary>
        // todo: support partial chains (i.e. Release, Dev/macOS, etc)?
        public static void PerformBuild()
        {
            // clear notifications (if any)
            BuildNotificationList.instance.RefreshAll();

            // get build arguments
            var fullArgs = Environment.GetCommandLineArgs();

            // search full args for method name (where our args start)
            var start = 0;
            while (++start < fullArgs.Length)
            {
                if (fullArgs[start - 1] == "SuperUnityBuild.BuildTool.BuildCLI.PerformBuild")
                {
                    break;
                }
            }

            var args = new string[fullArgs.Length - start];
            Array.Copy(fullArgs, start, args, 0, args.Length);

            if (args.Length > 0) // build with args
            {
                Log(LogType.Log, $"Args: {string.Join(", ", args)}");

                if (args.Length > 1)
                {
                    Log(LogType.Warning, "Found more than 1 command line argument, all but first will be ignored.");
                }

                var buildConfigs = args[0].Split(';').Select(s => s.Trim(' ')).ToArray();
                Log(LogType.Log, $"Building with {buildConfigs.Length} configurations.");
                BuildProject.PerformBuild(buildConfigs);
            }
            else // build all if no args
            {
                Log(LogType.Log, "No args provided, building all configurations.");
                BuildProject.BuildAll();
            }

            // Set exit code to indicate success or failure
            Application.Quit(BuildNotificationList.instance.errors.Count > 0 ? 1 : 0);
        }

        public static void Log(LogType logType, string message)
        {
            Debug.unityLogger.Log(logType, "SuperUnityBuild CLI", message);
        }
    }
}
