using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Debugging.Console;
using TheLastStand.Manager.Building;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Enemy;

namespace TheLastStand.Manager.Turret;

public class TurretManager : ABuildingBehaviorManager<TurretManager>
{
	public List<TheLastStand.Model.Building.Building> Turrets => TPSingleton<BuildingManager>.Instance.Buildings.Where((TheLastStand.Model.Building.Building x) => x.IsTurret).ToList();

	public override List<IBehaviorModel> BehaviorModels => ((IEnumerable<IBehaviorModel>)Turrets.Select((TheLastStand.Model.Building.Building turret) => turret.BattleModule)).ToList();

	public static void StartTurn()
	{
		for (int i = 0; i < TPSingleton<TurretManager>.Instance.Turrets.Count; i++)
		{
			TPSingleton<TurretManager>.Instance.Turrets[i].BuildingController.StartTurn();
		}
	}

	public override List<IBehaviorModel> SortBehaviors(List<IBehaviorModel> skillCasters, bool shuffleList = true)
	{
		new List<IBehaviorModel>();
		return skillCasters.OrderBy((IBehaviorModel caster) => caster.BehaviourDefinition.GoalsComputingOrder).ThenBy(delegate(IBehaviorModel caster)
		{
			int num = 0;
			Goal[] goals = caster.Goals;
			foreach (Goal goal in goals)
			{
				goal.Skill.SkillAction.SkillActionExecution.Caster = caster;
				goal.Skill.SkillAction.SkillActionExecution.SkillSourceTileObject = caster;
				num += goal.Skill.SkillAction.SkillActionExecution.InRangeTiles.Range.Count((KeyValuePair<Tile, TilesInRangeInfos.TileDisplayInfos> tile) => goal.Skill.SkillController.IsValidatingTargetingConstraints(tile.Key));
			}
			return num;
		}).ThenBy((IBehaviorModel _) => (!shuffleList) ? 1 : RandomManager.GetRandomRange(TPSingleton<TurretManager>.Instance, 0, skillCasters.Count))
			.ToList();
	}

	[DevConsoleCommand(Name = "TurretsResetTurn")]
	public static void Debug_TurretsResetTurn()
	{
		foreach (TheLastStand.Model.Building.Building turret in TPSingleton<TurretManager>.Instance.Turrets)
		{
			turret.BuildingController.BattleModuleController.DecrementGoalsCooldown();
			turret.BuildingController.BattleModuleController.RefillSkillUsesPerTurn();
			turret.BuildingController.BattleModuleController.RefillSkillsOverallUses();
		}
	}
}
