using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Model.Building.BuildingGaugeEffect;
using TheLastStand.Model.Building.BuildingUpgrade;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

/// <summary>
/// Controller xử lý hiệu ứng nâng cấp tăng chỉ số thanh năng lượng/sản xuất (Gauge Effect) của công trình (như tăng lượng Vàng nhận được "GainGold" hoặc Materials "GainMaterials").
/// </summary>
public class ImproveGaugeEffectController : BuildingUpgradeEffectController
{
	#region Properties

	/// <summary>
	/// Model dữ liệu hiệu ứng nâng cấp thanh năng lượng.
	/// </summary>
	public ImproveGaugeEffect ImproveGaugeEffect => base.BuildingUpgradeEffect as ImproveGaugeEffect;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo controller nâng cấp Gauge Effect với định nghĩa và model nâng cấp công trình tương ứng.
	/// </summary>
	public ImproveGaugeEffectController(ImproveGaugeEffectDefinition definition, TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade buildingUpgrade)
	{
		base.BuildingUpgradeEffect = new ImproveGaugeEffect(definition, this, buildingUpgrade);
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Kích hoạt tăng giá trị phần thưởng sản xuất (Vàng hoặc Vật liệu) dựa trên ID định nghĩa hiệu ứng thanh Gauge.
	/// </summary>
	public override void TriggerEffect(bool onLoad = false)
	{
		switch (base.BuildingUpgradeEffect.BuildingUpgrade.Building.ProductionModule.BuildingGaugeEffect.BuildingGaugeEffectDefinition.Id)
		{
		case "GainGold":
			(base.BuildingUpgradeEffect.BuildingUpgrade.Building.ProductionModule.BuildingGaugeEffect as GainGold).UpgradedBonusValue += ImproveGaugeEffect.ImproveGaugeEffectDefinition.Value;
			break;
		case "GainMaterials":
			(base.BuildingUpgradeEffect.BuildingUpgrade.Building.ProductionModule.BuildingGaugeEffect as GainMaterials).UpgradedBonusValue += ImproveGaugeEffect.ImproveGaugeEffectDefinition.Value;
			break;
		}
	}

	#endregion
}

