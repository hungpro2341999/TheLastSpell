using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Definition.Building.BuildingGaugeEffect;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building.BuildingGaugeEffect;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Unit;
using TheLastStand.Serialization;
using TheLastStand.View.Building.BuildingGaugeEffect;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Building.BuildingGaugeEffect;

/// <summary>
/// Controller xử lý hiệu ứng "Nâng cấp chỉ số tướng toàn đội" (Upgrade Stat Gauge Effect) khi thanh tiến độ công trình hoàn thành.
/// Áp dụng cho các công trình huấn luyện / bùa chú đặc biệt (như Đền thờ hoặc Thao trường):
/// - Khi đạt mốc ngưỡng sản xuất, tự động duyệt qua tất cả Hero đang tham chiến (PlayableUnit).
/// - Tăng (hoặc giảm) chỉ số cơ bản (BaseStat) tương ứng với giá trị Bonus trong định nghĩa cấu hình.
/// - Hiển thị animation thông báo nâng cấp chỉ số nổi bật (UpgradeStatDisplay) trên mỗi Hero.
/// </summary>
public class UpgradeStatGaugeEffectController : BuildingGaugeEffectController
{
	#region Constructors

	/// <summary>
	/// Khởi tạo controller từ dữ liệu đã lưu (Save game).
	/// </summary>
	/// <param name="container">Dữ liệu tuần tự hóa trạng thái tiến độ tích lũy.</param>
	/// <param name="productionBuilding">Module sản xuất của công trình liên kết.</param>
	/// <param name="definition">Định nghĩa cấu hình hiệu ứng nâng cấp chỉ số.</param>
	public UpgradeStatGaugeEffectController(SerializedGaugeEffect container, ProductionModule productionBuilding, BuildingGaugeEffectDefinition definition)
	{
		base.BuildingGaugeEffect = new UpgradeStatGaugeEffect(productionBuilding, definition, this, new UpgradeStatView());
		base.BuildingGaugeEffect.BuildingGaugeEffectView.BuildingGaugeEffect = base.BuildingGaugeEffect;
		base.BuildingGaugeEffect.Deserialize(container);
	}

	/// <summary>
	/// Khởi tạo mới controller khi công trình được xây dựng lần đầu.
	/// </summary>
	/// <param name="productionBuilding">Module sản xuất của công trình liên kết.</param>
	/// <param name="definition">Định nghĩa cấu hình hiệu ứng nâng cấp chỉ số.</param>
	public UpgradeStatGaugeEffectController(ProductionModule productionBuilding, BuildingGaugeEffectDefinition definition)
	{
		base.BuildingGaugeEffect = new UpgradeStatGaugeEffect(productionBuilding, definition, this, new UpgradeStatView());
		base.BuildingGaugeEffect.BuildingGaugeEffectView.BuildingGaugeEffect = base.BuildingGaugeEffect;
	}

	#endregion

	#region Trigger Effect Execution

	/// <summary>
	/// Kích hoạt hiệu ứng hoàn thành thanh tiến độ:
	/// 1. Lấy dữ liệu loại chỉ số và lượng chỉ số thưởng (Stat, Bonus).
	/// 2. Duyệt qua tất cả các PlayableUnit trong PlayableUnitManager.
	/// 3. Khởi tạo và gắn animation nổi UpgradeStatDisplay lên từng Hero.
	/// 4. Thay đổi chỉ số cơ bản (IncreaseBaseStat / DecreaseBaseStat) trên PlayableUnitStatsController của Hero.
	/// </summary>
	/// <returns>Danh sách controller của tất cả Hero chịu tác động của hiệu ứng.</returns>
	public override List<IEffectTargetSkillActionController> TriggerEffect()
	{
		List<IEffectTargetSkillActionController> list = base.TriggerEffect();
		UpgradeStatGaugeEffect upgradeStatGaugeEffect = base.BuildingGaugeEffect as UpgradeStatGaugeEffect;
		
		// Áp dụng nâng cấp chỉ số cho từng Hero trong đội hình
		foreach (PlayableUnit playableUnit in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
		{
			UpgradeStatDisplay pooledComponent = ObjectPooler.GetPooledComponent("UpgradeStatDisplay", ResourcePooler.LoadOnce<UpgradeStatDisplay>("Prefab/Displayable Effect/UI Effect Displays/UpgradeStatDisplay"), EffectManager.EffectDisplaysParent);
			pooledComponent.Init(upgradeStatGaugeEffect.UpgradeStatGaugeEffectDefinition.UpgradeStatDefinition.Stat, upgradeStatGaugeEffect.UpgradeStatGaugeEffectDefinition.UpgradeStatDefinition.Bonus);
			playableUnit.PlayableUnitController.AddEffectDisplay(pooledComponent);
			list.Add(playableUnit.PlayableUnitController);
			
			// Cập nhật chỉ số cơ bản cho Hero
			if (upgradeStatGaugeEffect.UpgradeStatGaugeEffectDefinition.UpgradeStatDefinition.Bonus >= 0)
			{
				playableUnit.PlayableUnitStatsController.IncreaseBaseStat(upgradeStatGaugeEffect.UpgradeStatGaugeEffectDefinition.UpgradeStatDefinition.Stat, upgradeStatGaugeEffect.UpgradeStatGaugeEffectDefinition.UpgradeStatDefinition.Bonus, includeChildStat: true);
			}
			else
			{
				playableUnit.PlayableUnitStatsController.DecreaseBaseStat(upgradeStatGaugeEffect.UpgradeStatGaugeEffectDefinition.UpgradeStatDefinition.Stat, -upgradeStatGaugeEffect.UpgradeStatGaugeEffectDefinition.UpgradeStatDefinition.Bonus, includeChildStat: false);
			}
		}
		TPSingleton<BuildingManager>.Instance.Log($"({base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingDefinition.Id}) UpgradeStat ({upgradeStatGaugeEffect.UpgradeStatGaugeEffectDefinition.UpgradeStatDefinition.Bonus} {upgradeStatGaugeEffect.UpgradeStatGaugeEffectDefinition.UpgradeStatDefinition.Stat})", CLogLevel.MAJOR);
		return list;
	}

	#endregion
}

