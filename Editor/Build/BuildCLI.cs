using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace SuperUnityBuild.BuildTool
{
	public static class BuildCLI
	{
		private static string[] GetArgs([CallerMemberName] string callerMemberName = "")
		{
			if(string.IsNullOrEmpty(callerMemberName))
			{
				Log(LogType.Error, "Can't get args. Caller member name is missing.");
				return null;
			}

			// get build arguments
			var fullArgs = Environment.GetCommandLineArgs();

			// search full args for method name (where our args start)
			var start = 0;
			while (++start < fullArgs.Length)
			{
				if (fullArgs[start - 1] == $"SuperUnityBuild.BuildTool.BuildCLI.{callerMemberName}")
				{
					break;
				}
			}

			var args = new string[fullArgs.Length - start];
			Array.Copy(fullArgs, start, args, 0, args.Length);

			return args;
		}

		/// <summary>
		/// Configures the editor to use the provided SuperUnityBuild configuration keychain
		/// (i.e. "Release/PC/Windows x86 (App)")
		/// </summary>
		public static void ConfigureEditorEnvironment()
		{
			var args = GetArgs();

			if (args.Length <= 0)
			{
				// no args provided, do nothing
				Log(LogType.Log, "No args provided. Not doing anything.");
				return;
			}

			Log(LogType.Log, $"Args: {string.Join(", ", args)}");

			if (args.Length > 1)
			{
				Log(LogType.Warning, "Found more than 1 command line argument, all but first will be ignored.");
			}

			var config = args[0].Trim(' ');

			if (!BuildSettings.projectConfigurations.ParseKeychain(
				    config, out var releaseType,
				    out _, out _, out _, out _
			    ))
			{
				Log(LogType.Log, $"Could not parse configuration {config}");
			}

			Log(LogType.Log, $"Configuring editor with configuration {config}");

			// Update Editor environment settings to match selected build configuration
			BuildProject.ConfigureEditor(config);

			// Apply scene list
			BuildProject.SetEditorBuildSettingsScenes(releaseType);
		}

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
			var args = GetArgs();

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

			// count valid errors
			var errorCount = BuildNotificationList.instance.errors.Count(error => error.valid == null || error.valid());

			LogNotifications();

			// Set exit code to indicate success or failure
			EditorApplication.Exit(errorCount > 0 ? 1 : 0);
		}

		private static void LogNotifications()
		{
			void LogList(List<BuildNotification> list)
			{
				foreach (var notif in list.Where(notif => notif.valid == null || notif.valid()))
				{
					var logType = notif.cat switch
					{
						BuildNotification.Category.Notification => LogType.Log,
						BuildNotification.Category.Warning => LogType.Warning,
						BuildNotification.Category.Error => LogType.Error,
						_ => throw new ArgumentOutOfRangeException()
					};

					Debug.unityLogger.Log(logType, notif.title, notif.details);
				}
			}

			Log(LogType.Log, "Build notifications:");
			LogList(BuildNotificationList.instance.errors);
			LogList(BuildNotificationList.instance.warnings);
			LogList(BuildNotificationList.instance.notifications);
		}

		private static void Log(LogType logType, string message)
		{
			Debug.unityLogger.Log(logType, "SuperUnityBuild CLI", message);
		}
	}
}