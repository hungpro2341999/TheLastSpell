using System;
using System.Collections;
using TPLib;
using TPLib.Localization;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Controller;
using TheLastStand.Controller.SplashScreen;
using TheLastStand.Framework.Database;
using TheLastStand.Manager.Modding;
using TheLastStand.Model.SplashScreen;
using UnityEngine;

namespace TheLastStand.Manager;

public class SplashScreenManager : Manager<SplashScreenManager>
{
	public static class Constants
	{
		public const string FadeOutAnimatorTrigger = "FadeOut";
	}

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private AnimationClip fadeOutAnimation;

	[SerializeField]
	private GameObject initializedText;

	[SerializeField]
	private MainMenuSceneLoader splashScreenSceneLoader;

	[SerializeField]
	[Range(0f, 10f)]
	private float splashScreenDuration;

	[SerializeField]
	[Range(0f, 5f)]
	private float delayBeforeDisplayingInitializationLabel = 1f;

	[SerializeField]
	private string currentStateName = string.Empty;

	public static string CurrentStateName
	{
		get
		{
			return TPSingleton<SplashScreenManager>.Instance.currentStateName;
		}
		set
		{
			TPSingleton<SplashScreenManager>.Instance.currentStateName = value;
		}
	}

	public static SplashScreen SplashScreen { get; private set; }

	public bool SplashScreenIsOver { get; set; }

	public MainMenuSceneLoader SplashScreenSceneLoader => splashScreenSceneLoader;

	public static string GetArg(string name)
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i] == name && commandLineArgs.Length > i + 1)
			{
				return commandLineArgs[i + 1];
			}
		}
		return null;
	}

	protected override void Awake()
	{
		base.Awake();
		_ = Localizer.dictionary;
		SplashScreen = new SplashScreenController().SplashScreen;
		StartCoroutine(StartInitializationSequence());
		StartCoroutine(StartSplashScreenAnimation());
	}

	private IEnumerator StartInitializationSequence()
	{
		while (!TPSingleton<DatabaseLoader>.Instance.Initialized)
		{
			yield return SharedYields.WaitForEndOfFrame;
		}
		TPSingleton<ModManager>.Instance.OnModsLoadedEvent += OnModsLoaded;
		SplashScreen.SplashScreenController.SetState("ModsLoading");
	}

	private void OnModsLoaded()
	{
		TPSingleton<ModManager>.Instance.OnModsLoadedEvent -= OnModsLoaded;
		TPSingleton<SettingsManager>.Instance.OnSettingsDeserializedEvent += OnSettingsDeserialized;
		SplashScreen.SplashScreenController.SetState("SettingsLoading");
	}

	private void OnSettingsDeserialized()
	{
		TPSingleton<SettingsManager>.Instance.OnSettingsDeserializedEvent -= OnSettingsDeserialized;
		SplashScreen.SplashScreenController.SetState("MainMenuLoading");
	}

	private IEnumerator StartSplashScreenAnimation()
	{
		yield return null;
		yield return SharedYields.WaitForSeconds(splashScreenDuration);
		animator.SetTrigger("FadeOut");
		yield return SharedYields.WaitForEndOfFrame;
		yield return SharedYields.WaitForSeconds(fadeOutAnimation.length + 0.1f);
		SplashScreenIsOver = true;
		Log("SplashScreen animation is over !", CLogLevel.DETAILED);
		yield return SharedYields.WaitForSeconds(delayBeforeDisplayingInitializationLabel);
		if (CurrentStateName != "MainMenuLoading")
		{
			Log("Display Initializing text !", CLogLevel.DETAILED);
			initializedText.SetActive(value: true);
		}
	}
}
