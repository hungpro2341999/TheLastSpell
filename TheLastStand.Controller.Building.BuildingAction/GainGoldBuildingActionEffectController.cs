using TPLib;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Building.BuildingAction;

/// <summary>
/// Controller xử lý hiệu ứng "Nhận Vàng" (Gain Gold Effect) từ hành động của công trình (ví dụ: Mỏ vàng, Nhà tạo tiền tệ).
/// Kiểm tra công trình còn hoạt động hay không, cộng vàng vào kho tài nguyên chung và kích hoạt hiệu ứng hiển thị số vàng bay lên (GainGoldDisplay).
/// </summary>
public class GainGoldBuildingActionEffectController : BuildingActionEffectController
{
	#region Properties & Model

	/// <summary>
	/// Model chuyên biệt lưu trữ dữ liệu nhận vàng của hành động.
	/// </summary>
	public GainGoldBuildingActionEffect GainGoldBuildingActionEffect => base.BuildingActionEffect as GainGoldBuildingActionEffect;

	#endregion

	#region Constructor

	/// <summary>
	/// Khởi tạo controller cho hiệu ứng nhận vàng từ công trình.
	/// </summary>
	/// <param name="definition">Định nghĩa dữ liệu cấu hình hiệu ứng nhận vàng.</param>
	/// <param name="productionBuilding">Module sản xuất của công trình liên kết.</param>
	public GainGoldBuildingActionEffectController(GainGoldBuildingActionEffectDefinition definition, ProductionModule productionBuilding)
		: base(definition, productionBuilding)
	{
		base.BuildingActionEffect = new GainGoldBuildingActionEffect(definition, this, productionBuilding);
	}

	#endregion

	#region Tile Targeting Validation

	/// <summary>
	/// Kiểm tra tính hợp lệ trên ô Tile chỉ định. Luôn trả về true vì đây là hành động nội tại sinh tài nguyên.
	/// </summary>
	public override bool CanExecuteActionEffectOnTile(Tile tile)
	{
		return true;
	}

	#endregion

	#region Action Execution Logic

	/// <summary>
	/// Thực thi hiệu ứng nhận vàng:
	/// - Đảm bảo công trình không bị phá hủy (bất tử hoặc chưa chết).
	/// - Tăng lượng vàng tổng thông qua ResourceManager.
	/// - Lấy prefab GainGoldDisplay từ ObjectPooler và hiển thị animation bay lên tại vị trí công trình.
	/// </summary>
	public override void ExecuteActionEffect()
	{
		// Kiểm tra điều kiện: công trình phải là loại bất hoại (indestructible) hoặc hiện vẫn còn sống
		if (base.BuildingActionEffect.ProductionBuilding.BuildingParent.BlueprintModule.IsIndestructible || !base.BuildingActionEffect.ProductionBuilding.BuildingParent.DamageableModule.IsDead)
		{
			// Cộng vàng vào kho tài nguyên tổng của người chơi
			TPSingleton<ResourceManager>.Instance.SetGold(TPSingleton<ResourceManager>.Instance.Gold + GainGoldBuildingActionEffect.GainGoldBuildingActionDefinition.GainGold);
			TPSingleton<BuildingManager>.Instance.Log($"Gaining {GainGoldBuildingActionEffect.GainGoldBuildingActionDefinition.GainGold} Gold from GainGold building action effect.");
			
			// Hiển thị chữ số vàng (+Gold) hiệu ứng nổi trên đầu công trình
			GainGoldDisplay pooledComponent = ObjectPooler.GetPooledComponent("GainGoldDisplay", ResourcePooler.LoadOnce<GainGoldDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainGoldDisplay"), EffectManager.EffectDisplaysParent);
			pooledComponent.Init(GainGoldBuildingActionEffect.GainGoldBuildingActionDefinition.GainGold);
			base.BuildingActionEffect.ProductionBuilding.BuildingParent.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent);
		}
	}

	#endregion
}

