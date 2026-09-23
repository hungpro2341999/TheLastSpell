using TheLastStand.Definition.Building.Module;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using UnityEngine;

namespace TheLastStand.Controller.Building.Module;

public class BrazierModuleController : BuildingModuleController
{
	#region Properties
	/// <summary>
	/// Model dữ liệu Đốt ngọn lửa / Điểm hỏa đài (BrazierModule) của công trình.
	/// </summary>
	public BrazierModule BrazierModule { get; }
	#endregion

	#region Initialization & Factory
	/// <summary>
	/// Khởi tạo Controller quản lý điểm hỏa đài.
	/// </summary>
	public BrazierModuleController(BuildingController buildingControllerParent, BrazierModuleDefinition brazierModuleDefinition)
		: base(buildingControllerParent, brazierModuleDefinition)
	{
		BrazierModule = base.BuildingModule as BrazierModule;
	}

	/// <summary>
	/// Khởi tạo Model BrazierModule tương ứng.
	/// </summary>
	protected override BuildingModule CreateModel(TheLastStand.Model.Building.Building building, BuildingModuleDefinition buildingModuleDefinition)
	{
		return new BrazierModule(building, buildingModuleDefinition as BrazierModuleDefinition, this);
	}
	#endregion

	#region Brazier Logic
	/// <summary>
	/// Trừ điểm Brazier (đốt ngọn lửa/điểm hỏa đài) khi chịu sát thương.
	/// Nếu điểm về 0, chuẩn bị tử trận cho Boss hoặc kích hoạt hiệu ứng OnExtinguish (dập tắt hỏa đài).
	/// </summary>
	public int LoseBrazierPoints(int damage, bool triggerEvent = false)
	{
		if (BrazierModule.BrazierPoints <= 0)
		{
			return 0;
		}
		int num = Mathf.Min(damage, BrazierModule.BrazierPoints);
		BrazierModule.BrazierPoints -= num;
		if (BrazierModule.BuildingParent.IsBossPhaseActor && BrazierModule.BrazierPoints == 0)
		{
			BrazierModule.BuildingParent.PrepareBossActorDeath();
		}
		if (triggerEvent && BrazierModule.BrazierPoints == 0)
		{
			BrazierModule.IsExtinguishing = true;
			base.BuildingControllerParent.PassivesModuleController?.ApplyPassiveEffect(E_EffectTime.OnExtinguish);
		}
		return num;
	}
	#endregion
}
