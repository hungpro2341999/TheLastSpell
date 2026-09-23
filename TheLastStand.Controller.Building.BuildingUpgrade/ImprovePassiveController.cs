using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Model.Building.BuildingUpgrade;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

/// <summary>
/// Controller xử lý hiệu ứng nâng cấp chỉ số/hiệu ứng nội tại (Passive Effect) của công trình.
/// </summary>
public class ImprovePassiveController : BuildingUpgradeEffectController
{
	#region Properties

	/// <summary>
	/// Model dữ liệu nâng cấp nội tại.
	/// </summary>
	public ImprovePassive ImprovePassive => base.BuildingUpgradeEffect as ImprovePassive;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo controller nâng cấp nội tại.
	/// </summary>
	public ImprovePassiveController(ImprovePassiveDefinition definition, TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade buildingUpgrade)
	{
		base.BuildingUpgradeEffect = new ImprovePassive(definition, this, buildingUpgrade);
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Tìm nội tại tương ứng theo PassiveId trong công trình và cải thiện hiệu ứng của nội tại đó.
	/// </summary>
	public override void TriggerEffect(bool onLoad = false)
	{
		for (int num = base.BuildingUpgradeEffect.BuildingUpgrade.Building.PassivesModule.BuildingPassives.Count - 1; num >= 0; num--)
		{
			if (base.BuildingUpgradeEffect.BuildingUpgrade.Building.PassivesModule.BuildingPassives[num].BuildingPassiveDefinition.Id == ImprovePassive.ImprovePassiveDefinition.PassiveId)
			{
				base.BuildingUpgradeEffect.BuildingUpgrade.Building.PassivesModule.BuildingPassives[num].BuildingPassiveController.ImproveEffects(ImprovePassive.ImprovePassiveDefinition.Value.EvalToInt());
			}
		}
	}

	#endregion
}

