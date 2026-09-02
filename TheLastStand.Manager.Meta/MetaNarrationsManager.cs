using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Debugging.Console;
using TheLastStand.Controller.Meta;
using TheLastStand.Database;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;
using TheLastStand.Model.Meta;
using TheLastStand.Serialization;

namespace TheLastStand.Manager.Meta;

public class MetaNarrationsManager : Manager<MetaNarrationsManager>, ISerializable, IDeserializable
{
	public static bool AnyValidMandatoryNarration
	{
		get
		{
			if (!LightNarration.MetaNarrationController.TryGetValidMandatoryReplica(1, out var replicas))
			{
				return DarkNarration.MetaNarrationController.TryGetValidMandatoryReplica(1, out replicas);
			}
			return true;
		}
	}

	public static MetaNarration DarkNarration { get; private set; }

	public static MetaNarration LightNarration { get; private set; }

	[DevConsoleCommand(Options = (DevConsoleCommandOptions.CanReadWrite | DevConsoleCommandOptions.ForceStatic))]
	public static bool NarrationDoneThisDay { get; set; } = false;

	[DevConsoleCommand(Options = (DevConsoleCommandOptions.CanReadWrite | DevConsoleCommandOptions.ForceStatic))]
	public static int VisualEvolutionForced { get; set; } = -1;

	public static IEnumerable<string> GetAllUsedReplicas()
	{
		return DarkNarration.AlreadyUsedReplicasIds.Concat(LightNarration.AlreadyUsedReplicasIds);
	}

	public static void OnTurnStart()
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day && TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production)
		{
			NarrationDoneThisDay = false;
		}
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		if (container is SerializedNarrations serializedNarrations)
		{
			DarkNarration = new MetaNarrationController(MetaNarrationDatabase.DarkGoddessNarrationDefinition, serializedNarrations.DarkNarration).MetaNarration;
			LightNarration = new MetaNarrationController(MetaNarrationDatabase.LightGoddessNarrationDefinition, serializedNarrations.LightNarration).MetaNarration;
			NarrationDoneThisDay = serializedNarrations.NarrationDoneThisDay;
		}
		else
		{
			DarkNarration = new MetaNarrationController(MetaNarrationDatabase.DarkGoddessNarrationDefinition).MetaNarration;
			LightNarration = new MetaNarrationController(MetaNarrationDatabase.LightGoddessNarrationDefinition).MetaNarration;
		}
		DarkNarration.IsDarkOne = true;
		LightNarration.IsDarkOne = false;
	}

	public void Reset()
	{
		NarrationDoneThisDay = false;
		DarkNarration = new MetaNarrationController(MetaNarrationDatabase.DarkGoddessNarrationDefinition).MetaNarration;
		LightNarration = new MetaNarrationController(MetaNarrationDatabase.LightGoddessNarrationDefinition).MetaNarration;
		DarkNarration.IsDarkOne = true;
		LightNarration.IsDarkOne = false;
	}

	public ISerializedData Serialize()
	{
		return new SerializedNarrations
		{
			DarkNarration = (DarkNarration?.Serialize() as SerializedNarration),
			LightNarration = (LightNarration?.Serialize() as SerializedNarration),
			NarrationDoneThisDay = NarrationDoneThisDay
		};
	}

	[DevConsoleCommand("NarrationForceDisplayGoddessName")]
	public static void DebugNarrationForceDisplayGoddessName()
	{
		if (!LightNarration.AlreadyUsedReplicasIds.Contains(LightNarration.MetaNarrationDefinition.NameRevealDialogueId))
		{
			LightNarration.AlreadyUsedReplicasIds.Add(LightNarration.MetaNarrationDefinition.NameRevealDialogueId);
		}
		if (!DarkNarration.AlreadyUsedReplicasIds.Contains(DarkNarration.MetaNarrationDefinition.NameRevealDialogueId))
		{
			DarkNarration.AlreadyUsedReplicasIds.Add(DarkNarration.MetaNarrationDefinition.NameRevealDialogueId);
		}
	}
}
