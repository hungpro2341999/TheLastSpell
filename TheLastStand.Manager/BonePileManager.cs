using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Manager.Building;
using TheLastStand.Model;
using TheLastStand.Model.Building;

namespace TheLastStand.Manager;

public class BonePileManager : ABuildingBehaviorManager<BonePileManager>
{
	public List<TheLastStand.Model.Building.Building> BonePiles => TPSingleton<BuildingManager>.Instance.Buildings.Where((TheLastStand.Model.Building.Building x) => x.IsBonePile).ToList();

	public override List<IBehaviorModel> BehaviorModels => ((IEnumerable<IBehaviorModel>)BonePiles.Select((TheLastStand.Model.Building.Building bonePile) => bonePile.BattleModule)).ToList();

	public static void StartTurn()
	{
		for (int i = 0; i < TPSingleton<BonePileManager>.Instance.BonePiles.Count; i++)
		{
			TPSingleton<BonePileManager>.Instance.BonePiles[i].BuildingController.StartTurn();
		}
	}
}
