using System;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Model.Building;

public class Construction
{
	public enum E_State
	{
		None,
		Repair,
		Destroy,
		ChooseBuilding,
		PlaceBuilding
	}

	public enum E_RepairMode
	{
		None,
		Target,
		Id
	}

	public enum E_DestroyMode
	{
		None,
		Target
	}

	public enum E_UnusableActionCause
	{
		None,
		NotEnoughResources,
		NoValidBuilding
	}

	private E_RepairMode repairMode;

	private E_DestroyMode destroyMode;

	public List<BuildingDefinition> AvailableBuildings { get; set; }

	public List<Tile> BuildingAvailableSpaceTiles { get; } = new List<Tile>();

	public Dictionary<string, byte> BuildingCount { get; } = new Dictionary<string, byte>();

	public BuildingDefinition BuildingToPlace { get; set; }

	public Tile GhostedTile { get; set; }

	public E_State State { get; set; }

	public E_RepairMode RepairMode
	{
		get
		{
			return repairMode;
		}
		set
		{
			E_RepairMode arg = repairMode;
			repairMode = value;
			this.RepairModeChanged?.Invoke(arg, repairMode);
		}
	}

	public E_DestroyMode DestroyMode
	{
		get
		{
			return destroyMode;
		}
		set
		{
			E_DestroyMode arg = destroyMode;
			destroyMode = value;
			this.DestroyModeChanged?.Invoke(arg, destroyMode);
		}
	}

	public event Action<E_RepairMode, E_RepairMode> RepairModeChanged;

	public event Action<E_DestroyMode, E_DestroyMode> DestroyModeChanged;

	public bool AnyTargetReparationAffordable(out E_UnusableActionCause unusableActionCause)
	{
		unusableActionCause = E_UnusableActionCause.None;
		bool flag = false;
		foreach (Building building in TPSingleton<BuildingManager>.Instance.Buildings)
		{
			if (building.ConstructionModule.NeedRepair)
			{
				flag = true;
				int repairCost = building.ConstructionModule.RepairCost;
				if ((building.ConstructionModule.CostsGold && TPSingleton<ResourceManager>.Instance.Gold >= repairCost) || (building.ConstructionModule.CostsMaterials && TPSingleton<ResourceManager>.Instance.Materials >= repairCost))
				{
					return true;
				}
			}
		}
		unusableActionCause = (flag ? E_UnusableActionCause.NotEnoughResources : E_UnusableActionCause.NoValidBuilding);
		return false;
	}

	public bool AnyIdReparationAffordable(out E_UnusableActionCause unusableActionCause)
	{
		unusableActionCause = E_UnusableActionCause.None;
		bool flag = false;
		foreach (var item in TPSingleton<BuildingManager>.Instance.Buildings.GroupBy((Building o) => o.BuildingDefinition.ConstructionModuleDefinition, (Building p) => p, (ConstructionModuleDefinition constructionModuleDefinition, IEnumerable<Building> buildings) => new
		{
			ConstructionDefinition = constructionModuleDefinition,
			Cost = buildings.Sum((Building o) => o.ConstructionModule.RepairCost)
		}))
		{
			if (item.Cost > 0)
			{
				flag = true;
			}
			bool flag2 = ((item.ConstructionDefinition.NativeGoldCost > 0) ? (item.Cost <= TPSingleton<ResourceManager>.Instance.Gold) : (item.Cost <= TPSingleton<ResourceManager>.Instance.Materials));
			if (item.Cost > 0 && flag2)
			{
				return true;
			}
		}
		unusableActionCause = (flag ? E_UnusableActionCause.NotEnoughResources : E_UnusableActionCause.NoValidBuilding);
		return false;
	}
}
