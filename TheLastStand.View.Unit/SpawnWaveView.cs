using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.TileMap;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Seer;
using UnityEngine;

namespace TheLastStand.View.Unit;

public class SpawnWaveView : SerializedMonoBehaviour
{
	public enum E_SpawnWaveDetailedZone
	{
		None,
		Zone_N,
		Zone_N_NE,
		Zone_NE,
		Zone_E_NE,
		Zone_E,
		Zone_E_SE,
		Zone_SE,
		Zone_S_SE,
		Zone_S,
		Zone_S_SW,
		Zone_SW,
		Zone_W_SW,
		Zone_W,
		Zone_W_NW,
		Zone_NW,
		Zone_N_NW
	}

	[Serializable]
	public struct SpawnWaveArrowPair
	{
		public E_SpawnWaveDetailedZone SpawnWaveDetailedZone;

		public SpawnWaveViewPreviewFeedback SpawnWaveViewPreviewFeedback;

		public List<SpawnDirectionsDefinition.E_Direction> SpawnDirections => SpawnWaveDetailedZone.ToSpawnDirections();

		public SpawnDirectionsDefinition.E_Direction CentralSpawnDirection
		{
			get
			{
				if (!SpawnWaveDetailedZone.IsCentralZone())
				{
					return SpawnDirectionsDefinition.E_Direction.None;
				}
				return SpawnWaveDetailedZone.ToSpawnDirections()[0];
			}
		}
	}

	[Serializable]
	public struct ArrowDisplayAnimator
	{
		public string Id;

		public RuntimeAnimatorController AnimatorController;
	}

	[Serializable]
	public struct ArrowSizeSuffixPair
	{
		public int value;

		public string sizeSuffix;
	}

	public struct SpawnWaveViewRefreshInfo
	{
		public int DirectionDangerLevel;

		public int DirectionSpawnAmount;

		public int LocalProportionPercentage;

		public Tile Tile;

		public E_SpawnWaveDetailedZone Zone;

		public SpawnWaveViewRefreshInfo(SpawnWaveArrowPair spawnWaveArrowPair, SpawnWave spawnWave, bool calculateLocalPercentage)
		{
			DirectionSpawnAmount = 0;
			LocalProportionPercentage = 0;
			foreach (SpawnDirectionsDefinition.E_Direction spawnDirection in spawnWaveArrowPair.SpawnDirections)
			{
				spawnWave.RotatedProportionPerDirection.TryGetValue(spawnDirection, out var value);
				int num = value?.TotalProportion ?? 0;
				DirectionSpawnAmount += Mathf.RoundToInt((float)num / 100f * (float)spawnWave.SpawnsCount);
				LocalProportionPercentage += (calculateLocalPercentage ? Mathf.RoundToInt((float)num * spawnWave.GetSpawnPointPercentageInZone(spawnWaveArrowPair.SpawnWaveDetailedZone.ToTileFlag(), spawnDirection)) : num);
			}
			DirectionDangerLevel = (spawnWaveArrowPair.SpawnWaveDetailedZone.IsCentralZone() ? SpawnWaveManager.SpawnWaveView.GetDangerLevel(DirectionSpawnAmount) : (-1));
			Tile = GetTileForZone(spawnWaveArrowPair.SpawnWaveDetailedZone);
			Zone = spawnWaveArrowPair.SpawnWaveDetailedZone;
		}
	}

	public static class Constants
	{
		public const string Bot = "Bot";

		public const string BotLeft = "BotLeft";

		public const string BotRight = "BotRight";

		public const string Right = "Right";

		public const string TopRight = "TopRight";

		public const string AssetNameFormat = "WavesArrow_{0}_{1}";
	}

	[SerializeField]
	private List<int> dangerLevels = new List<int>();

	[SerializeField]
	private List<ArrowSizeSuffixPair> sizeSuffixValues = new List<ArrowSizeSuffixPair>();

	[SerializeField]
	private List<SpawnWaveArrowPair> spawnWavePreviewFeedbacks = new List<SpawnWaveArrowPair>();

	[SerializeField]
	private List<ArrowDisplayAnimator> ArrowDisplayAnimators = new List<ArrowDisplayAnimator>();

	public SpawnWave SpawnWave { get; set; }

	public List<SpawnWaveArrowPair> SpawnWavePreviewFeedbacks => spawnWavePreviewFeedbacks;

	public void RefreshPosition()
	{
		foreach (SpawnWaveArrowPair spawnWavePreviewFeedback in spawnWavePreviewFeedbacks)
		{
			spawnWavePreviewFeedback.SpawnWaveViewPreviewFeedback.RefreshPosition(GetTileForZone(spawnWavePreviewFeedback.SpawnWaveDetailedZone));
		}
	}

	public void Refresh(bool onDayStart = false, bool forceDisplayArrows = false)
	{
		if (SpawnWave == null)
		{
			CLoggerManager.Log("Tried to refresh SpawnWaveView with a null spawnWave, early returning. (Known error case: deserialize of Seer)", LogType.Warning, CLogLevel.NORMAL, forcePrintInUnity: true, "SpawnWaveManager");
			return;
		}
		if (SpawnWaveManager.DetailedSpawnWaveArrows)
		{
			RefreshAllArrows(forceDisplayArrows);
		}
		else
		{
			RefreshMainArrows(forceDisplayArrows);
		}
		if (SpawnWaveManager.AliveSeer && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.NightReport && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.CutscenePlaying && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.GameOver && TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night)
		{
			TPSingleton<SeerPreviewDisplay>.Instance.Displayed = true;
			TPSingleton<SeerPreviewDisplay>.Instance.DisplayEnemyPortraits();
		}
		else
		{
			TPSingleton<SeerPreviewDisplay>.Instance.Displayed = false;
		}
	}

	public void RefreshAllArrows(bool forceDisplayArrows)
	{
		foreach (SpawnWaveArrowPair spawnWavePreviewFeedback in spawnWavePreviewFeedbacks)
		{
			SpawnWaveViewRefreshInfo spawnWaveViewRefreshInfo = new SpawnWaveViewRefreshInfo(spawnWavePreviewFeedback, SpawnWave, calculateLocalPercentage: true);
			spawnWavePreviewFeedback.SpawnWaveViewPreviewFeedback.Refresh(spawnWaveViewRefreshInfo, forceDisplayArrows);
		}
	}

	public void RefreshMainArrows(bool forceDisplayArrows)
	{
		foreach (SpawnWaveArrowPair spawnWavePreviewFeedback in spawnWavePreviewFeedbacks)
		{
			if (spawnWavePreviewFeedback.SpawnWaveDetailedZone.IsCentralZone())
			{
				SpawnWaveViewRefreshInfo spawnWaveViewRefreshInfo = new SpawnWaveViewRefreshInfo(spawnWavePreviewFeedback, SpawnWave, calculateLocalPercentage: false);
				spawnWavePreviewFeedback.SpawnWaveViewPreviewFeedback.Refresh(spawnWaveViewRefreshInfo, forceDisplayArrows);
			}
			else
			{
				spawnWavePreviewFeedback.SpawnWaveViewPreviewFeedback.Clear();
			}
		}
	}

	public int GetDangerLevel(int spawnCount)
	{
		int result = 0;
		for (int i = 0; i < dangerLevels.Count && dangerLevels[i] < spawnCount; i++)
		{
			result = i + 1;
		}
		return result;
	}

	public static Tile GetTileForZone(E_SpawnWaveDetailedZone zone)
	{
		Tile centerTile = TileMapController.GetCenterTile();
		int densityValue = TPSingleton<FogManager>.Instance.Fog.DensityValue;
		int x;
		int y;
		switch (zone)
		{
		case E_SpawnWaveDetailedZone.Zone_N:
			x = centerTile.X;
			y = densityValue + centerTile.Y;
			break;
		case E_SpawnWaveDetailedZone.Zone_E:
			x = densityValue + centerTile.X;
			y = centerTile.Y;
			break;
		case E_SpawnWaveDetailedZone.Zone_S:
			x = centerTile.X;
			y = centerTile.Y - densityValue;
			break;
		case E_SpawnWaveDetailedZone.Zone_W:
			x = centerTile.X - densityValue;
			y = centerTile.Y;
			break;
		case E_SpawnWaveDetailedZone.Zone_NE:
			x = densityValue + centerTile.X;
			y = densityValue + centerTile.Y;
			break;
		case E_SpawnWaveDetailedZone.Zone_SE:
			x = densityValue + centerTile.X;
			y = centerTile.Y - densityValue;
			break;
		case E_SpawnWaveDetailedZone.Zone_NW:
			x = centerTile.X - densityValue;
			y = densityValue + centerTile.Y;
			break;
		case E_SpawnWaveDetailedZone.Zone_SW:
			x = centerTile.X - densityValue;
			y = centerTile.Y - densityValue;
			break;
		case E_SpawnWaveDetailedZone.Zone_N_NE:
			x = densityValue / 2 + centerTile.X;
			y = densityValue + centerTile.Y;
			break;
		case E_SpawnWaveDetailedZone.Zone_E_NE:
			x = densityValue + centerTile.X;
			y = densityValue / 2 + centerTile.Y;
			break;
		case E_SpawnWaveDetailedZone.Zone_E_SE:
			x = densityValue + centerTile.X;
			y = centerTile.Y - densityValue / 2;
			break;
		case E_SpawnWaveDetailedZone.Zone_S_SE:
			x = densityValue / 2 + centerTile.X;
			y = centerTile.Y - densityValue;
			break;
		case E_SpawnWaveDetailedZone.Zone_S_SW:
			x = centerTile.X - densityValue / 2;
			y = centerTile.Y - densityValue;
			break;
		case E_SpawnWaveDetailedZone.Zone_W_SW:
			x = centerTile.X - densityValue;
			y = centerTile.Y - densityValue / 2;
			break;
		case E_SpawnWaveDetailedZone.Zone_W_NW:
			x = centerTile.X - densityValue;
			y = densityValue / 2 + centerTile.Y;
			break;
		case E_SpawnWaveDetailedZone.Zone_N_NW:
			x = centerTile.X - densityValue / 2;
			y = densityValue + centerTile.Y;
			break;
		default:
			return null;
		}
		return TPSingleton<TileMapManager>.Instance.TileMap.GetTile(x, y);
	}

	public bool GetArrowAssetId(E_SpawnWaveDetailedZone zone, int proportion, out RuntimeAnimatorController animatorController)
	{
		string arrowAssetId = $"WavesArrow_{GetZoneId(zone)}_{GetSizeSuffix(proportion)}";
		animatorController = SpawnWaveManager.CurrentSpawnWave?.SpawnWaveView.ArrowDisplayAnimators.FirstOrDefault((ArrowDisplayAnimator x) => x.Id == arrowAssetId).AnimatorController;
		return animatorController != null;
	}

	private string GetSizeSuffix(int value)
	{
		for (int num = sizeSuffixValues.Count - 1; num >= 0; num--)
		{
			if (sizeSuffixValues[num].value <= value)
			{
				return sizeSuffixValues[num].sizeSuffix;
			}
		}
		return string.Empty;
	}

	private static string GetZoneId(E_SpawnWaveDetailedZone zone)
	{
		switch (zone)
		{
		case E_SpawnWaveDetailedZone.Zone_N:
		case E_SpawnWaveDetailedZone.Zone_N_NE:
		case E_SpawnWaveDetailedZone.Zone_N_NW:
			return "Right";
		case E_SpawnWaveDetailedZone.Zone_E_NE:
		case E_SpawnWaveDetailedZone.Zone_E:
		case E_SpawnWaveDetailedZone.Zone_E_SE:
			return "Right";
		case E_SpawnWaveDetailedZone.Zone_S_SE:
		case E_SpawnWaveDetailedZone.Zone_S:
		case E_SpawnWaveDetailedZone.Zone_S_SW:
			return "Bot";
		case E_SpawnWaveDetailedZone.Zone_W_SW:
		case E_SpawnWaveDetailedZone.Zone_W:
		case E_SpawnWaveDetailedZone.Zone_W_NW:
			return "Bot";
		case E_SpawnWaveDetailedZone.Zone_NE:
			return "TopRight";
		case E_SpawnWaveDetailedZone.Zone_SE:
		case E_SpawnWaveDetailedZone.Zone_NW:
			return "BotRight";
		case E_SpawnWaveDetailedZone.Zone_SW:
			return "BotLeft";
		default:
			return string.Empty;
		}
	}
}
