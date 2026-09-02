using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Building.Module;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Definition.Unit.Race;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.TileMap;
using UnityEngine;

namespace TheLastStand.Model.Building.Module;

public class BlueprintModule : BuildingModule, IBarker
{
	public RaceDefinition BarkerRaceDefinition => null;

	public BlueprintModuleController BlueprintModuleController => base.BuildingModuleController as BlueprintModuleController;

	public BlueprintModuleDefinition BlueprintModuleDefinition => base.BuildingModuleDefinition as BlueprintModuleDefinition;

	public virtual Transform BarkViewFollowTarget => base.BuildingParent.BuildingView.transform;

	public bool HasBark { get; set; }

	public bool IsIndestructible
	{
		get
		{
			if (base.BuildingParent.DamageableModule != null)
			{
				return base.BuildingParent.DamageableModule.HealthTotal == 0f;
			}
			return true;
		}
	}

	public virtual bool IsStoppingLineOfSight => true;

	public List<Tile> OccupiedTiles => base.BuildingParent.OriginTile.GetOccupiedTiles(BlueprintModuleDefinition);

	public Tile OriginTile => base.BuildingParent.OriginTile;

	public BlueprintModule(Building buildingParent, BlueprintModuleDefinition blueprintModuleDefinition, BlueprintModuleController blueprintModuleController)
		: base(buildingParent, blueprintModuleDefinition, blueprintModuleController)
	{
	}

	public static Vector2Int GetRelativeBuildingTilePosition(Tile buildingTile, Tile originTile, BlueprintModuleDefinition blueprintDefinition)
	{
		return new Vector2Int(buildingTile.Position.x - (originTile.Position.x - blueprintDefinition.OriginX), buildingTile.Position.y - (originTile.Position.y - blueprintDefinition.OriginY));
	}

	public Tile.E_UnitAccess ConvertBuildingTileToUnitAccess(Tile buildingTile)
	{
		Vector2Int relativeBuildingTilePosition = GetRelativeBuildingTilePosition(buildingTile);
		if (BlueprintModuleDefinition.Tiles.Count <= relativeBuildingTilePosition.y || BlueprintModuleDefinition.Tiles[relativeBuildingTilePosition.y].Count <= relativeBuildingTilePosition.x)
		{
			TPSingleton<BuildingManager>.Instance.LogError(BlueprintModuleDefinition.BuildingDefinition.Id + " does not contain this tile", CLogLevel.DETAILED);
		}
		return BlueprintModuleDefinition.Tiles[relativeBuildingTilePosition.y][relativeBuildingTilePosition.x];
	}

	public Vector2Int GetRelativeBuildingTilePosition(Tile buildingTile)
	{
		return GetRelativeBuildingTilePosition(buildingTile, base.BuildingParent.OriginTile, BlueprintModuleDefinition);
	}

	public virtual bool IsTargetableByAI()
	{
		if (!IsIndestructible)
		{
			return base.BuildingParent.DamageableModule.Health > 0f;
		}
		return false;
	}
}
