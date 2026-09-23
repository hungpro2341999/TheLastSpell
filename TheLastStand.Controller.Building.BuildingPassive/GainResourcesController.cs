using TPLib;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Building.BuildingPassive;

/// <summary>
/// Bộ điều khiển hiệu ứng nội tại nhận tài nguyên (Gain Resources).
/// Tự động cộng Vàng (Gold), Vật liệu (Materials) hoặc Linh hồn đày đọa (Damned Souls),
/// đồng thời khởi tạo hiệu ứng chữ bay hiển thị trên công trình tương ứng.
/// </summary>
public class GainResourcesController : BuildingPassiveEffectController
{
	#region Properties

	/// <summary>
	/// Model lưu trữ định nghĩa và dữ liệu nhận tài nguyên.
	/// </summary>
	public GainResources GainResources => base.BuildingPassiveEffect as GainResources;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo GainResourcesController với module nội tại và định nghĩa tài nguyên cộng thêm.
	/// </summary>
	/// <param name="buildingPassivesModule">Module nội tại của công trình.</param>
	/// <param name="gainResourcesDefinition">Định nghĩa số lượng tài nguyên nhận được.</param>
	public GainResourcesController(PassivesModule buildingPassivesModule, GainResourcesDefinition gainResourcesDefinition)
	{
		base.BuildingPassiveEffect = new GainResources(buildingPassivesModule, gainResourcesDefinition, this);
	}

	#endregion

	#region Passive Effect Execution

	/// <summary>
	/// Thực thi cộng tài nguyên cho người chơi và hiển thị các UI số nổi (Floating UI Text) tương ứng.
	/// </summary>
	public override void Apply()
	{
		TheLastStand.Model.Building.Building buildingParent = base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent;
		int gainMaterials = GainResources.GainResourcesDefinition.GainMaterials;
		int gainGold = GainResources.GainResourcesDefinition.GainGold;
		int gainDamnedSouls = GainResources.GainResourcesDefinition.GainDamnedSouls;

		// Cập nhật giá trị vào các kho tài nguyên của trò chơi
		TPSingleton<ResourceManager>.Instance.SetGold(TPSingleton<ResourceManager>.Instance.Gold + gainGold);
		TPSingleton<ResourceManager>.Instance.Materials += gainMaterials;
		ApplicationManager.Application.DamnedSouls += (uint)gainDamnedSouls;

		// Hiển thị UI hiệu ứng nhận Vàng nếu có
		if (gainGold > 0)
		{
			GainGoldDisplay pooledComponent = ObjectPooler.GetPooledComponent("GainGoldDisplay", ResourcePooler.LoadOnce<GainGoldDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainGoldDisplay"), EffectManager.EffectDisplaysParent);
			pooledComponent.Init(gainGold);
			buildingParent.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent);
		}

		// Hiển thị UI hiệu ứng nhận Vật liệu xây dựng nếu có
		if (gainMaterials > 0)
		{
			GainMaterialDisplay pooledComponent2 = ObjectPooler.GetPooledComponent("GainMaterialDisplay", ResourcePooler.LoadOnce<GainMaterialDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainMaterialDisplay"), EffectManager.EffectDisplaysParent);
			pooledComponent2.Init(gainMaterials);
			buildingParent.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent2);
		}

		// Hiển thị UI hiệu ứng nhận Damned Souls nếu có
		if (gainDamnedSouls > 0)
		{
			GainDamnedSoulsDisplay pooledComponent3 = ObjectPooler.GetPooledComponent("GainDamnedSoulsDisplay", ResourcePooler.LoadOnce<GainDamnedSoulsDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainDamnedSoulsDisplay"), EffectManager.EffectDisplaysParent);
			pooledComponent3.Init(gainDamnedSouls);
			buildingParent.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent3);
		}
	}

	#endregion
}
