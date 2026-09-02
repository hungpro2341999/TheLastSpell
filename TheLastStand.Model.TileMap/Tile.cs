using System;
using System.Collections.Generic;
using TPLib;
using TheLastStand.Controller;
using TheLastStand.Controller.TileMap;
using TheLastStand.Definition;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Hazard;
using TheLastStand.Definition.TileMap;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Manager;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View;
using TheLastStand.View.Building;
using TheLastStand.View.TileMap;
using TheLastStand.View.Unit;
using UnityEngine;

namespace TheLastStand.Model.TileMap;

public class Tile : ITileObject
{
	[Flags]
	public enum E_UnitAccess
	{
		None = 0,
		Blocked = 1,
		Hero = 2,
		Enemy = 4,
		Everyone = 6
	}

	private SpawnDirectionsDefinition.E_Direction directionToCenter;

	private int? willBeReachedBy;

	public HazardDefinition.E_HazardType HazardOwned;

	public int DistanceToCity = -1;

	public int DistanceToMagicCircle = -1;

	public SpawnDirectionsDefinition.E_Direction DirectionToCenter
	{
		get
		{
			if (directionToCenter == SpawnDirectionsDefinition.E_Direction.None)
			{
				Tile centerTile = TileMapController.GetCenterTile();
				directionToCenter = TileMapController.GetDirectionBetweenTiles(centerTile, this).ToSpawnDirection();
			}
			return directionToCenter;
		}
	}

	public TheLastStand.Model.Building.Building Building { get; set; }

	public E_UnitAccess CurrentUnitAccess { get; set; } = E_UnitAccess.Everyone;

	private IDamageable Damageable
	{
		get
		{
			if (Building == null || Building.BlueprintModule.IsIndestructible)
			{
				return Unit;
			}
			return Building.DamageableModule;
		}
	}

	public ISkillCaster SkillCaster
	{
		get
		{
			ISkillCaster skillCaster = Building?.BattleModule;
			return skillCaster ?? Unit;
		}
	}

	public List<EnemyUnitDeadBodyView> EnemyUnitDeadBodyViews { get; } = new List<EnemyUnitDeadBodyView>();

	public List<BuildingCorpseView> DeadBuildingViews { get; } = new List<BuildingCorpseView>();

	public HashSet<BuildingDefinition.E_BuildingCategory> ForbiddenBuildingFromCategory { get; set; } = new HashSet<BuildingDefinition.E_BuildingCategory>();

	public GroundDefinition GroundDefinition { get; set; }

	public bool HasAnyFog
	{
		get
		{
			if (!HasFog)
			{
				return HasLightFogOn;
			}
			return true;
		}
	}

	public bool HasLightFogOn
	{
		get
		{
			if (TPSingleton<FogManager>.Instance.Fog.LightFogTiles.TryGetValue(this, out var value))
			{
				return value.Mode == Fog.LightFogTileInfo.E_LightFogMode.Activated;
			}
			return false;
		}
	}

	public bool HasFog => HazardOwned.HasFlag(HazardDefinition.E_HazardType.Fog);

	public bool IsCrossableByEnemyOnly => CurrentUnitAccess == E_UnitAccess.Enemy;

	public bool IsCrossableByEveryone => CurrentUnitAccess == E_UnitAccess.Everyone;

	public bool IsCrossableByHeroOnly => CurrentUnitAccess == E_UnitAccess.Hero;

	public string Id => ToString();

	public bool IsCrossable
	{
		get
		{
			if (!GroundDefinition.IsCrossable)
			{
				if (Building != null)
				{
					return Building.IsProtoBridge;
				}
				return false;
			}
			return true;
		}
	}

	public bool IsInBuildingVolume { get; set; }

	public bool IsInCity => GroundDefinition.GroundCategory == GroundDefinition.E_GroundCategory.City;

	public bool IsInWorld => true;

	public List<Tile> OccupiedTiles { get; }

	public Tile OriginTile => this;

	public Vector2Int Position => new Vector2Int(X, Y);

	public ITileObject TileObject
	{
		get
		{
			ITileObject building = Building;
			return building ?? Unit;
		}
	}

	public ITileObjectController TileObjectController => TileController;

	public ITileObjectDefinition TileObjectDefinition => null;

	public ITileObjectView TileObjectView => TileView;

	public TileController TileController { get; }

	public TileView TileView { get; }

	public TheLastStand.Model.Unit.Unit Unit { get; set; }

	public bool WillBeReached => WillBeReachedBy.HasValue;

	public int? WillBeReachedBy
	{
		get
		{
			return willBeReachedBy;
		}
		set
		{
			willBeReachedBy = value;
		}
	}

	public int X { get; set; }

	public int Y { get; set; }

	public bool IsCityTile => GroundDefinition.GroundCategory == GroundDefinition.E_GroundCategory.City;

	public Tile(TileController tileController, TileView tileView, int x, int y, GroundDefinition groundDefinition)
	{
		TileController = tileController;
		TileView = tileView;
		X = x;
		Y = y;
		GroundDefinition = groundDefinition;
		OccupiedTiles = new List<Tile>(1) { this };
	}

	public static E_UnitAccess CharToUnitAccess(char tileChar)
	{
		return tileChar switch
		{
			'B' => E_UnitAccess.Blocked, 
			'H' => E_UnitAccess.Hero, 
			'E' => E_UnitAccess.Everyone, 
			_ => E_UnitAccess.None, 
		};
	}

	public bool CanAffectThroughFog(ISkillCaster skillCaster)
	{
		if (HasAnyFog && !skillCaster.CanAffectTargetInFog)
		{
			if (!HasFog)
			{
				return skillCaster.CanAffectTargetInLightFog;
			}
			return false;
		}
		return true;
	}

	public IDamageable GetDamageable(bool isUnitPriority = false)
	{
		if (!isUnitPriority)
		{
			return Damageable;
		}
		if (Unit != null)
		{
			return Unit;
		}
		if (Building?.DamageableModule != null && !Building.BlueprintModule.IsIndestructible)
		{
			return Building.DamageableModule;
		}
		return null;
	}

	public bool HasALightFogSupplier(out ILightFogSupplier lightFogSupplier)
	{
		if (Building?.PassivesModule?.BuildingPassives != null)
		{
			foreach (BuildingPassive item in Building.PassivesModule?.BuildingPassives)
			{
				lightFogSupplier = item.PassiveEffects.Find((BuildingPassiveEffect x) => x is ILightFogSupplier) as ILightFogSupplier;
				if (lightFogSupplier != null)
				{
					return true;
				}
			}
		}
		if (Unit is EnemyUnit enemyUnit)
		{
			return enemyUnit.HasLightFogSupplier(out lightFogSupplier);
		}
		lightFogSupplier = null;
		return false;
	}

	public bool HasTileFlagTag(TileFlagDefinition.E_TileFlagTag tileFlagTag)
	{
		if (TPSingleton<TileMapManager>.Instance.TileMap.TilesWithFlag.TryGetValue(tileFlagTag, out var value))
		{
			return value.Contains(this);
		}
		return false;
	}

	public bool IsCrossableBy(TheLastStand.Model.Unit.Unit unit)
	{
		if (!(unit is PlayableUnit))
		{
			if (unit is EnemyUnit)
			{
				return CurrentUnitAccess.HasFlag(E_UnitAccess.Enemy);
			}
			return IsCrossableByEveryone;
		}
		return CurrentUnitAccess.HasFlag(E_UnitAccess.Hero);
	}

	public bool IsEmpty()
	{
		if (Building == null)
		{
			return Unit == null;
		}
		return false;
	}

	public bool IsTargetableByAI()
	{
		return true;
	}

	public override string ToString()
	{
		return $"x={X} y={Y}";
	}
}
