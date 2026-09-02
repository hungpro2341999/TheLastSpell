using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Debugging.Console;
using TheLastStand.Manager.Building;
using TheLastStand.Model;
using TheLastStand.Model.Building;

namespace TheLastStand.Manager.Trap;

public class TrapManager : ABuildingBehaviorManager<TrapManager>
{
	public List<TheLastStand.Model.Building.Building> Traps => TPSingleton<BuildingManager>.Instance.Buildings.Where((TheLastStand.Model.Building.Building x) => x.IsTrap).ToList();

	public override List<IBehaviorModel> BehaviorModels => ((IEnumerable<IBehaviorModel>)Traps.Select((TheLastStand.Model.Building.Building trap) => trap.BattleModule)).ToList();

	public static void StartTurn()
	{
		for (int i = 0; i < TPSingleton<TrapManager>.Instance.Traps.Count; i++)
		{
			TPSingleton<TrapManager>.Instance.Traps[i].BuildingController.StartTurn();
		}
	}

	public override IEnumerator PrepareSkillsForGroups()
	{
		yield return base.PrepareSkillsForGroups();
		foreach (TheLastStand.Model.Building.Building trap in Traps)
		{
			if (trap.BattleModule.RemainingTrapCharges == 0)
			{
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayBuildingInstantly(trap, trap.OriginTile, "_Disabled");
			}
		}
	}

	[DevConsoleCommand(Name = "TrapsResetTurn")]
	public static void Debug_TrapsResetTurn()
	{
		foreach (TheLastStand.Model.Building.Building trap in TPSingleton<TrapManager>.Instance.Traps)
		{
			trap.BuildingController.BattleModuleController.DecrementGoalsCooldown();
			trap.BuildingController.BattleModuleController.RefillSkillUsesPerTurn();
			trap.BuildingController.BattleModuleController.RefillSkillsOverallUses();
		}
	}
}
