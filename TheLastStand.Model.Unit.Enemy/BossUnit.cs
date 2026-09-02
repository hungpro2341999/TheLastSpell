using System.Collections.Generic;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Controller.Unit;
using TheLastStand.Controller.Unit.Enemy;
using TheLastStand.Controller.Unit.Stat;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Manager.Unit;
using TheLastStand.Serialization.Unit;
using TheLastStand.View.Unit;

namespace TheLastStand.Model.Unit.Enemy;

public class BossUnit : EnemyUnit
{
	public class StringToBossUnitTemplateIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(BossUnitDatabase.BossUnitTemplateDefinitions.Keys);
	}

	public BossUnitController BossUnitController => base.UnitController as BossUnitController;

	public BossUnitStatsController BossUnitStatsController => base.UnitStatsController as BossUnitStatsController;

	public BossUnitTemplateDefinition BossUnitTemplateDefinition => base.UnitTemplateDefinition as BossUnitTemplateDefinition;

	public bool AlwaysPlayDeathCutscene
	{
		get
		{
			if (HasDeathCutscene)
			{
				return BossUnitTemplateDefinition.AlwaysPlayDeathCutscene;
			}
			return false;
		}
	}

	public bool HasDeathCutscene => BossUnitTemplateDefinition.DeathCutsceneId != null;

	public override string Name => Localizer.Get("BossName_" + base.EnemyUnitTemplateDefinition.Id);

	public BossUnit(UnitTemplateDefinition unitTemplateDefinition, UnitController unitController, UnitView unitView, UnitCreationSettings unitCreationSettings)
		: base(unitTemplateDefinition, unitController, unitView, unitCreationSettings)
	{
	}

	public BossUnit(UnitTemplateDefinition unitTemplateDefinition, SerializedEnemyUnit serializedUnit, UnitController unitController, UnitView unitView, UnitCreationSettings unitCreationSettings, int saveVersion)
		: base(unitTemplateDefinition, serializedUnit, unitController, unitView, unitCreationSettings, saveVersion)
	{
	}

	public override void Log(object message, CLogLevel logLevel = CLogLevel.NORMAL, bool forcePrintInUnity = false, bool printStackTrace = false)
	{
		TPSingleton<BossManager>.Instance.Log($"[{UniqueIdentifier}]: {message}", UnitView, logLevel, forcePrintInUnity, printStackTrace);
	}

	public override void LogError(object message, CLogLevel logLevel = CLogLevel.NORMAL, bool forcePrintInUnity = true, bool printStackTrace = true)
	{
		TPSingleton<BossManager>.Instance.LogError($"[{UniqueIdentifier}]: {message}", UnitView, logLevel, forcePrintInUnity, printStackTrace);
	}

	public override void LogWarning(object message, CLogLevel logLevel = CLogLevel.NORMAL, bool forcePrintInUnity = true, bool printStackTrace = false)
	{
		TPSingleton<BossManager>.Instance.LogWarning($"[{UniqueIdentifier}]: {message}", UnitView, logLevel, forcePrintInUnity, printStackTrace);
	}
}
