using TPLib;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.View;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Building.BuildingAction;

/// <summary>
/// Controller xử lý hiệu ứng "Nâng cấp chỉ số tướng / Hero" (Upgrade Stat Effect) từ công trình (ví dụ: Trường bắn, Nhà rèn, Đền thờ).
/// Tăng hoặc giảm trực tiếp chỉ số cơ bản (Base Stat) của một Hero đơn lẻ hoặc toàn bộ Hero trên bản đồ,
/// kèm theo hiển thị hoạt ảnh chỉ số tăng (UpgradeStatDisplay) và làm mới HUD chân dung nhân vật.
/// </summary>
public class UpgradeStatBuildingActionEffectController : BuildingActionEffectController
{
	#region Properties & Model

	/// <summary>
	/// Model chuyên biệt lưu trữ dữ liệu nâng cấp chỉ số.
	/// </summary>
	public UpgradeStatBuildingActionEffect UpgradeStatBuildingActionEffect => base.BuildingActionEffect as UpgradeStatBuildingActionEffect;

	#endregion

	#region Constructor

	/// <summary>
	/// Khởi tạo controller cho hiệu ứng nâng cấp chỉ số.
	/// </summary>
	/// <param name="definition">Định nghĩa dữ liệu chỉ số cần tăng (Stat, giá trị thưởng Bonus, mục tiêu All/Single).</param>
	/// <param name="productionBuilding">Module sản xuất của công trình liên kết.</param>
	public UpgradeStatBuildingActionEffectController(UpgradeStatBuildingActionEffectDefinition definition, ProductionModule productionBuilding)
		: base(definition, productionBuilding)
	{
		base.BuildingActionEffect = new UpgradeStatBuildingActionEffect(definition, this, productionBuilding);
	}

	#endregion

	#region Tile Targeting Validation

	/// <summary>
	/// Kiểm tra tính hợp lệ trên ô Tile chỉ định:
	/// - All: Luôn hợp lệ (tác động tất cả).
	/// - Single: Ô phải có một đơn vị điều khiển được (PlayableUnit).
	/// </summary>
	/// <param name="tile">Ô Tile mục tiêu cần kiểm tra.</param>
	/// <returns>True nếu ô mục tiêu hợp lệ để nâng cấp chỉ số.</returns>
	public override bool CanExecuteActionEffectOnTile(Tile tile)
	{
		return UpgradeStatBuildingActionEffect.UpgradeStatBuildingActionDefinition.BuildingActionTargeting switch
		{
			BuildingActionEffectDefinition.E_BuildingActionTargeting.All => true, 
			BuildingActionEffectDefinition.E_BuildingActionTargeting.Single => tile.Unit is PlayableUnit, 
			_ => false, 
		};
	}

	#endregion

	#region Action Execution Logic

	/// <summary>
	/// Thực thi tăng chỉ số: Duyệt tất cả Hero nếu là All, hoặc áp dụng cho Hero tại ô được chỉ định nếu là Single.
	/// </summary>
	public override void ExecuteActionEffect()
	{
		switch (UpgradeStatBuildingActionEffect.UpgradeStatBuildingActionDefinition.BuildingActionTargeting)
		{
		case BuildingActionEffectDefinition.E_BuildingActionTargeting.All:
		{
			// Nâng cấp chỉ số cho tất cả Hero
			foreach (PlayableUnit playableUnit2 in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
			{
				UpgradeStat(playableUnit2, UpgradeStatBuildingActionEffect.UpgradeStatBuildingActionDefinition.Stat, UpgradeStatBuildingActionEffect.UpgradeStatBuildingActionDefinition.Bonus);
			}
			break;
		}
		case BuildingActionEffectDefinition.E_BuildingActionTargeting.Single:
			// Nâng cấp chỉ số cho Hero được chọn cụ thể
			if (base.BuildingActionEffect.Target.Unit is PlayableUnit playableUnit)
			{
				UpgradeStat(playableUnit, UpgradeStatBuildingActionEffect.UpgradeStatBuildingActionDefinition.Stat, UpgradeStatBuildingActionEffect.UpgradeStatBuildingActionDefinition.Bonus);
			}
			else
			{
				TPSingleton<BuildingManager>.Instance.LogError("Selected Unit is not a playable.");
			}
			break;
		}
	}

	/// <summary>
	/// Thay đổi chỉ số cơ bản của Hero và hiển thị hiệu ứng UI.
	/// </summary>
	/// <param name="playableUnit">Hero mục tiêu.</param>
	/// <param name="stat">Loại chỉ số cần thay đổi (Health, Damage, Armor, AP, Move Points, v.v.).</param>
	/// <param name="bonus">Giá trị cộng thêm (nếu âm sẽ trừ chỉ số).</param>
	private void UpgradeStat(PlayableUnit playableUnit, UnitStatDefinition.E_Stat stat, int bonus)
	{
		// Lấy component animation từ ObjectPooler để hiển thị icon và số điểm tăng
		UpgradeStatDisplay pooledComponent = ObjectPooler.GetPooledComponent("UpgradeStatDisplay", ResourcePooler.LoadOnce<UpgradeStatDisplay>("Prefab/Displayable Effect/UI Effect Displays/UpgradeStatDisplay"), EffectManager.EffectDisplaysParent);
		pooledComponent.Init(stat, bonus);
		playableUnit.PlayableUnitController.AddEffectDisplay(pooledComponent);
		
		// Cập nhật giá trị chỉ số gốc thông qua PlayableUnitStatsController
		if (bonus >= 0)
		{
			playableUnit.PlayableUnitStatsController.IncreaseBaseStat(stat, bonus, includeChildStat: true);
		}
		else
		{
			playableUnit.PlayableUnitStatsController.DecreaseBaseStat(stat, -bonus, includeChildStat: false);
		}
		
		// Làm mới panel chân dung Hero để phản ánh chỉ số mới ngay lập tức
		GameView.TopScreenPanel.UnitPortraitsPanel.RefreshPortraitsStats();
		TPSingleton<BuildingManager>.Instance.Log($"UpgradeStat {stat} of {playableUnit.Id} by {bonus}.");
	}

	#endregion
}

