using System;
using TPLib.Log;
using TheLastStand.Model.LaunchArgument;
using UnityEngine;

namespace TheLastStand.Manager;

public class LaunchArgumentsManager : Manager<LaunchArgumentsManager>
{
	[SerializeField]
	private string editorArgs = string.Empty;

	private static bool initialized;

	protected override void Awake()
	{
		if (!initialized)
		{
			base.Awake();
			ReadAndActivateLaunchArguments();
			initialized = true;
		}
	}

	private void ReadAndActivateLaunchArguments()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		if (commandLineArgs.Length != 0)
		{
			Log("Launch Arguments :", CLogLevel.DETAILED);
		}
		string[] array = commandLineArgs;
		foreach (string text in array)
		{
			if (text != null && text == "-dev")
			{
				Log("* -dev", CLogLevel.DETAILED);
				DevArgument.ActivateLaunchArgument();
			}
		}
	}
}
