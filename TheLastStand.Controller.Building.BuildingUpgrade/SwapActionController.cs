using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Building.BuildingAction;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.BuildingUpgrade;
using UnityEngine;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

/// <summary>
/// Controller xử lý hiệu ứng nâng cấp hoán đổi (thay thế) hành động của công trình (Swap Action) từ OldActionId sang NewActionId.
/// </summary>
public class SwapActionController : BuildingUpgradeEffectController
{
	#region Properties

	/// <summary>
	/// Model dữ liệu nâng cấp hoán đổi hành động.
	/// </summary>
	public SwapAction SwapAction => base.BuildingUpgradeEffect as SwapAction;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo controller nâng cấp hoán đổi hành động.
	/// </summary>
	public SwapActionController(SwapActionDefinition definition, TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade buildingUpgrade)
	{
		base.BuildingUpgradeEffect = new SwapAction(definition, this, buildingUpgrade);
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Tìm hành động cũ (OldActionId) trong ProductionModule của công trình và thay thế bằng hành động mới (NewActionId), duy trì số lượt dùng còn lại.
	/// </summary>
	public override void TriggerEffect(bool onLoad = false)
	{
		int num = 0;
		TheLastStand.Model.Building.BuildingAction.BuildingAction buildingAction = base.BuildingUpgradeEffect.BuildingUpgrade.Building.ProductionModule.BuildingActions.Find((TheLastStand.Model.Building.BuildingAction.BuildingAction buildingAction3) => buildingAction3.BuildingActionDefinition.Id == SwapAction.SwapActionDefinition.OldActionId);
		if (buildingAction != null)
		{
			num = base.BuildingUpgradeEffect.BuildingUpgrade.Building.ProductionModule.BuildingActions.IndexOf(buildingAction);
			base.BuildingUpgradeEffect.BuildingUpgrade.Building.ProductionModule.BuildingActions.RemoveAt(num);
			BuildingActionDefinition buildingActionDefinition = null;
			if (BuildingDatabase.BuildingActionDefinitions.TryGetValue(SwapAction.SwapActionDefinition.NewActionId, out var value))
			{
				buildingActionDefinition = value.Clone();
				TheLastStand.Model.Building.BuildingAction.BuildingAction buildingAction2 = new BuildingActionController(buildingActionDefinition, base.BuildingUpgradeEffect.BuildingUpgrade.Building.ProductionModule).BuildingAction;
				int num2 = buildingActionDefinition.UsesPerTurnCount - buildingAction.BuildingActionDefinition.UsesPerTurnCount;
				buildingAction2.UsesPerTurnRemaining = Mathf.Clamp(buildingAction.UsesPerTurnRemaining + num2, 0, buildingActionDefinition.UsesPerTurnCount);
				base.BuildingUpgradeEffect.BuildingUpgrade.Building.ProductionModule.BuildingActions.Insert(num, buildingAction2);
			}
			else
			{
				TPSingleton<BuildingManager>.Instance.LogError("BuildingActionDefinition " + SwapAction.SwapActionDefinition.NewActionId + " not found", CLogLevel.MAJOR);
			}
		}
		else
		{
			TPSingleton<BuildingManager>.Instance.LogError("SwapActionController was not able to find an existing building action with the Id " + SwapAction.SwapActionDefinition.OldActionId + " => Abort upgrade effect", CLogLevel.MAJOR);
		}
	}

	#endregion
}

