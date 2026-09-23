using System.Collections.Generic;
using TPLib;
using TheLastStand.Controller.Building.BuildingPassive;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;
using TheLastStand.Serialization.Building;

namespace TheLastStand.Controller.Building.Module;

public class PassivesModuleController : BuildingModuleController
{
	#region Properties
	/// <summary>
	/// Model quản lý nội tại/bị động (PassivesModule) của công trình.
	/// </summary>
	public PassivesModule PassivesModule { get; }
	#endregion

	#region Initialization & Factory
	/// <summary>
	/// Khởi tạo Controller xử lý các hiệu ứng nội tại của công trình.
	/// </summary>
	public PassivesModuleController(BuildingController buildingControllerParent, PassivesModuleDefinition passivesModuleDefinition)
		: base(buildingControllerParent, passivesModuleDefinition)
	{
		PassivesModule = base.BuildingModule as PassivesModule;
	}

	/// <summary>
	/// Khởi tạo danh sách các BuildingPassive từ định nghĩa PassivesModuleDefinition.
	/// </summary>
	public void CreatePassives()
	{
		PassivesModule.BuildingPassives = new List<TheLastStand.Model.Building.BuildingPassive.BuildingPassive>();
		for (int i = 0; i < PassivesModule.PassivesModuleDefinition.BuildingPassiveDefinitions.Count; i++)
		{
			PassivesModule.BuildingPassives.Add(new BuildingPassiveController(PassivesModule, PassivesModule.PassivesModuleDefinition.BuildingPassiveDefinitions[i]).BuildingPassive);
		}
	}

	/// <summary>
	/// Khởi tạo Model PassivesModule tương ứng.
	/// </summary>
	protected override BuildingModule CreateModel(TheLastStand.Model.Building.Building building, BuildingModuleDefinition buildingModuleDefinition)
	{
		return new PassivesModule(building, buildingModuleDefinition as PassivesModuleDefinition, this);
	}
	#endregion

	#region Passive Trigger & Life Cycle
	/// <summary>
	/// Kích hoạt hiệu ứng bị động của công trình tại một thời điểm nhất định (E_EffectTime).
	/// </summary>
	public void ApplyPassiveEffect(E_EffectTime effectTime, bool force = false, bool onLoad = false)
	{
		if (PassivesModule.BuildingPassives != null && !(ApplicationManager.Application.State.GetName() == "LevelEditor"))
		{
			for (int i = 0; i < PassivesModule.BuildingPassives.Count; i++)
			{
				PassivesModule.BuildingPassives[i].BuildingPassiveController.Trigger(effectTime, force, onLoad);
			}
		}
	}

	/// <summary>
	/// Kích hoạt các hiệu ứng bị động khi bắt đầu lượt (Day Turn hoặc Night Turn).
	/// </summary>
	public void StartTurn()
	{
		switch (TPSingleton<GameManager>.Instance.Game.Cycle)
		{
		case Game.E_Cycle.Day:
			if (TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production)
			{
				ApplyPassiveEffect(E_EffectTime.OnStartProductionTurn);
			}
			break;
		case Game.E_Cycle.Night:
			switch (TPSingleton<GameManager>.Instance.Game.NightTurn)
			{
			case Game.E_NightTurn.EnemyUnits:
				ApplyPassiveEffect(E_EffectTime.OnStartNightTurnEnemy);
				break;
			case Game.E_NightTurn.PlayableUnits:
				ApplyPassiveEffect(E_EffectTime.OnStartNightTurnPlayable);
				break;
			}
			break;
		}
	}

	/// <summary>
	/// Kích hoạt các hiệu ứng bị động khi kết thúc lượt (Production, Night End, Enemy/Player Turn End).
	/// </summary>
	public void EndTurn()
	{
		switch (TPSingleton<GameManager>.Instance.Game.Cycle)
		{
		case Game.E_Cycle.Day:
			if (TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production)
			{
				ApplyPassiveEffect(E_EffectTime.OnEndProductionTurn);
			}
			break;
		case Game.E_Cycle.Night:
			if (SpawnWaveManager.CurrentSpawnWave == null)
			{
				ApplyPassiveEffect(E_EffectTime.OnNightEnd);
			}
			switch (TPSingleton<GameManager>.Instance.Game.NightTurn)
			{
			case Game.E_NightTurn.EnemyUnits:
				ApplyPassiveEffect(E_EffectTime.OnEndNightTurnEnemy);
				break;
			case Game.E_NightTurn.PlayableUnits:
				ApplyPassiveEffect(E_EffectTime.OnEndNightTurnPlayable);
				break;
			}
			break;
		}
	}

	/// <summary>
	/// Xử lý khi công trình bị hủy: kích hoạt hiệu ứng OnDeath và hủy bỏ các hiệu ứng thụ động vĩnh viễn đã áp dụng.
	/// </summary>
	public void OnDeath(bool triggerOnDeathEvent = true)
	{
		if (!(ApplicationManager.Application.State.GetName() == "LevelEditor"))
		{
			if (triggerOnDeathEvent)
			{
				ApplyPassiveEffect(E_EffectTime.OnDeath);
			}
			UndoPermanentPassiveEffects();
		}
	}

	/// <summary>
	/// Hủy bỏ (hoàn tác) các hiệu ứng bị động vĩnh viễn đã được gán bởi công trình này.
	/// </summary>
	public void UndoPermanentPassiveEffects()
	{
		for (int i = 0; i < PassivesModule.BuildingPassives.Count; i++)
		{
			PassivesModule.BuildingPassives[i].BuildingPassiveController.UndoPermanentPassiveEffects();
		}
	}
	#endregion

	#region Serialization & Deserialization
	/// <summary>
	/// Giải mã và phục hồi trạng thái các bị động của công trình từ dữ liệu lưu trữ (Save Data).
	/// </summary>
	public void DeserializePassive(List<SerializedBuildingPassive> passiveElements, int saveVersion)
	{
		PassivesModule.BuildingPassives = new List<TheLastStand.Model.Building.BuildingPassive.BuildingPassive>();
		foreach (SerializedBuildingPassive passiveElement in passiveElements)
		{
			PassivesModule.BuildingPassives.Add(new BuildingPassiveController(passiveElement, PassivesModule, saveVersion).BuildingPassive);
		}
	}
	#endregion
}
