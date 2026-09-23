using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Meta;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Definition.Meta;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;
using UnityEngine;

namespace TheLastStand.Controller.Building.BuildingPassive;

/// <summary>
/// Bộ điều khiển hiệu ứng nội tại nạp thanh tích lũy hiệu ứng (Fill Effect Gauge).
/// Tính toán giá trị nạp dựa trên công thức cấu hình, điểm thưởng nâng cấp, 
/// và các hiệu ứng cộng thêm từ hệ thống nâng cấp vĩnh viễn (Meta Upgrades).
/// </summary>
public class FillEffectGaugeController : BuildingPassiveEffectController
{
	#region Properties

	/// <summary>
	/// Model lưu trữ dữ liệu của hiệu ứng nạp thanh gauge.
	/// </summary>
	public FillEffectGauge FillEffectGauge => base.BuildingPassiveEffect as FillEffectGauge;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo FillEffectGaugeController với module nội tại và định nghĩa cấu hình.
	/// </summary>
	/// <param name="buildingPassivesModule">Module nội tại của công trình.</param>
	/// <param name="fillEffectGaugeDefinition">Định nghĩa cấu hình của hiệu ứng nạp gauge.</param>
	public FillEffectGaugeController(PassivesModule buildingPassivesModule, FillEffectGaugeDefinition fillEffectGaugeDefinition)
	{
		base.BuildingPassiveEffect = new FillEffectGauge(buildingPassivesModule, fillEffectGaugeDefinition, this);
	}

	#endregion

	#region Passive Effect Execution & Upgrades

	/// <summary>
	/// Thực thi nạp điểm vào thanh tích lũy sản xuất của công trình cha.
	/// </summary>
	public override void Apply()
	{
		// Kiểm tra công trình có sở hữu module sản xuất với hiệu ứng thanh gauge hay không
		if (base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.ProductionModule?.BuildingGaugeEffect == null)
		{
			TPSingleton<BuildingManager>.Instance.LogError("The building " + base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.BuildingDefinition.Id + " has no gauge effect", CLogLevel.MAJOR);
			return;
		}

		// Tính toán tổng lượng điểm cần nạp
		int num = ComputePassiveValue();
		if (num > 0)
		{
			base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.BuildingController.ProductionModuleController.AddProductionUnits(num, useRandomDelay: true);
			TPSingleton<BuildingManager>.Instance.Log($"({base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.BuildingDefinition.Id}) Fill effect gauge (+{num}, total {base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.ProductionModule.BuildingGaugeEffect.Units})", CLogLevel.MAJOR);
		}
	}

	/// <summary>
	/// Tăng cường lượng nạp thanh gauge khi công trình được nâng cấp trong trận đấu.
	/// </summary>
	/// <param name="bonus">Lượng giá trị nạp cộng thêm.</param>
	public override void ImproveEffect(int bonus)
	{
		FillEffectGauge.UpgradedBonusValue += bonus;
	}

	/// <summary>
	/// Tính toán tổng giá trị điểm nạp vào gauge:
	/// Kết hợp giá trị biểu thức gốc (eval qua BuildingInterpreterContext) + Điểm thưởng nâng cấp + Phần trăm Meta Upgrade.
	/// </summary>
	/// <returns>Tổng số điểm tích lũy được cộng vào thanh gauge.</returns>
	public int ComputePassiveValue()
	{
		// Đánh giá giá trị biểu thức công thức ban đầu
		int num = FillEffectGauge.FillEffectGaugeDefinition.Value.EvalToInt(new BuildingInterpreterContext());
		int num2 = 0;
		num2 += FillEffectGauge.UpgradedBonusValue;

		// Áp dụng thưởng % sản xuất nội tại từ nâng cấp Meta (Meta Upgrades) nếu công trình không mở khóa mặc định
		if (!MetaUpgradesManager.IsThisBuildingUnlockedByDefault(base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.Id) && MetaUpgradeEffectsController.TryGetEffectsOfType<BuildingModifierMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
		{
			for (int num3 = effects.Length - 1; num3 >= 0; num3--)
			{
				if (effects[num3].BuildingId == base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.Id)
				{
					num2 += Mathf.RoundToInt((float)(num * effects[num3].PassiveProductionBonus) / 100f);
				}
			}
		}

		return num + num2;
	}

	#endregion
}
