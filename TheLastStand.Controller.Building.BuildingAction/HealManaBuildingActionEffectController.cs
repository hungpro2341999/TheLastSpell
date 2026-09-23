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
/// Controller xử lý hiệu ứng "Hồi Năng Lượng / Mana" (Heal Mana Effect) từ công trình (ví dụ: Giếng Mana / Magic Well).
/// Hỗ trợ phục hồi Mana cho một Hero mục tiêu hoặc toàn đội hình, kích hoạt animation nổi RestoreStatDisplay
/// và cập nhật lại chỉ số Mana trên thanh chân dung Hero (Portraits Panel).
/// </summary>
public class HealManaBuildingActionEffectController : BuildingActionEffectController
{
	#region Properties & Model

	/// <summary>
	/// Model chuyên biệt lưu trữ dữ liệu hồi mana của hành động.
	/// </summary>
	public HealManaBuildingActionEffect HealManaBuildingActionEffect => base.BuildingActionEffect as HealManaBuildingActionEffect;

	#endregion

	#region Constructor

	/// <summary>
	/// Khởi tạo controller cho hiệu ứng hồi mana từ công trình.
	/// </summary>
	/// <param name="definition">Định nghĩa dữ liệu cấu hình hồi mana (lượng hồi, cơ chế chọn mục tiêu).</param>
	/// <param name="productionBuilding">Module sản xuất của công trình liên kết.</param>
	public HealManaBuildingActionEffectController(HealManaBuildingActionEffectDefinition definition, ProductionModule productionBuilding)
		: base(definition, productionBuilding)
	{
		base.BuildingActionEffect = new HealManaBuildingActionEffect(definition, this, productionBuilding);
	}

	#endregion

	#region Tile Targeting Validation

	/// <summary>
	/// Kiểm tra tính hợp lệ trên ô Tile chỉ định:
	/// - All: Luôn hợp lệ (tác động toàn đội).
	/// - Single: Ô phải có PlayableUnit và lượng Mana hiện tại phải nhỏ hơn Mana tối đa (chưa đầy Mana).
	/// </summary>
	/// <param name="tile">Ô Tile mục tiêu.</param>
	/// <returns>True nếu ô hợp lệ để hồi Mana.</returns>
	public override bool CanExecuteActionEffectOnTile(Tile tile)
	{
		return HealManaBuildingActionEffect.HealManaBuildingActionDefinition.BuildingActionTargeting switch
		{
			BuildingActionEffectDefinition.E_BuildingActionTargeting.All => true, 
			BuildingActionEffectDefinition.E_BuildingActionTargeting.Single => tile.Unit is PlayableUnit playableUnit && playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.Mana) < playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.ManaTotal), 
			_ => false, 
		};
	}

	#endregion

	#region Action Execution Logic

	/// <summary>
	/// Thực thi hiệu ứng hồi Mana cho tất cả Hero hoặc Hero đơn lẻ được chọn.
	/// </summary>
	public override void ExecuteActionEffect()
	{
		switch (HealManaBuildingActionEffect.HealManaBuildingActionDefinition.BuildingActionTargeting)
		{
		case BuildingActionEffectDefinition.E_BuildingActionTargeting.All:
		{
			// Hồi Mana cho tất cả Hero đang chơi
			foreach (PlayableUnit playableUnit2 in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
			{
				HealMana(playableUnit2, HealManaBuildingActionEffect.HealManaBuildingActionDefinition.Amount);
			}
			break;
		}
		case BuildingActionEffectDefinition.E_BuildingActionTargeting.Single:
			// Hồi Mana cho Hero được click chọn
			if (base.BuildingActionEffect.Target.Unit is PlayableUnit playableUnit)
			{
				HealMana(playableUnit, HealManaBuildingActionEffect.HealManaBuildingActionDefinition.Amount);
			}
			else
			{
				TPSingleton<BuildingManager>.Instance.LogError("Selected Unit is not a playable.");
			}
			break;
		}
	}

	/// <summary>
	/// Phục hồi Mana cho PlayableUnit, hiển thị animation chữ số Mana nổi lên và cập nhật UI chân dung nhân vật.
	/// </summary>
	/// <param name="playableUnit">Hero nhận Mana.</param>
	/// <param name="amount">Lượng Mana cộng thêm.</param>
	private void HealMana(PlayableUnit playableUnit, int amount)
	{
		TPSingleton<BuildingManager>.Instance.Log($"Adding {amount} mana to {playableUnit.Id}.");
		// Gọi hàm tăng Mana trên controller của Hero
		float num = playableUnit.PlayableUnitController.GainMana(amount);
		if (!(num <= 0f))
		{
			// Hiển thị animation thông báo khôi phục chỉ số Mana (+Mana)
			RestoreStatDisplay pooledComponent = ObjectPooler.GetPooledComponent("RestoreStatDisplay", ResourcePooler.LoadOnce<RestoreStatDisplay>("Prefab/Displayable Effect/UI Effect Displays/RestoreStatDisplay"), EffectManager.EffectDisplaysParent);
			pooledComponent.Init(UnitStatDefinition.E_Stat.Mana, (int)num);
			playableUnit.PlayableUnitController.AddEffectDisplay(pooledComponent);
			
			// Cập nhật lại thanh trạng thái và số liệu Mana trên panel chân dung Hero ở góc trên màn hình
			GameView.TopScreenPanel.UnitPortraitsPanel.RefreshPortraitsStats();
		}
	}

	#endregion
}

