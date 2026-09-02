using Rewired;
using TPLib;
using TPLib.Debugging;
using UnityEngine;

namespace TheLastStand.Manager;

public class TimeManager : TPSingleton<TimeManager>
{
	private static class Debug_Consts
	{
		public static string DebugPlayerId = "Debug";
	}

	[SerializeField]
	[Range(1f, 20f)]
	private float debugTimeScaleWhenSpeedUp = 10f;

	[SerializeField]
	[Range(0f, 1f)]
	private float debugTimeScaleWhenSpeedDown = 0.2f;

	public bool IsModifyingTimeScale { get; private set; }

	private void Start()
	{
		Debug_Start();
	}

	private void Debug_Start()
	{
		Player player = ReInput.players.GetPlayer(Debug_Consts.DebugPlayerId);
		player.AddInputEventDelegate(Debug_OnDebugSpeedUpButtonJustPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, 49);
		player.AddInputEventDelegate(Debug_OnDebugSpeedUpButtonJustReleased, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, 49);
		player.AddInputEventDelegate(Debug_OnDebugSpeedDownButtonJustPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, 51);
		player.AddInputEventDelegate(Debug_OnDebugSpeedDownButtonJustReleased, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, 51);
	}

	private void Debug_OnDebugSpeedDownButtonJustPressed(InputActionEventData data)
	{
		if (DebugManager.DebugMode && Mathf.Abs(Time.timeScale - 1f) < Mathf.Epsilon)
		{
			Time.timeScale = debugTimeScaleWhenSpeedDown;
			IsModifyingTimeScale = true;
		}
	}

	private void Debug_OnDebugSpeedDownButtonJustReleased(InputActionEventData data)
	{
		Time.timeScale = 1f;
		IsModifyingTimeScale = false;
	}

	private void Debug_OnDebugSpeedUpButtonJustPressed(InputActionEventData data)
	{
		if (DebugManager.DebugMode && Mathf.Abs(Time.timeScale - 1f) < Mathf.Epsilon)
		{
			Time.timeScale = debugTimeScaleWhenSpeedUp;
			IsModifyingTimeScale = true;
		}
	}

	private void Debug_OnDebugSpeedUpButtonJustReleased(InputActionEventData data)
	{
		Time.timeScale = 1f;
		IsModifyingTimeScale = false;
	}
}
