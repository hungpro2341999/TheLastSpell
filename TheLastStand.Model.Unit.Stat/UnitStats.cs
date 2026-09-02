using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Unit.Stat;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Status;
using TheLastStand.Serialization.Unit;

namespace TheLastStand.Model.Unit.Stat;

public abstract class UnitStats : ISerializable, IDeserializable
{
	public int InjuryStage { get; set; }

	public Dictionary<UnitStatDefinition.E_Stat, UnitStat> Stats { get; set; } = new Dictionary<UnitStatDefinition.E_Stat, UnitStat>(UnitStatDefinition.SharedStatComparer);

	public Dictionary<UnitStatDefinition.E_Stat, UnitStat>.KeyCollection StatsKeys => Stats.Keys;

	public Unit Unit { get; set; }

	public UnitStatsController UnitStatsController { get; private set; }

	public abstract DamageableType UnitType { get; }

	public UnitStats(UnitStatsController statsController, Unit unit)
	{
		UnitStatsController = statsController;
		Unit = unit;
	}

	public virtual void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedUnitStats serializedUnitStats = container as SerializedUnitStats;
		InjuryStage = serializedUnitStats.InjuryStage;
		Unit.StatusList.ForEach(delegate(TheLastStand.Model.Status.Status o)
		{
			UnitStatsController.OnStatusAdded(o);
		});
		serializedUnitStats.Stats.ForEach(delegate(SerializedUnitStat o)
		{
			UnitStatsController.InitStat(o);
		});
		if (saveVersion != -1 && saveVersion < 9 && SaveManager.GameSaveVersion >= 9)
		{
			TPSingleton<PlayableUnitManager>.Instance.Log($"Loading a v{saveVersion} save with the application being v{SaveManager.GameSaveVersion}, changing opportunistic, isolated and poison.", CLogLevel.DETAILED, forcePrintInUnity: true);
			UnitStatsController.IncreaseBaseStat(UnitStatDefinition.E_Stat.OpportunisticAttacks, 100f, includeChildStat: false, refreshHud: false);
			UnitStatsController.IncreaseBaseStat(UnitStatDefinition.E_Stat.IsolatedAttacks, 100f, includeChildStat: false, refreshHud: false);
			UnitStatsController.IncreaseBaseStat(UnitStatDefinition.E_Stat.PoisonDamageModifier, 100f, includeChildStat: false, refreshHud: false);
		}
	}

	public virtual ISerializedData Serialize()
	{
		return new SerializedUnitStats
		{
			Stats = Stats.Select((KeyValuePair<UnitStatDefinition.E_Stat, UnitStat> o) => o.Value.Serialize()).Cast<SerializedUnitStat>().ToList(),
			InjuryStage = InjuryStage
		};
	}
}
