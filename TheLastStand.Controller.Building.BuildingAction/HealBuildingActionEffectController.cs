using TPLib;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.View.Skill.SkillAction;

namespace TheLastStand.Controller.Building.BuildingAction;

/// <summary>
/// Controller xử lý hiệu ứng "Hồi Máu" (Heal Effect) từ công trình (ví dụ: Bệnh viện / Trạm y tế).
/// Hỗ trợ hồi máu cho một Hero mục tiêu chỉ định hoặc toàn bộ các Hero đồng minh trên chiến trường,
/// đồng thời cập nhật chỉ số vết thương (Injury stage) và hiển thị hiệu ứng hồi máu (HealFeedback).
/// </summary>
public class HealBuildingActionEffectController : BuildingActionEffectController
{
	#region Properties & Model

	/// <summary>
	/// Model chuyên biệt lưu trữ dữ liệu hồi máu của hành động.
	/// </summary>
	public HealBuildingActionEffect HealBuildingActionEffect => base.BuildingActionEffect as HealBuildingActionEffect;

	#endregion

	#region Constructor

	/// <summary>
	/// Khởi tạo controller cho hiệu ứng hồi máu của công trình.
	/// </summary>
	/// <param name="definition">Định nghĩa dữ liệu cấu hình hồi máu (lượng máu, phạm vi mục tiêu All/Single).</param>
	/// <param name="productionBuilding">Module sản xuất của công trình liên kết.</param>
	public HealBuildingActionEffectController(HealBuildingActionEffectDefinition definition, ProductionModule productionBuilding)
		: base(definition, productionBuilding)
	{
		base.BuildingActionEffect = new HealBuildingActionEffect(definition, this, productionBuilding);
	}

	#endregion

	#region Tile Targeting Validation

	/// <summary>
	/// Kiểm tra tính hợp lệ trên ô Tile chỉ định:
	/// - Nếu phạm vi là All: Luôn hợp lệ (không cần chọn ô cụ thể).
	/// - Nếu phạm vi là Single: Ô Tile phải có Hero đồng minh (PlayableUnit) và Hero đó chưa đầy máu.
	/// </summary>
	/// <param name="tile">Ô Tile mục tiêu được chọn.</param>
	/// <returns>True nếu có thể hồi máu cho mục tiêu trên ô này.</returns>
	public override bool CanExecuteActionEffectOnTile(Tile tile)
	{
		return HealBuildingActionEffect.HealBuildingActionDefinition.BuildingActionTargeting switch
		{
			BuildingActionEffectDefinition.E_BuildingActionTargeting.All => true, 
			BuildingActionEffectDefinition.E_BuildingActionTargeting.Single => tile.Unit is PlayableUnit playableUnit && playableUnit.Health < playableUnit.HealthTotal, 
			_ => false, 
		};
	}

	#endregion

	#region Action Execution Logic

	/// <summary>
	/// Thực thi hiệu ứng hồi máu:
	/// Duyệt hồi máu cho tất cả PlayableUnit nếu là All, hoặc hồi máu cho Hero tại ô đích nếu là Single.
	/// </summary>
	public override void ExecuteActionEffect()
	{
		switch (HealBuildingActionEffect.HealBuildingActionDefinition.BuildingActionTargeting)
		{
		case BuildingActionEffectDefinition.E_BuildingActionTargeting.All:
		{
			// Hồi máu cho toàn bộ đội hình Hero đang tham chiến
			foreach (PlayableUnit playableUnit2 in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
			{
				Heal(playableUnit2, HealBuildingActionEffect.HealBuildingActionDefinition.Amount);
			}
			break;
		}
		case BuildingActionEffectDefinition.E_BuildingActionTargeting.Single:
			// Hồi máu cho đơn vị Hero cụ thể được người chơi click chọn
			if (base.BuildingActionEffect.Target.Unit is PlayableUnit playableUnit)
			{
				Heal(playableUnit, HealBuildingActionEffect.HealBuildingActionDefinition.Amount);
			}
			else
			{
				TPSingleton<BuildingManager>.Instance.LogError("Selected Unit is not a playable.");
			}
			break;
		}
	}

	/// <summary>
	/// Thực hiện cộng máu cho Hero và hiển thị UI Feedback hồi phục.
	/// </summary>
	/// <param name="playableUnit">Hero được hồi máu.</param>
	/// <param name="amount">Lượng máu hồi phục.</param>
	private void Heal(PlayableUnit playableUnit, int amount)
	{
		TPSingleton<BuildingManager>.Instance.Log($"Healing {playableUnit.Id} by {amount} points.");
		// Thực hiện hồi máu trên PlayableUnitController (trả về lượng máu thực tế được hồi)
		float num = playableUnit.UnitController.GainHealth(amount, refreshHud: false);
		if (!(num <= 0f))
		{
			// Hiển thị số máu hồi phục nổi trên đầu nhân vật
			HealFeedback healFeedback = playableUnit.DamageableView.HealFeedback;
			healFeedback.AddHealInstance(num, playableUnit.Health);
			playableUnit.UnitController.AddEffectDisplay(healFeedback);
			
			// Cập nhật lại thanh máu và các mốc chấn thương (Injury Stage) trên giao diện HUD của Hero
			playableUnit.UnitView.UnitHUD.RefreshInjuryStage();
		}
	}

	#endregion
}

