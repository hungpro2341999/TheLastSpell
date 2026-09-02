using TheLastStand.Controller.Building.Module;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using UnityEngine;

namespace TheLastStand.Model.Building.Module;

public class ConstructionModule : BuildingModule
{
	public ConstructionModuleController ConstructionModuleController => base.BuildingModuleController as ConstructionModuleController;

	public ConstructionModuleDefinition ConstructionModuleDefinition => base.BuildingModuleDefinition as ConstructionModuleDefinition;

	public bool CostsGold => ConstructionModuleDefinition.NativeGoldCost > 0;

	public bool CostsMaterials => ConstructionModuleDefinition.NativeMaterialsCost > 0;

	public int Cost => BuildingManager.ComputeBuildingCost(ConstructionModuleDefinition);

	public bool IsDemolishable
	{
		get
		{
			if (ConstructionModuleDefinition.IsDemolishable)
			{
				return !ApocalypseManager.CurrentApocalypse.BuildingsNotDemolishableAnymore.Contains(base.BuildingParent.Id);
			}
			return false;
		}
	}

	public virtual int RepairCost
	{
		get
		{
			float num = ((base.BuildingParent.DamageableModule != null && base.BuildingParent.DamageableModule.HealthTotal > 0f) ? ((base.BuildingParent.DamageableModule.HealthTotal - base.BuildingParent.DamageableModule.Health) / base.BuildingParent.DamageableModule.HealthTotal) : 0f);
			return Mathf.CeilToInt(ConstructionDatabase.ConstructionDefinition.RepairCostRatio * (float)Cost * num);
		}
	}

	public virtual bool NeedRepair
	{
		get
		{
			if (ConstructionModuleDefinition.IsRepairable)
			{
				return base.BuildingParent.DamageableModule.Health < base.BuildingParent.DamageableModule.HealthTotal;
			}
			return false;
		}
	}

	public ConstructionModule(Building buildingParent, ConstructionModuleDefinition constructionModuleDefinition, ConstructionModuleController constructionModuleController)
		: base(buildingParent, constructionModuleDefinition, constructionModuleController)
	{
	}
}
