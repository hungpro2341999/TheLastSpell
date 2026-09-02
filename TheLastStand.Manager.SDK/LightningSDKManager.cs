using System;
using System.Collections;
using System.Collections.Generic;
using AsusAuraWrapper;
using TPLib;
using TheLastStand.Framework.Automaton;
using TheLastStand.Framework.Extensions;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Manager.SDK;

public class LightningSDKManager : Manager<LightningSDKManager>
{
	public enum SDKEvent
	{
		MAIN_MENU,
		WORLDMAP,
		PRODUCTION,
		DEPLOYMENT,
		PLAYER_TURN,
		ENEMY_TURN,
		DARK_SHOP,
		LIGHT_SHOP,
		HUB_SHOP
	}

	[Serializable]
	public struct ColorEventPair
	{
		public SDKEvent Event;

		public DataColor Color;
	}

	private AsusAuraService asusAuraService;

	private Color32 currentColor = Color.black;

	private Coroutine currentCoroutine;

	[SerializeField]
	private List<ColorEventPair> colorPerEvent = new List<ColorEventPair>();

	[SerializeField]
	private AnimationCurve flashCurve;

	[SerializeField]
	[Range(0.05f, 1f)]
	private float flashDuration = 0.1f;

	[SerializeField]
	[Range(0.05f, 2f)]
	private float transitionDuration = 0.5f;

	public void HandleApplicationStateColor(State state, float? duration = null)
	{
		switch (state.GetName())
		{
		case "GameLobby":
			TryTransitionToColorEvent(SDKEvent.MAIN_MENU, duration);
			break;
		case "WorldMap":
			TryTransitionToColorEvent(SDKEvent.WORLDMAP, duration);
			break;
		case "MetaShops":
			break;
		}
	}

	public void HandleGameCycleColor(float? duration = null)
	{
		switch (TPSingleton<GameManager>.Instance.Game.Cycle)
		{
		case Game.E_Cycle.Day:
			switch (TPSingleton<GameManager>.Instance.Game.DayTurn)
			{
			case Game.E_DayTurn.Production:
				TryTransitionToColorEvent(SDKEvent.PRODUCTION, duration);
				break;
			case Game.E_DayTurn.Deployment:
				TryTransitionToColorEvent(SDKEvent.DEPLOYMENT, duration);
				break;
			}
			break;
		case Game.E_Cycle.Night:
			switch (TPSingleton<GameManager>.Instance.Game.NightTurn)
			{
			case Game.E_NightTurn.EnemyUnits:
				TryTransitionToColorEvent(SDKEvent.ENEMY_TURN, duration);
				break;
			case Game.E_NightTurn.PlayableUnits:
				TryTransitionToColorEvent(SDKEvent.PLAYER_TURN, duration);
				break;
			}
			break;
		}
	}

	public void HandleMetaShopTransition(SDKEvent metaShopTarget, float? duration = null)
	{
		switch (metaShopTarget)
		{
		case SDKEvent.DARK_SHOP:
		case SDKEvent.LIGHT_SHOP:
			TryTransitionToColorEvent(metaShopTarget, duration);
			break;
		case SDKEvent.HUB_SHOP:
			TransitionToColor(Color.black, duration);
			break;
		}
	}

	public void OnApplicationStateChange(State state)
	{
		HandleApplicationStateColor(state);
	}

	public void SetAllLightsToColor(uint r, uint g, uint b, uint a)
	{
		asusAuraService?.SetAllLightsToColor(r, g, b, a);
	}

	public void SetAllLightsToColor(Color32 color)
	{
		SetAllLightsToColor(color.r, color.g, color.b, color.a);
	}

	public void TransitionToColor(Color32 targetColor, float? duration = null)
	{
		StopCurrentCoroutineIfNeeded();
		currentCoroutine = StartCoroutine(ColorTransitionCoroutine(currentColor, targetColor, duration ?? transitionDuration));
	}

	public void TriggerFlashEffect(bool backToCurrentColorOnEnd = true)
	{
		StopCurrentCoroutineIfNeeded();
		currentCoroutine = StartCoroutine(FlashCoroutine(backToCurrentColorOnEnd));
	}

	public bool TryTransitionToColorEvent(SDKEvent SDKEventTriggered, float? duration = null)
	{
		if (colorPerEvent.TryFind((ColorEventPair x) => x.Event == SDKEventTriggered, out var value))
		{
			TransitionToColor(value.Color._Color, duration);
			return true;
		}
		return false;
	}

	protected override void Awake()
	{
		base.Awake();
		if (base._IsValid)
		{
			ApplicationManager.Application.ApplicationController.ApplicationStateChangeEvent += OnApplicationStateChange;
			HandleApplicationStateColor(ApplicationManager.Application.State);
		}
	}

	private IEnumerator ColorTransitionCoroutine(Color32 initialColor, Color32 targetColor, float duration)
	{
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			currentColor = Color.Lerp(initialColor, targetColor, elapsedTime / duration);
			SetAllLightsToColor(currentColor);
			yield return null;
		}
		currentCoroutine = null;
	}

	private IEnumerator FlashCoroutine(bool backToCurrentColorOnEnd = true)
	{
		float elapsedTime = 0f;
		Color32 currentFlashColor = Color.black;
		while (elapsedTime < flashDuration)
		{
			elapsedTime += Time.deltaTime;
			currentFlashColor = Color.white * flashCurve.Evaluate(elapsedTime / flashDuration);
			SetAllLightsToColor(currentFlashColor);
			yield return null;
		}
		if (backToCurrentColorOnEnd)
		{
			yield return ColorTransitionCoroutine(currentFlashColor, currentColor, transitionDuration);
		}
		else
		{
			currentColor = currentFlashColor;
		}
	}

	private void OnApplicationQuit()
	{
		asusAuraService?.ExitSDKMode();
	}

	private void StopCurrentCoroutineIfNeeded()
	{
		if (currentCoroutine != null)
		{
			StopCoroutine(currentCoroutine);
		}
	}
}
