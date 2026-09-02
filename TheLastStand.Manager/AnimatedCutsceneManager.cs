using System;
using System.Collections;
using TPLib;
using TheLastStand.View;

namespace TheLastStand.Manager;

public class AnimatedCutsceneManager : Manager<AnimatedCutsceneManager>
{
	public class Constants
	{
		public const string Introduction = "Introduction";
	}

	private string currentCutsceneId;

	private string followingStateName;

	private Action callbackOnOver;

	private bool changeState = true;

	public static string FollowingStateName
	{
		get
		{
			if (!string.IsNullOrEmpty(TPSingleton<AnimatedCutsceneManager>.Instance.followingStateName))
			{
				return TPSingleton<AnimatedCutsceneManager>.Instance.followingStateName;
			}
			return "NewGame";
		}
	}

	public string CurrentCutsceneId => currentCutsceneId;

	public static IEnumerator OnIntroductionOver()
	{
		if (TPSingleton<AnimatedCutsceneManager>.Instance.changeState)
		{
			ApplicationManager.Application.ApplicationController.SetState(FollowingStateName);
		}
		else
		{
			TPSingleton<AnimatedCutsceneManager>.Instance.callbackOnOver?.Invoke();
		}
		TPSingleton<AnimatedCutsceneView>.Instance.RemoveAnimatedCutscene();
		yield return TPSingleton<AnimatedCutsceneView>.Instance.Fade(fadeIn: false);
	}

	public static void PlayPostVictoryAnimatedCutscene(string cutsceneId, Action endCallback = null)
	{
		TPSingleton<AnimatedCutsceneManager>.Instance.currentCutsceneId = cutsceneId;
		TPSingleton<AnimatedCutsceneManager>.Instance.callbackOnOver = endCallback;
		TPSingleton<AnimatedCutsceneManager>.Instance.changeState = false;
		TPSingleton<AnimatedCutsceneView>.Instance.StartAnimatedCutScene(cutsceneId);
	}

	public static IEnumerator PlayAnimatedCutsceneWithFade(string cutsceneId)
	{
		yield return TPSingleton<AnimatedCutsceneView>.Instance.Fade(fadeIn: true);
		TPSingleton<AnimatedCutsceneManager>.Instance.currentCutsceneId = cutsceneId;
		TPSingleton<AnimatedCutsceneManager>.Instance.changeState = false;
		yield return TPSingleton<AnimatedCutsceneView>.Instance.StartAnimatedCutSceneCoroutine(cutsceneId);
	}

	public static void PlayPreGameAnimatedCutscene(string followingStateName, string cutsceneId = "Introduction")
	{
		TPSingleton<AnimatedCutsceneManager>.Instance.currentCutsceneId = cutsceneId;
		TPSingleton<AnimatedCutsceneManager>.Instance.followingStateName = followingStateName;
		TPSingleton<AnimatedCutsceneManager>.Instance.changeState = true;
		ApplicationManager.Application.ApplicationController.SetState("AnimatedCutscene");
	}
}
