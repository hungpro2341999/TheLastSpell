using TPLib;
using TheLastStand.Controller.Unit.Stat;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Serialization.Unit;
using TheLastStand.View.Unit;

namespace TheLastStand.Controller.Unit.Enemy;

public class EliteEnemyUnitController : EnemyUnitController
{
	public EliteEnemyUnit EliteEnemyUnit => base.Unit as EliteEnemyUnit;

	public EliteEnemyUnitController(EliteEnemyUnitTemplateDefinition eliteEnemyUnitTemplateDefinition, UnitView view, Tile tile, UnitCreationSettings unitCreationSettings, EnemyAffixDefinition enemyAffixDefinition = null)
	{
		base.Unit = new EliteEnemyUnit(eliteEnemyUnitTemplateDefinition, this, view, unitCreationSettings, enemyAffixDefinition)
		{
			State = TheLastStand.Model.Unit.Unit.E_State.Ready
		};
		Init(view, tile, unitCreationSettings.OverrideVariantId);
	}

	public EliteEnemyUnitController(SerializedEliteEnemyUnit serializedUnit, UnitView unitView, UnitCreationSettings unitCreationSettings, int saveVersion = -1)
	{
		base.Unit = new EliteEnemyUnit(EnemyUnitDatabase.EliteEnemyUnitTemplateDefinitions[serializedUnit.Id], serializedUnit, this, unitView, unitCreationSettings, saveVersion)
		{
			State = TheLastStand.Model.Unit.Unit.E_State.Ready
		};
		base.Unit.DeserializeAfterInit(serializedUnit, saveVersion);
		Init(serializedUnit, unitView, base.Unit.OriginTile, saveVersion);
	}

	protected override void AddNewKillReportEntry(ISkillCaster killer)
	{
		int num = -1;
		for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.NightReport.KillsThisNight.Count; i++)
		{
			if (TPSingleton<PlayableUnitManager>.Instance.NightReport.KillsThisNight[i].SpecificId == EliteEnemyUnit.EliteEnemyUnitTemplateDefinition.Id)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			TPSingleton<PlayableUnitManager>.Instance.NightReport.KillsThisNight.Insert(num + 1, new KillReportData(EliteEnemyUnit, killer as IEntity));
		}
		else
		{
			TPSingleton<PlayableUnitManager>.Instance.NightReport.KillsThisNight.Add(new KillReportData(EliteEnemyUnit, killer as IEntity));
		}
	}

	protected override void InitStats()
	{
		base.Unit.UnitStatsController = new EliteEnemyUnitStatsController(EliteEnemyUnit);
	}

	protected override void InitVariantId()
	{
		base.EnemyUnit.VariantId = "01";
	}
}
