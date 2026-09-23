using System.Collections.Generic;
using TPLib;
using TheLastStand.Controller.CastFx;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Definition.CastFx;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingUpgrade;
using TheLastStand.Model.CastFx;
using TheLastStand.Model.TileMap;
using TheLastStand.Serialization;
using UnityEngine;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

/// <summary>
/// Controller quản lý tiến trình nâng cấp của một công trình (Building Upgrade).
/// Xử lý logic kiểm tra tài nguyên, mở khóa cấp độ nâng cấp, áp dụng hiệu ứng và phát hiệu ứng hình ảnh (CastFx).
/// </summary>
public class BuildingUpgradeController
{
	#region Properties

	/// <summary>
	/// Model dữ liệu đại diện cho nâng cấp công trình tương ứng.
	/// </summary>
	public TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade BuildingUpgrade { get; protected set; }

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo controller nâng cấp công trình từ dữ liệu đã được lưu (Serialized).
	/// </summary>
	/// <param name="container">Dữ liệu nâng cấp đã được serialization.</param>
	/// <param name="building">Công trình sở hữu nâng cấp này.</param>
	public BuildingUpgradeController(SerializedUpgrade container, TheLastStand.Model.Building.Building building)
	{
		BuildingUpgradeDefinition definition = BuildingDatabase.BuildingUpgradeDefinitions[container.Id];
		BuildingUpgrade = CreateModel(container, definition, this, building);
		InitializeCastFxs();
	}

	/// <summary>
	/// Khởi tạo controller nâng cấp công trình từ định nghĩa nâng cấp (Definition).
	/// </summary>
	/// <param name="definition">Định nghĩa nâng cấp công trình.</param>
	/// <param name="building">Công trình sở hữu nâng cấp này.</param>
	public BuildingUpgradeController(BuildingUpgradeDefinition definition, TheLastStand.Model.Building.Building building)
	{
		BuildingUpgrade = CreateModel(definition, this, building);
		InitializeCastFxs();
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Thực hiện mở khóa cấp độ nâng cấp tiếp theo cho công trình.
	/// </summary>
	/// <param name="freeUpgrade">Nếu true, sẽ miễn phí không tiêu tốn Vàng (Gold) hay Vật liệu (Materials).</param>
	/// <param name="playFx">Nếu true, sẽ phát hiệu ứng thị giác/âm thanh khi nâng cấp thành công.</param>
	/// <param name="sendAnalytics">Nếu true, sẽ gửi dữ liệu phân tích (analytics) sự kiện mua nâng cấp.</param>
	public void UnlockUpgrade(bool freeUpgrade = false, bool playFx = true, bool sendAnalytics = false)
	{
		if ((!freeUpgrade && TPSingleton<ResourceManager>.Instance.Gold < BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[BuildingUpgrade.UpgradeLevel + 1].GoldCost) || TPSingleton<ResourceManager>.Instance.Materials < BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[BuildingUpgrade.UpgradeLevel + 1].MaterialCost)
		{
			TPSingleton<BuildingManager>.Instance.Log("Tried to unlock upgrade but the player does not have enough resources.");
			return;
		}
		if (sendAnalytics && Analytics.AllowedToSendData)
		{
			Analytics.SendPurchaseBuildingEvent(BuildingUpgrade.Building.BuildingDefinition.BlueprintModuleDefinition.Category.ToString(), BuildingUpgrade.Building.Id, $"{BuildingUpgrade.BuildingUpgradeDefinition.Id}_{BuildingUpgrade.UpgradeLevel + 1}");
		}
		BuildingUpgrade.UpgradeLevel++;
		if (!freeUpgrade)
		{
			TPSingleton<ResourceManager>.Instance.SetGold(TPSingleton<ResourceManager>.Instance.Gold - BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[BuildingUpgrade.UpgradeLevel].GoldCost);
			TPSingleton<ResourceManager>.Instance.Materials -= BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[BuildingUpgrade.UpgradeLevel].MaterialCost;
		}
		int i = 0;
		for (int count = BuildingUpgrade.BuildingUpgradeLevels[BuildingUpgrade.UpgradeLevel].Effects.Count; i < count; i++)
		{
			BuildingUpgrade.BuildingUpgradeLevels[BuildingUpgrade.UpgradeLevel].Effects[i].BuildingUpgradeEffectController.TriggerEffect();
		}
		if (playFx)
		{
			PlayFx();
		}
	}

	#endregion

	#region Protected Methods

	/// <summary>
	/// Phát hiệu ứng kỹ năng / thi triển (CastFx) tương ứng với cấp nâng cấp hiện tại.
	/// </summary>
	protected virtual void PlayFx()
	{
		(BuildingUpgrade.BuildingUpgradeLevels[BuildingUpgrade.UpgradeLevel].OverrideCastFx ?? BuildingUpgrade.CastFx)?.CastFxController.PlayCastFxs(TileObjectSelectionManager.E_Orientation.NONE, default(Vector2), BuildingUpgrade.Building);
	}

	/// <summary>
	/// Tạo đối tượng Model dữ liệu cho nâng cấp công trình (từ dữ liệu lưu trữ).
	/// </summary>
	protected virtual TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade CreateModel(SerializedUpgrade container, BuildingUpgradeDefinition definition, BuildingUpgradeController controller, TheLastStand.Model.Building.Building building)
	{
		return new TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade(container, definition, controller, building);
	}

	/// <summary>
	/// Tạo đối tượng Model dữ liệu cho nâng cấp công trình (từ định nghĩa).
	/// </summary>
	protected virtual TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade CreateModel(BuildingUpgradeDefinition definition, BuildingUpgradeController controller, TheLastStand.Model.Building.Building building)
	{
		return new TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade(definition, controller, building);
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Khởi tạo đối tượng CastFx cho định nghĩa hiệu ứng thi triển.
	/// </summary>
	private TheLastStand.Model.CastFx.CastFx InitializeCastFx(CastFxDefinition castFxDefinition)
	{
		TheLastStand.Model.CastFx.CastFx castFx = new CastFxController(castFxDefinition).CastFx;
		castFx.TargetTile = BuildingUpgrade.Building.OriginTile;
		castFx.SourceTile = BuildingUpgrade.Building.OriginTile;
		castFx.CastFXInterpreterContext = new CastFXInterpreterContext(castFx);
		castFx.AffectedTiles.Add(new List<Tile>(1) { castFx.TargetTile });
		return castFx;
	}

	/// <summary>
	/// Khởi tạo danh sách CastFx cho tất cả các level nâng cấp có định nghĩa Fx.
	/// </summary>
	private void InitializeCastFxs()
	{
		if (BuildingUpgrade.BuildingUpgradeDefinition.CastFxDefinition != null)
		{
			BuildingUpgrade.CastFx = InitializeCastFx(BuildingUpgrade.BuildingUpgradeDefinition.CastFxDefinition);
		}
		int i = 0;
		for (int count = BuildingUpgrade.BuildingUpgradeLevels.Count; i < count; i++)
		{
			if (BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[i].OverrideCastFxDefinition != null)
			{
				BuildingUpgrade.BuildingUpgradeLevels[i].OverrideCastFx = InitializeCastFx(BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[i].OverrideCastFxDefinition);
			}
		}
	}

	#endregion
}

