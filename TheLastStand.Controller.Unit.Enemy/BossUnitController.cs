using TPLib;
using TheLastStand.Controller.Unit.Stat;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Serialization.Unit;
using TheLastStand.View.Unit;

namespace TheLastStand.Controller.Unit.Enemy;

public class BossUnitController : EnemyUnitController
{
	public BossUnit BossUnit => base.Unit as BossUnit;

	public BossUnitController(BossUnitTemplateDefinition bossUnitTemplateDefinition, UnitView view, Tile tile, UnitCreationSettings unitCreationSettings)
	{
		base.Unit = new BossUnit(bossUnitTemplateDefinition, this, view, unitCreationSettings)
		{
			State = TheLastStand.Model.Unit.Unit.E_State.Ready
		};
		Init(view, tile, unitCreationSettings.OverrideVariantId);
	}

	public BossUnitController(SerializedEnemyUnit serializedUnit, UnitView unitView, UnitCreationSettings unitCreationSettings, int saveVersion = -1)
	{
		base.Unit = new BossUnit(BossUnitDatabase.BossUnitTemplateDefinitions[serializedUnit.Id], serializedUnit, this, unitView, unitCreationSettings, saveVersion)
		{
			State = TheLastStand.Model.Unit.Unit.E_State.Ready
		};
		base.Unit.DeserializeAfterInit(serializedUnit, saveVersion);
		Init(serializedUnit, unitView, base.Unit.OriginTile, saveVersion);
	}

	protected override int GetRandomVisualVariantIndex()
	{
		return RandomManager.GetRandomRange(TPSingleton<BossManager>.Instance, 0, BossUnit.BossUnitTemplateDefinition.VisualVariants.Count);
	}

	protected override void InitStats()
	{
		if (BossUnit != null)
		{
			base.Unit.UnitStatsController = new BossUnitStatsController(BossUnit);
			base.Unit.UnitStatsController.SnapBaseStatTo(UnitStatDefinition.E_Stat.Health, UnitStatDefinition.E_Stat.HealthTotal);
			base.Unit.UnitStatsController.SnapBaseStatTo(UnitStatDefinition.E_Stat.Armor, UnitStatDefinition.E_Stat.ArmorTotal);
			base.Unit.UnitStatsController.SnapBaseStatTo(UnitStatDefinition.E_Stat.MovePoints, UnitStatDefinition.E_Stat.MovePointsTotal);
		}
	}

	protected override void OnDeath()
	{
		base.OnDeath();
		if (TPSingleton<BossManager>.Instance.BossUnits.Contains(BossUnit))
		{
			BossManager.DestroyUnit(BossUnit);
		}
	}
}
