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
	#region Fields & Properties
	private Coroutine displayEffectsCoroutine;

	/// <summary>
	/// Model bản vẽ/chiếm ô (BlueprintModule) của công trình.
	/// </summary>
	public BlueprintModule BlueprintModule { get; }
	#endregion

	#region Initialization & Factory
	/// <summary>
	/// Khởi tạo Controller bản vẽ công trình.
	/// </summary>
	public BlueprintModuleController(BuildingController buildingControllerParent, BlueprintModuleDefinition blueprintModuleDefinition)
		: base(buildingControllerParent, blueprintModuleDefinition)
	{
		BlueprintModule = base.BuildingModule as BlueprintModule;
	}

	/// <summary>
	/// Khởi tạo Model BlueprintModule tương ứng.
	/// </summary>
	protected override BuildingModule CreateModel(TheLastStand.Model.Building.Building building, BuildingModuleDefinition buildingModuleDefinition)
	{
		return new BlueprintModule(building, blueprintModuleDefinition as BlueprintModuleDefinition, this);
	}
	#endregion

	#region Effect Display
	/// <summary>
	/// Thêm hiệu ứng kỹ năng vào danh sách hiển thị của công trình và đăng ký với EffectManager.
	/// </summary>
	public void AddEffectDisplay(IDisplayableEffect displayableEffect)
	{
		base.BuildingControllerParent.Building.BuildingView.AddSkillEffectDisplay(displayableEffect);
		EffectManager.Register(this);
	}

	/// <summary>
	/// Kích hoạt Coroutine hiển thị các hiệu ứng kỹ năng đang chờ.
	/// </summary>
	public void DisplayEffects(float delay = 0f)
	{
		if (displayEffectsCoroutine == null)
		{
			displayEffectsCoroutine = TPSingleton<GameManager>.Instance.StartCoroutine(DisplayEffectsCoroutine(delay));
		}
	}

	/// <summary>
	/// Trả về số lượng hiệu ứng kỹ năng hiện tại của công trình.
	/// </summary>
	public int GetEffectsCount()
	{
		return base.BuildingControllerParent.BuildingView.SkillEffectDisplays.Count;
	}

	/// <summary>
	/// Coroutine thực thi hiển thị hiệu ứng kỹ năng và hủy đăng ký với EffectManager sau khi hoàn thành.
	/// </summary>
	private IEnumerator DisplayEffectsCoroutine(float delay)
	{
		yield return base.BuildingControllerParent.BuildingView.DisplaySkillEffects(delay);
		EffectManager.Unregister(this);
		displayEffectsCoroutine = null;
	}
	#endregion

	#region Tile Management & Spatial Queries
	/// <summary>
	/// Giải phóng các ô (Tile) mà công trình đang chiếm giữ khi công trình bị hủy hoặc di dời,
	/// bao gồm cả việc cập nhật lại vùng DeadZone nếu là loại OccupationVolumeType.Adjacent.
	/// </summary>
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

	/// <summary>
	/// Lấy danh sách các ô kề cạnh (4 hướng) xung quanh các ô công trình chiếm giữ.
	/// </summary>
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

	/// <summary>
	/// Lấy danh sách các ô kề xung quanh kể cả đường chéo (8 hướng).
	/// </summary>
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

	/// <summary>
	/// Lấy danh sách các ô nằm trong khoảng tầm xa (range) tính từ các ô công trình đang chiếm giữ.
	/// </summary>
	public List<Tile> GetTilesInRange(int maxRange, int minRange = 0, bool cardinalOnly = false)
	{
		return BlueprintModule.OccupiedTiles.GetTilesInRange(maxRange, minRange, cardinalOnly);
	}

	/// <summary>
	/// Lấy từ điển các ô trong khoảng tầm xa kèm theo ô công trình gần nhất tương ứng.
	/// </summary>
	public Dictionary<Tile, Tile> GetTilesInRangeWithClosestOccupiedTile(int maxRange, int minRange = 0, bool cardinalOnly = false)
	{
		return BlueprintModule.OccupiedTiles.GetTilesInRangeWithClosestOccupiedTile(maxRange, minRange, cardinalOnly);
	}
	#endregion
}
