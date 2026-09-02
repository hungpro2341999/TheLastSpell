using TheLastStand.Controller.Building.Module;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.Module;
using UnityEngine;

namespace TheLastStand.Model.Building.Module;

public class TrapConstructionModule : ConstructionModule
{
	public override bool NeedRepair
	{
		get
		{
			if (base.ConstructionModuleDefinition.IsRepairable)
			{
				return base.BuildingParent.BattleModule.RemainingTrapCharges < base.BuildingParent.BattleModule.BattleModuleDefinition.MaximumTrapCharges;
			}
			return false;
		}
	}

	public override int RepairCost
	{
		get
		{
			float num = (float)(base.BuildingParent.BattleModule.BattleModuleDefinition.MaximumTrapCharges - base.BuildingParent.BattleModule.RemainingTrapCharges) / (float)base.BuildingParent.BattleModule.BattleModuleDefinition.MaximumTrapCharges;
			return Mathf.CeilToInt(ConstructionDatabase.ConstructionDefinition.RepairCostRatio * (float)base.Cost * num);
		}
	}

	public TrapConstructionModule(Building buildingParent, ConstructionModuleDefinition constructionModuleDefinition, TrapConstructionModuleController trapConstructionModuleController)
		: base(buildingParent, constructionModuleDefinition, trapConstructionModuleController)
	{
	}
}
