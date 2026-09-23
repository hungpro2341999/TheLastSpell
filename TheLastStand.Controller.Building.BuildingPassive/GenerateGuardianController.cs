using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using UnityEngine;

namespace TheLastStand.Controller.Building.BuildingPassive;

/// <summary>
/// Bộ điều khiển hiệu ứng nội tại triệu hồi Vệ binh (Generate Guardian).
/// Quản lý việc sinh ra một đơn vị kẻ địch Vệ binh (Guardian Enemy Unit) bảo vệ công trình,
/// tính toán vị trí spawn tối ưu (hướng xa khỏi Magic Circle, tránh sương mù, dọn dẹp vật cản nếu cần)
/// và xử lý tiêu diệt vệ binh khi công trình bị hủy hoặc giải phóng liên kết.
/// </summary>
public class GenerateGuardianController : BuildingPassiveEffectController
{
	#region Properties

	/// <summary>
	/// Model lưu trữ dữ liệu hiệu ứng sinh Vệ binh và tham chiếu đến Vệ binh đang sống.
	/// </summary>
	public GenerateGuardian GenerateGuardian => base.BuildingPassiveEffect as GenerateGuardian;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo GenerateGuardianController với module nội tại và định nghĩa sinh vệ binh.
	/// </summary>
	/// <param name="buildingPassivesModule">Module nội tại của công trình cha.</param>
	/// <param name="generateGuardianDefinition">Định nghĩa cấu hình sinh vệ binh.</param>
	public GenerateGuardianController(PassivesModule buildingPassivesModule, GenerateGuardianDefinition generateGuardianDefinition)
	{
		base.BuildingPassiveEffect = new GenerateGuardian(buildingPassivesModule, generateGuardianDefinition, this);
	}

	#endregion

	#region Passive Effect Lifecycle

	/// <summary>
	/// Thực thi triệu hồi Vệ binh: Ưu tiên sinh ở ô nằm ngoài màn sương (Mist/Fog); 
	/// nếu không thể thì buộc phải sinh trong sương mù.
	/// </summary>
	public override void Apply()
	{
		if (!TrySpawnGuardian(throughFog: false))
		{
			TPSingleton<BuildingManager>.Instance.LogError("Could not spawn a guardian outside of mist, something went wrong.");
			if (!TrySpawnGuardian(throughFog: true))
			{
				TPSingleton<BuildingManager>.Instance.LogError("Could not spawn a guardian INSIDE of mist, something went really wrong!");
			}
		}
	}

	/// <summary>
	/// Hủy hiệu ứng: Tiêu diệt đơn vị Vệ binh hiện tại nếu đang còn sống (chuẩn bị chết và chạy hoạt ảnh tử vong).
	/// </summary>
	public override void Unapply()
	{
		EnemyUnit guardian = GenerateGuardian.Guardian;
		if (guardian != null && !guardian.IsDead && !guardian.IsDeathRattling)
		{
			GenerateGuardian.Guardian.UnitController.PrepareForDeath();
			GenerateGuardian.Guardian.UnitView.PlayDieAnim();
		}
	}

	/// <summary>
	/// Phản hồi khi công trình sở hữu bị phá hủy: Gỡ bỏ liên kết công trình khỏi đơn vị Vệ binh.
	/// </summary>
	public override void OnDeath()
	{
		base.OnDeath();
		if (GenerateGuardian.Guardian != null)
		{
			GenerateGuardian.Guardian.LinkedBuilding = null;
		}
	}

	#endregion

	#region Private Spawn Logic

	/// <summary>
	/// Cố gắng tìm kiếm vị trí phù hợp quanh công trình và triệu hồi đơn vị Vệ binh.
	/// </summary>
	/// <param name="throughFog">Cho phép sinh đơn vị bên trong sương mù hay không.</param>
	/// <returns>True nếu sinh thành công; ngược lại False.</returns>
	private bool TrySpawnGuardian(bool throughFog)
	{
		TheLastStand.Model.Building.Building building = base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent;
		List<Tile> list = building.BlueprintModule.BlueprintModuleController.GetAdjacentTilesWithDiagonals();

		// Lọc bỏ các ô có sương mù nếu không cho phép xuyên sương
		if (!throughFog)
		{
			list = list.Where((Tile tile) => !tile.HasFog).ToList();
		}

		// Xáo trộn ngẫu nhiên danh sách các ô kề bên
		list = RandomManager.Shuffle(TPSingleton<EnemyUnitManager>.Instance, list).ToList();
		Tile magicCircleTile = BuildingManager.MagicCircle.OriginTile;

		// Phân loại ô theo hướng vector: Ưu tiên các ô hướng ra ngoài, xa dần Vòng phép ma thuật (Magic Circle)
		list.Split((Tile adjacentTile) => Vector2.Dot(rhs: new Vector2(building.OriginTile.X - magicCircleTile.X, building.OriginTile.Y - magicCircleTile.Y), lhs: new Vector2(adjacentTile.X - building.OriginTile.X, adjacentTile.Y - building.OriginTile.Y)) <= 0f);
		EnemyUnitTemplateDefinition guardianToSpawn = TPSingleton<EnemyUnitManager>.Instance.GetGuardianToSpawn();

		// Lượt duyệt 1: Thử spawn ngay trên các ô hoàn toàn trống và hợp lệ
		foreach (Tile item in list)
		{
			if (guardianToSpawn.CanSpawnOn(item))
			{
				GenerateGuardian.Guardian = EnemyUnitManager.CreateEnemyUnit(guardianToSpawn, item, new UnitCreationSettings(null, castSpawnSkill: true, playSpawnAnim: true, playSpawnCutscene: true, waitSpawnAnim: false, -1, GenerateGuardian.BuildingPassivesModule.BuildingParent, isGuardian: true));
				return true;
			}
		}

		// Lượt duyệt 2: Thử đè lên các công trình phá hủy được (không phải công trình bất hoại)
		foreach (Tile item2 in list)
		{
			if (guardianToSpawn.CanSpawnOn(item2, isPhaseActor: false, ignoreUnits: false, ignoreBuildings: true))
			{
				List<Tile> occupiedTiles = item2.GetOccupiedTiles(guardianToSpawn);
				if (!occupiedTiles.Any((Tile occupiedTile) => occupiedTile.Building != null && occupiedTile.Building.BlueprintModule.IsIndestructible))
				{
					TileMapManager.ClearBuildingOnTiles(occupiedTiles);
					GenerateGuardian.Guardian = EnemyUnitManager.CreateEnemyUnit(guardianToSpawn, item2, new UnitCreationSettings(null, castSpawnSkill: true, playSpawnAnim: true, playSpawnCutscene: true, waitSpawnAnim: false, -1, GenerateGuardian.BuildingPassivesModule.BuildingParent, isGuardian: true));
					return true;
				}
			}
		}

		// Lượt duyệt 3: Đẩy dọn các đơn vị quân đồng minh và quái vật đang đứng trên ô để ép buộc spawn
		foreach (Tile item3 in list)
		{
			if (guardianToSpawn.CanSpawnOn(item3, isPhaseActor: false, ignoreUnits: true))
			{
				List<Tile> occupiedTiles2 = item3.GetOccupiedTiles(guardianToSpawn);
				TileMapManager.FreeTilesFromPlayableUnits(occupiedTiles2);
				TileMapManager.ClearEnemiesOnTiles(occupiedTiles2);
				GenerateGuardian.Guardian = EnemyUnitManager.CreateEnemyUnit(guardianToSpawn, item3, new UnitCreationSettings(null, castSpawnSkill: true, playSpawnAnim: true, playSpawnCutscene: true, waitSpawnAnim: false, -1, GenerateGuardian.BuildingPassivesModule.BuildingParent, isGuardian: true));
				return true;
			}
		}

		return false;
	}

	#endregion
}
