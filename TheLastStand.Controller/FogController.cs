using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.TileMap;
using TheLastStand.DRM.Achievements;
using TheLastStand.Database.Fog;
using TheLastStand.Definition.Hazard;
using TheLastStand.Manager;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.Controller;

public static class FogController
{
	public static List<ILightFogSupplier> LightFogSuppliers { get; private set; } = new List<ILightFogSupplier>();

	public static List<ILightFogSupplier> MovingLightFogSuppliers { get; private set; } = new List<ILightFogSupplier>();

	public static void DecreaseDensity(bool refreshFog = false, int amount = 1)
	{
		TPSingleton<FogManager>.Instance.Fog.DensityIndex -= amount;
		SetFogTilesAndRecomputeSpawnPoints();
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day)
		{
			RefreshFogArea();
		}
		else
		{
			RefreshFog(refreshFog);
		}
		if (IsDensityAtMinimum())
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_REPEL_FOG_AT_MAX);
		}
	}

	public static Dictionary<Fog.LightFogTileInfo.E_LightFogMode, List<Tile>> DecrementLightFogTilesBuffer(List<Tile> tiles)
	{
		Dictionary<Fog.LightFogTileInfo.E_LightFogMode, List<Tile>> dictionary = CreateEditTilesDico();
		for (int num = tiles.Count - 1; num >= 0; num--)
		{
			if (TPSingleton<FogManager>.Instance.Fog.LightFogTiles.TryGetValue(tiles[num], out var value))
			{
				if (--value.Buffer <= 0)
				{
					tiles[num].HazardOwned &= ~HazardDefinition.E_HazardType.LightFog;
					TPSingleton<FogManager>.Instance.Fog.LightFogTiles.Remove(tiles[num]);
					dictionary[Fog.LightFogTileInfo.E_LightFogMode.None].Add(tiles[num]);
				}
				else
				{
					Fog.LightFogTileInfo.E_LightFogMode lightFogMode = GetLightFogMode(tiles[num]);
					if (value.Mode != lightFogMode)
					{
						value.Mode = lightFogMode;
						dictionary[lightFogMode].Add(tiles[num]);
					}
				}
			}
		}
		return dictionary;
	}

	public static void IncreaseDensity(bool refreshFog = false, int amount = 1)
	{
		TPSingleton<FogManager>.Instance.Fog.DensityIndex += amount;
		SetFogTilesAndRecomputeSpawnPoints();
		RefreshFog(refreshFog);
	}

	public static Dictionary<Fog.LightFogTileInfo.E_LightFogMode, List<Tile>> IncrementLightFogTilesBuffer(List<Tile> tiles)
	{
		Dictionary<Fog.LightFogTileInfo.E_LightFogMode, List<Tile>> dictionary = CreateEditTilesDico();
		for (int i = 0; i < tiles.Count; i++)
		{
			Fog.LightFogTileInfo.E_LightFogMode lightFogMode = GetLightFogMode(tiles[i]);
			if (TPSingleton<FogManager>.Instance.Fog.LightFogTiles.TryGetValue(tiles[i], out var value))
			{
				value.Buffer++;
				if (lightFogMode != value.Mode)
				{
					value.Mode = lightFogMode;
					dictionary[lightFogMode].Add(tiles[i]);
				}
			}
			else
			{
				tiles[i].HazardOwned |= HazardDefinition.E_HazardType.LightFog;
				TPSingleton<FogManager>.Instance.Fog.LightFogTiles.Add(tiles[i], new Fog.LightFogTileInfo(lightFogMode));
				dictionary[lightFogMode].Add(tiles[i]);
			}
		}
		return dictionary;
	}

	private static Fog.LightFogTileInfo.E_LightFogMode GetLightFogMode(Tile tile)
	{
		if (tile.HasALightFogSupplier(out var lightFogSupplier))
		{
			if (lightFogSupplier.IsLightFogSupplierMoving && lightFogSupplier.LightFogSupplierMoveDatas.CurrentTile != tile)
			{
				return Fog.LightFogTileInfo.E_LightFogMode.Activated;
			}
			if (!lightFogSupplier.CanLightFogExistOnSelf)
			{
				return Fog.LightFogTileInfo.E_LightFogMode.Impeded;
			}
			return Fog.LightFogTileInfo.E_LightFogMode.Activated;
		}
		if (TryGetLightFogSupplierMovingOnTile(tile, out var lightFogSupplier2))
		{
			if (!lightFogSupplier2.CanLightFogExistOnSelf)
			{
				return Fog.LightFogTileInfo.E_LightFogMode.Impeded;
			}
			return Fog.LightFogTileInfo.E_LightFogMode.Activated;
		}
		if (!ShouldActivateLightFogOnTile(tile))
		{
			return Fog.LightFogTileInfo.E_LightFogMode.Deactivated;
		}
		return Fog.LightFogTileInfo.E_LightFogMode.Activated;
	}

	private static bool TryGetLightFogSupplierMovingOnTile(Tile tile, out ILightFogSupplier lightFogSupplier)
	{
		for (int i = 0; i < MovingLightFogSuppliers.Count; i++)
		{
			lightFogSupplier = MovingLightFogSuppliers[i];
			if (lightFogSupplier.IsLightFogSupplierMoving && lightFogSupplier.LightFogSupplierMoveDatas.CurrentTile == tile)
			{
				return true;
			}
		}
		lightFogSupplier = null;
		return false;
	}

	public static bool IsDensityAtMaximum()
	{
		if (TPSingleton<FogManager>.Instance.Fog.DensityIndex != TPSingleton<FogManager>.Instance.Fog.FogDefinition.FogDensities.Count - 1)
		{
			return TPSingleton<FogManager>.Instance.Fog.DensityIndex >= TPSingleton<FogManager>.Instance.FogMaxIndex;
		}
		return true;
	}

	public static bool IsDensityAtMinimum()
	{
		return TPSingleton<FogManager>.Instance.Fog.DensityIndex == 0;
	}

	public static bool IsDensityEqualTo(string densityName)
	{
		for (int num = TPSingleton<FogManager>.Instance.Fog.FogDefinition.FogDensities.Count - 1; num >= 0; num--)
		{
			if (TPSingleton<FogManager>.Instance.Fog.FogDefinition.FogDensities[num].Name == densityName)
			{
				return TPSingleton<FogManager>.Instance.Fog.DensityIndex == num;
			}
		}
		TPSingleton<FogManager>.Instance.LogError("Tried to set the density of the fog but couldn't find any defined density in FogDefinition.xml with the name \"" + densityName + "\" !", CLogLevel.MAJOR);
		return false;
	}

	public static void RefreshFog(bool instant = false)
	{
		for (int i = 0; i < TPSingleton<FogManager>.Instance.Fog.FogTiles.Count; i++)
		{
			Tile tile = TPSingleton<FogManager>.Instance.Fog.FogTiles[i];
			if (tile.Unit != null && tile.Unit is EnemyUnit enemyUnit)
			{
				enemyUnit.EnemyUnitView.RefreshHealth();
				enemyUnit.EnemyUnitView.RefreshInjuryStage();
			}
			if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && tile.Building != null && tile.Building.OccupiedTiles.All((Tile t) => t.HasFog) && !tile.Building.IsObstacle)
			{
				BuildingManager.DestroyBuilding(tile);
			}
		}
		TPSingleton<FogManager>.Instance.FogView.DisplayFog(instant);
	}

	public static void RegisterSupplier(ILightFogSupplier lightFogSupplier)
	{
		LightFogSuppliers.Add(lightFogSupplier);
		if (lightFogSupplier.CanLightFogSupplierMove)
		{
			MovingLightFogSuppliers.Add(lightFogSupplier);
		}
	}

	public static void RefreshLightFog(float fadeDuration = -1f, bool instant = false)
	{
		switch (TPSingleton<GameManager>.Instance.Game.Cycle)
		{
		case Game.E_Cycle.Day:
			SetLightFogTilesFromDictionnary(ToggleLightFogTiles((from kvp in TPSingleton<FogManager>.Instance.Fog.LightFogTiles
				where kvp.Value.Mode == Fog.LightFogTileInfo.E_LightFogMode.Activated
				select kvp.Key).ToList()), FogManager.LightFogFadeInEaseAndDuration, FogManager.LightFogFadeOutEaseAndDuration, FogManager.LightFogDisappearEaseAndDuration);
			break;
		case Game.E_Cycle.Night:
			SetLightFogTilesFromDictionnary(ToggleLightFogTiles((from kvp in TPSingleton<FogManager>.Instance.Fog.LightFogTiles
				where kvp.Value.Mode == Fog.LightFogTileInfo.E_LightFogMode.Deactivated
				select kvp.Key).ToList()), FogManager.LightFogFadeInEaseAndDuration, FogManager.LightFogFadeOutEaseAndDuration, FogManager.LightFogDisappearEaseAndDuration);
			break;
		}
	}

	public static void RefreshFogArea()
	{
		TileMapView.SetTiles(TileMapView.FogAreaTilemap, TPSingleton<FogManager>.Instance.Fog.FogTiles, (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day) ? "View/Tiles/Feedbacks/MistRange/MistRange" : null);
		TileMapView.FogLimitTilemap.ClearAllTiles();
		TileMapView.SetTiles(TileMapView.FogLimitTilemap, TPSingleton<FogManager>.Instance.Fog.TilesOutOfFog, "View/Tiles/Feedbacks/MistLimits/MistLimits");
		TPSingleton<FogManager>.Instance.FogView.ChangeFogAreaIntensity(TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day);
	}

	public static void RemoveLightFogTiles(List<Tile> tiles, float duration, Ease easing, bool instant = false, bool independently = false)
	{
		foreach (Tile tile in tiles)
		{
			RemoveLightFogTile(tile, duration, easing, instant, independently);
		}
	}

	public static void RemoveLightFogTile(Tile tile, float duration, Ease easing, bool instant = false, bool independently = false)
	{
		if (tile.Unit is EnemyUnit enemyUnit)
		{
			enemyUnit.EnemyUnitView.RefreshMaterial();
			enemyUnit.EnemyUnitView.RefreshStatus();
			enemyUnit.EnemyUnitView.RefreshInjuryStage();
		}
		if (instant)
		{
			TileMapView.SetTile(TileMapView.LightFogOffTilemap, tile);
			TileMapView.SetTile(TileMapView.LightFogOnTilemap, tile);
			return;
		}
		if (!independently)
		{
			TPSingleton<FogManager>.Instance.StartCoroutine(TPSingleton<TileMapView>.Instance.FadeTileAlphaCoroutine(tile, fadeIn: false, TileMapView.LightFogOffTilemap, null, duration, easing, completeIfRunningAlready: false));
		}
		else
		{
			TPSingleton<TileMapView>.Instance.FadeTileIndependently(ref FogManager.LightFogTweens, tile, fadeIn: false, TileMapView.LightFogOffTilemap, null, duration, easing);
		}
		if (!independently)
		{
			TPSingleton<FogManager>.Instance.StartCoroutine(TPSingleton<TileMapView>.Instance.FadeTileAlphaCoroutine(tile, fadeIn: false, TileMapView.LightFogOnTilemap, null, duration, easing, completeIfRunningAlready: false));
		}
		else
		{
			TPSingleton<TileMapView>.Instance.FadeTileIndependently(ref FogManager.LightFogTweens, tile, fadeIn: false, TileMapView.LightFogOnTilemap, null, duration, easing);
		}
	}

	public static void SetDensity(string densityName)
	{
		for (int num = TPSingleton<FogManager>.Instance.Fog.FogDefinition.FogDensities.Count - 1; num >= 0; num--)
		{
			if (TPSingleton<FogManager>.Instance.Fog.FogDefinition.FogDensities[num].Name == densityName)
			{
				TPSingleton<FogManager>.Instance.Fog.DensityIndex = num;
				SetFogTilesAndRecomputeSpawnPoints();
				RefreshFog();
				return;
			}
		}
		TPSingleton<FogManager>.Instance.LogError("Tried to set the density of the fog but couldn't find any defined density in FogDefinition.xml with the name \"" + densityName + "\" !", CLogLevel.MAJOR);
	}

	public static void SetFogTilesAndRecomputeSpawnPoints(bool instant = false)
	{
		SetFogTiles(instant);
		if (ApplicationManager.Application.State.GetName() != "LevelEditor")
		{
			SpawnWaveManager.CurrentSpawnWave?.SpawnWaveController.RecomputeSpawnPoints();
		}
	}

	public static void SetFogTiles(bool instant = false)
	{
		TPSingleton<FogManager>.Instance.Fog.TilesOutOfFog.Clear();
		Tile centerTile = TileMapController.GetCenterTile();
		List<Tile> list = new List<Tile>();
		int i = 0;
		for (int width = TPSingleton<TileMapManager>.Instance.TileMap.Width; i < width; i++)
		{
			int j = 0;
			for (int height = TPSingleton<TileMapManager>.Instance.TileMap.Height; j < height; j++)
			{
				Tile tile = TileMapManager.GetTile(i, j);
				bool flag = Mathf.Abs(tile.X - centerTile.X) >= TPSingleton<FogManager>.Instance.Fog.DensityValue || Mathf.Abs(tile.Y - centerTile.Y) >= TPSingleton<FogManager>.Instance.Fog.DensityValue;
				bool flag2 = Mathf.Abs(tile.X - centerTile.X) == TPSingleton<FogManager>.Instance.Fog.DensityValue || Mathf.Abs(tile.Y - centerTile.Y) == TPSingleton<FogManager>.Instance.Fog.DensityValue;
				if (tile.HasFog != flag)
				{
					list.Add(tile);
					if (flag)
					{
						tile.HazardOwned |= HazardDefinition.E_HazardType.Fog;
						if (!TPSingleton<FogManager>.Instance.Fog.FogTiles.Contains(tile))
						{
							TPSingleton<FogManager>.Instance.Fog.FogTiles.Add(tile);
						}
					}
					else
					{
						tile.HazardOwned &= ~HazardDefinition.E_HazardType.Fog;
						if (TPSingleton<FogManager>.Instance.Fog.FogTiles.Contains(tile))
						{
							TPSingleton<FogManager>.Instance.Fog.FogTiles.Remove(tile);
						}
					}
				}
				if (!flag || flag2)
				{
					TPSingleton<FogManager>.Instance.Fog.TilesOutOfFog.Add(tile);
				}
			}
		}
		TPSingleton<FogManager>.Instance.FogView.FadeFogTiles(list, TPSingleton<FogManager>.Instance.Fog.PreviousDensityIndex < TPSingleton<FogManager>.Instance.Fog.DensityIndex, instant);
	}

	public static bool ShouldActivateLightFogOnTile(Tile tile)
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night)
		{
			return false;
		}
		List<Tile> source = ((!FogDatabase.LightFogDefinition.Repel.CheckDiagonals) ? TileMapController.GetTilesInRange(tile, FogDatabase.LightFogDefinition.Repel.Range) : TileMapController.GetTilesInRect(new RectInt(tile.Position.x - FogDatabase.LightFogDefinition.Repel.Range, tile.Position.y - FogDatabase.LightFogDefinition.Repel.Range, FogDatabase.LightFogDefinition.Repel.Range * 2, FogDatabase.LightFogDefinition.Repel.Range * 2)).ToList());
		if (!source.Any((Tile x) => x.Unit is PlayableUnit))
		{
			return !(tile.Unit is PlayableUnit);
		}
		return false;
	}

	public static Dictionary<Fog.LightFogTileInfo.E_LightFogMode, List<Tile>> ToggleLightFogTiles(List<Tile> tiles)
	{
		Dictionary<Fog.LightFogTileInfo.E_LightFogMode, List<Tile>> dictionary = CreateEditTilesDico();
		for (int i = 0; i < tiles.Count; i++)
		{
			if (TPSingleton<FogManager>.Instance.Fog.LightFogTiles.TryGetValue(tiles[i], out var value))
			{
				Fog.LightFogTileInfo.E_LightFogMode lightFogMode = GetLightFogMode(tiles[i]);
				if (value.Mode != lightFogMode)
				{
					value.Mode = lightFogMode;
					dictionary[lightFogMode].Add(tiles[i]);
				}
			}
		}
		return dictionary;
	}

	public static void UnregisterSupplier(ILightFogSupplier lightFogSupplier)
	{
		LightFogSuppliers.Remove(lightFogSupplier);
		if (lightFogSupplier.CanLightFogSupplierMove)
		{
			MovingLightFogSuppliers.Remove(lightFogSupplier);
		}
	}

	public static void SetLightFogTilesFromDictionnary(Dictionary<Fog.LightFogTileInfo.E_LightFogMode, List<Tile>> tilesToUpdateByLightFogMode, FogManager.LightFogFadeEaseAndDuration activatedEaseAndDuration, FogManager.LightFogFadeEaseAndDuration deactivatedEaseAndDuration, FogManager.LightFogFadeEaseAndDuration noneEaseAndDuration, bool instant = false, bool independently = false)
	{
		tilesToUpdateByLightFogMode.ForEach(delegate(Fog.LightFogTileInfo.E_LightFogMode lightFogMode, List<Tile> tilesToUpdate)
		{
			switch (lightFogMode)
			{
			case Fog.LightFogTileInfo.E_LightFogMode.None:
			case Fog.LightFogTileInfo.E_LightFogMode.Impeded:
				RemoveLightFogTiles(tilesToUpdate, noneEaseAndDuration.duration, noneEaseAndDuration.ease, instant, independently);
				break;
			case Fog.LightFogTileInfo.E_LightFogMode.Activated:
				SetLightFogTiles(tilesToUpdate, lightFogMode, activatedEaseAndDuration.duration, activatedEaseAndDuration.ease, instant, independently);
				break;
			case Fog.LightFogTileInfo.E_LightFogMode.Deactivated:
				SetLightFogTiles(tilesToUpdate, lightFogMode, deactivatedEaseAndDuration.duration, deactivatedEaseAndDuration.ease, instant, independently);
				break;
			}
		});
	}

	private static void SetLightFogTiles(List<Tile> tiles, Fog.LightFogTileInfo.E_LightFogMode lightFogMode, float fadeDuration, Ease fadeEasing, bool instant = false, bool independently = false)
	{
		if (tiles == null || tiles.Count == 0)
		{
			return;
		}
		for (int num = tiles.Count - 1; num >= 0; num--)
		{
			if (tiles[num].Unit is EnemyUnit enemyUnit)
			{
				enemyUnit.EnemyUnitView.RefreshMaterial();
				enemyUnit.EnemyUnitView.RefreshStatus();
				enemyUnit.EnemyUnitView.RefreshInjuryStage();
			}
		}
		if (instant)
		{
			TileMapView.SetTiles(TileMapView.LightFogOffTilemap, tiles, (lightFogMode == Fog.LightFogTileInfo.E_LightFogMode.Deactivated) ? "View/Tiles/World/LightFog_Dispelled" : null);
			TileMapView.SetTiles(TileMapView.LightFogOnTilemap, tiles, (lightFogMode == Fog.LightFogTileInfo.E_LightFogMode.Activated) ? "View/Tiles/World/LightFog" : null);
			return;
		}
		if (!independently)
		{
			TPSingleton<FogManager>.Instance.StartCoroutine(TPSingleton<TileMapView>.Instance.FadeTilesAlphaCoroutine(tiles, lightFogMode == Fog.LightFogTileInfo.E_LightFogMode.Deactivated, TileMapView.LightFogOffTilemap, (lightFogMode == Fog.LightFogTileInfo.E_LightFogMode.Deactivated) ? "View/Tiles/World/LightFog_Dispelled" : null, fadeDuration, fadeEasing));
		}
		else
		{
			TPSingleton<TileMapView>.Instance.FadeTilesIndependently(ref FogManager.LightFogTweens, tiles, lightFogMode == Fog.LightFogTileInfo.E_LightFogMode.Deactivated, TileMapView.LightFogOffTilemap, (lightFogMode == Fog.LightFogTileInfo.E_LightFogMode.Deactivated) ? "View/Tiles/World/LightFog_Dispelled" : null, fadeDuration, fadeEasing);
		}
		if (!independently)
		{
			TPSingleton<FogManager>.Instance.StartCoroutine(TPSingleton<TileMapView>.Instance.FadeTilesAlphaCoroutine(tiles, lightFogMode == Fog.LightFogTileInfo.E_LightFogMode.Activated, TileMapView.LightFogOnTilemap, (lightFogMode == Fog.LightFogTileInfo.E_LightFogMode.Activated) ? "View/Tiles/World/LightFog" : null, fadeDuration, fadeEasing));
		}
		else
		{
			TPSingleton<TileMapView>.Instance.FadeTilesIndependently(ref FogManager.LightFogTweens, tiles, lightFogMode == Fog.LightFogTileInfo.E_LightFogMode.Activated, TileMapView.LightFogOnTilemap, (lightFogMode == Fog.LightFogTileInfo.E_LightFogMode.Activated) ? "View/Tiles/World/LightFog" : null, fadeDuration, fadeEasing);
		}
	}

	private static Dictionary<Fog.LightFogTileInfo.E_LightFogMode, List<Tile>> CreateEditTilesDico()
	{
		return new Dictionary<Fog.LightFogTileInfo.E_LightFogMode, List<Tile>>
		{
			{
				Fog.LightFogTileInfo.E_LightFogMode.Activated,
				new List<Tile>()
			},
			{
				Fog.LightFogTileInfo.E_LightFogMode.Deactivated,
				new List<Tile>()
			},
			{
				Fog.LightFogTileInfo.E_LightFogMode.Impeded,
				new List<Tile>()
			},
			{
				Fog.LightFogTileInfo.E_LightFogMode.None,
				new List<Tile>()
			}
		};
	}
}
