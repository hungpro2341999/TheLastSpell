using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.TileMap;
using TheLastStand.View.Skill.SkillAction;
using UnityEngine;

namespace TheLastStand.Controller.Building.Module;

public class BlueprintModuleController : BuildingModuleController, ITileObjectController, IEffectTargetSkillActionController
{
	private Coroutine displayEffectsCoroutine;

	public BlueprintModule BlueprintModule { get; }

	public BlueprintModuleController(BuildingController buildingControllerParent, BlueprintModuleDefinition blueprintModuleDefinition)
		: base(buildingControllerParent, blueprintModuleDefinition)
	{
		BlueprintModule = base.BuildingModule as BlueprintModule;
	}

	public void AddEffectDisplay(IDisplayableEffect displayableEffect)
	{
		base.BuildingControllerParent.Building.BuildingView.AddSkillEffectDisplay(displayableEffect);
		EffectManager.Register(this);
	}

	public void DisplayEffects(float delay = 0f)
	{
		if (displayEffectsCoroutine == null)
		{
			displayEffectsCoroutine = TPSingleton<GameManager>.Instance.StartCoroutine(DisplayEffectsCoroutine(delay));
		}
	}

	public void FreeOccupiedTiles()
	{
		for (int i = 0; i < BlueprintModule.OccupiedTiles.Count; i++)
		{
			Tile tile = BlueprintModule.OccupiedTiles[i];
			tile.TileController.SetBuilding(null);
			tile.Unit?.UnitView.UpdatePosition();
			tile.CurrentUnitAccess = Tile.E_UnitAccess.Everyone;
			if (base.BuildingControllerParent.Building.BuildingDefinition.ConstructionModuleDefinition.OccupationVolumeType != BuildingDefinition.E_OccupationVolumeType.Adjacent)
			{
				continue;
			}
			int buildingDeadZoneRange = BuildingManager.GetBuildingDeadZoneRange(base.BuildingControllerParent.Building.BuildingDefinition.Id);
			for (int j = -buildingDeadZoneRange; j <= buildingDeadZoneRange; j++)
			{
				for (int k = -buildingDeadZoneRange; k <= buildingDeadZoneRange; k++)
				{
					Tile tile2 = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(tile.Position.x + j, tile.Position.y + k);
					if (tile2 != null && !tile2.TileController.CheckIsInBuildingOccupationVolume())
					{
						tile2.TileController.SetOccupiedByBuildingVolume(isInBuildingVolume: false);
					}
				}
			}
		}
	}

	public List<Tile> GetAdjacentTiles()
	{
		HashSet<Tile> tiles = new HashSet<Tile>();
		foreach (Tile occupiedTile in BlueprintModule.OccupiedTiles)
		{
			occupiedTile.GetAdjacentTiles().ForEach(delegate(Tile o)
			{
				if (!BlueprintModule.OccupiedTiles.Contains(o))
				{
					tiles.Add(o);
				}
			});
		}
		return tiles.ToList();
	}

	public List<Tile> GetAdjacentTilesWithDiagonals()
	{
		HashSet<Tile> tiles = new HashSet<Tile>();
		foreach (Tile occupiedTile in BlueprintModule.OccupiedTiles)
		{
			occupiedTile.GetAdjacentTilesWithDiagonals().ForEach(delegate(Tile o)
			{
				if (!BlueprintModule.OccupiedTiles.Contains(o))
				{
					tiles.Add(o);
				}
			});
		}
		return tiles.ToList();
	}

	public int GetEffectsCount()
	{
		return base.BuildingControllerParent.BuildingView.SkillEffectDisplays.Count;
	}

	public List<Tile> GetTilesInRange(int maxRange, int minRange = 0, bool cardinalOnly = false)
	{
		return BlueprintModule.OccupiedTiles.GetTilesInRange(maxRange, minRange, cardinalOnly);
	}

	public Dictionary<Tile, Tile> GetTilesInRangeWithClosestOccupiedTile(int maxRange, int minRange = 0, bool cardinalOnly = false)
	{
		return BlueprintModule.OccupiedTiles.GetTilesInRangeWithClosestOccupiedTile(maxRange, minRange, cardinalOnly);
	}

	protected override BuildingModule CreateModel(TheLastStand.Model.Building.Building building, BuildingModuleDefinition buildingModuleDefinition)
	{
		return new BlueprintModule(building, buildingModuleDefinition as BlueprintModuleDefinition, this);
	}

	private IEnumerator DisplayEffectsCoroutine(float delay)
	{
		yield return base.BuildingControllerParent.BuildingView.DisplaySkillEffects(delay);
		EffectManager.Unregister(this);
		displayEffectsCoroutine = null;
	}
}
