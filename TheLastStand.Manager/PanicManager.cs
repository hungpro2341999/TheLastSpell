using System.Collections.Generic;
using TPLib;
using TPLib.Debugging;
using TPLib.Debugging.Console;
using TheLastStand.Controller.Panic;
using TheLastStand.Database;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;
using TheLastStand.Model.Panic;
using TheLastStand.Serialization;
using TheLastStand.View.Panic;
using UnityEngine;

namespace TheLastStand.Manager;

public class PanicManager : Manager<PanicManager>, ISerializable, IDeserializable
{
	[SerializeField]
	private PanicView panicView;

	private Panic panic;

	private bool initialized;

	public static Panic Panic => TPSingleton<PanicManager>.Instance.panic;

	public List<int> GoldValues => PanicDatabase.PanicDefinition.GoldValues;

	public List<int> MaterialValues => PanicDatabase.PanicDefinition.MaterialValues;

	public void Init()
	{
		if (!TPSingleton<PanicManager>.Instance.initialized)
		{
			TPSingleton<PanicManager>.Instance.initialized = true;
			Awake();
			TPSingleton<PanicManager>.Instance.panic = new PanicController(PanicDatabase.PanicDefinition, TPSingleton<PanicManager>.Instance.panicView).Panic;
		}
	}

	public static void StartTurn()
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits)
		{
			Panic.PanicController.ComputeExpectedValue(updateView: true);
		}
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		if (container is SerializedPanic serializedPanic)
		{
			panic.PanicController.SetValue(serializedPanic.Value);
			panic.PanicController.ComputeExpectedValue(updateView: true);
		}
	}

	public ISerializedData Serialize()
	{
		return new SerializedPanic
		{
			Value = panic.Value
		};
	}

	[DevConsoleCommand("AddPanic")]
	public static void DebugAddPanic(int amount = 10)
	{
		TPSingleton<PanicManager>.Instance.panic.PanicController.AddValue(amount);
	}

	[DevConsoleCommand("PanicGet")]
	public static void DebugGetPanic()
	{
		TPSingleton<DebugManager>.Instance.LogDevConsole($"Panic value = {Panic.Value}/{Panic.PanicDefinition.ValueMax}");
	}

	[DevConsoleCommand("RemoveAdd")]
	public static void DebugRemovePanic(int amount = 10)
	{
		TPSingleton<PanicManager>.Instance.panic.PanicController.RemoveValue(amount);
	}

	[DevConsoleCommand("SetPanic")]
	public static void DebugSetPanic(int amount)
	{
		TPSingleton<PanicManager>.Instance.panic.PanicController.SetValue(amount);
	}

	[DevConsoleCommand("RerollRewardAdd")]
	public static void DebugAddRerollReward(int amount = 10)
	{
		TPSingleton<PanicManager>.Instance.panic.PanicReward.RemainingNbRerollReward += amount;
	}
}
