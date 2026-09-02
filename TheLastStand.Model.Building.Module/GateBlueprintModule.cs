using TheLastStand.Controller.Building.Module;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;

namespace TheLastStand.Model.Building.Module;

public class GateBlueprintModule : BlueprintModule
{
	public static class Constants
	{
		public const string OpenedSuffix = "Opened";
	}

	public bool IsOpen
	{
		get
		{
			if (base.BuildingParent.OriginTile != null)
			{
				return base.BuildingParent.OriginTile.Unit != null;
			}
			return false;
		}
	}

	public override bool IsStoppingLineOfSight
	{
		get
		{
			foreach (Tile occupiedTile in base.BuildingParent.OriginTile.GetOccupiedTiles(base.BlueprintModuleDefinition))
			{
				if (occupiedTile.Unit is PlayableUnit)
				{
					return false;
				}
			}
			return base.IsStoppingLineOfSight;
		}
	}

	public GateBlueprintModule(Building buildingParent, BlueprintModuleDefinition blueprintModuleDefinition, GateBlueprintModuleController gateBlueprintModuleController)
		: base(buildingParent, blueprintModuleDefinition, gateBlueprintModuleController)
	{
	}

	public override bool IsTargetableByAI()
	{
		if (base.BuildingParent.OriginTile.Unit != null)
		{
			return false;
		}
		return base.IsTargetableByAI();
	}
}
