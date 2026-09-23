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
/// Controller xử lý hiệu ứng "Nhận Vật Liệu" (Gain Materials Effect) từ công trình (ví dụ: Xưởng cưa, Bãi khai thác đá, Tòa nhà sản xuất vật liệu).
/// Cộng số lượng vật liệu (Materials) vào kho tổng dùng để xây dựng/sửa chữa và hiển thị animation hiệu ứng (GainMaterialDisplay).
/// </summary>
public class GainMaterialsBuildingActionEffectController : BuildingActionEffectController
{
	#region Properties & Model

	/// <summary>
	/// Model chuyên biệt lưu trữ dữ liệu nhận vật liệu của hành động.
	/// </summary>
	public GainMaterialsBuildingActionEffect GainMaterialsBuildingActionEffect => base.BuildingActionEffect as GainMaterialsBuildingActionEffect;

	#endregion

	#region Constructor

	/// <summary>
	/// Khởi tạo controller cho hiệu ứng nhận vật liệu từ công trình.
	/// </summary>
	/// <param name="definition">Định nghĩa dữ liệu cấu hình hiệu ứng nhận vật liệu.</param>
	/// <param name="productionBuilding">Module sản xuất của công trình liên kết.</param>
	public GainMaterialsBuildingActionEffectController(GainMaterialsBuildingActionEffectDefinition definition, ProductionModule productionBuilding)
		: base(definition, productionBuilding)
	{
		base.BuildingActionEffect = new GainMaterialsBuildingActionEffect(definition, this, productionBuilding);
	}

	#endregion

	#region Tile Targeting Validation

	/// <summary>
	/// Kiểm tra tính hợp lệ trên ô Tile chỉ định. Luôn trả về true.
	/// </summary>
	public override bool CanExecuteActionEffectOnTile(Tile tile)
	{
		return true;
	}

	#endregion

	#region Action Execution Logic

	/// <summary>
	/// Thực thi hiệu ứng nhận vật liệu:
	/// - Đảm bảo công trình không bị phá hủy.
	/// - Cộng trực tiếp số lượng vật liệu vào ResourceManager.Instance.Materials.
	/// - Khởi tạo animation nổi GainMaterialDisplay trên công trình.
	/// </summary>
	public override void ExecuteActionEffect()
	{
		// Kiểm tra công trình vẫn còn sống hoặc thuộc dạng không thể phá hủy
		if (base.BuildingActionEffect.ProductionBuilding.BuildingParent.BlueprintModule.IsIndestructible || !base.BuildingActionEffect.ProductionBuilding.BuildingParent.DamageableModule.IsDead)
		{
			// Cộng vật liệu vào kho tài nguyên dùng chung
			TPSingleton<ResourceManager>.Instance.Materials += GainMaterialsBuildingActionEffect.GainMaterialsBuildingActionDefinition.GainMaterials;
			TPSingleton<BuildingManager>.Instance.Log($"Gaining {GainMaterialsBuildingActionEffect.GainMaterialsBuildingActionDefinition.GainMaterials} Materials from GainMaterials building action effect.");
			
			// Hiển thị animation số vật liệu (+Materials) nổi lên trên đầu công trình
			GainMaterialDisplay pooledComponent = ObjectPooler.GetPooledComponent("GainMaterialDisplay", ResourcePooler.LoadOnce<GainMaterialDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainMaterialDisplay"), EffectManager.EffectDisplaysParent);
			pooledComponent.Init(GainMaterialsBuildingActionEffect.GainMaterialsBuildingActionDefinition.GainMaterials);
			base.BuildingActionEffect.ProductionBuilding.BuildingParent.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent);
		}
	}

	#endregion
}

