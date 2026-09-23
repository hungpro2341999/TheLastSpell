using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Controller.Building.BuildingAction;
using TheLastStand.Controller.Building.BuildingGaugeEffect;
using TheLastStand.Definition.Building.BuildingGaugeEffect;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Building;
using UnityEngine;

namespace TheLastStand.Controller.Building.Module;

public class ProductionModuleController : BuildingModuleController
{
	#region Properties
	/// <summary>
	/// Model sản xuất (ProductionModule) của công trình.
	/// </summary>
	public ProductionModule ProductionModule { get; }
	#endregion

	#region Initialization & Factory
	/// <summary>
	/// Khởi tạo Controller sản xuất tài nguyên/vật phẩm của công trình.
	/// </summary>
	public ProductionModuleController(BuildingController buildingControllerParent, ProductionModuleDefinition productionModuleDefinition)
		: base(buildingControllerParent, productionModuleDefinition)
	{
		ProductionModule = base.BuildingModule as ProductionModule;
	}

	/// <summary>
	/// Khởi tạo hiệu ứng thanh điểm sản xuất (Gauge Effect) dựa trên định nghĩa (CreateItem, GainGold, GainMaterials, OpenMagicSeal, GlobalUpgradeStat).
	/// </summary>
	public void CreateGaugeEffect()
	{
		if (ProductionModule.ProductionModuleDefinition?.BuildingGaugeEffectDefinition != null)
		{
			BuildingGaugeEffectController buildingGaugeEffectController = null;
			switch (ProductionModule.ProductionModuleDefinition.BuildingGaugeEffectDefinition.Id)
			{
			case "CreateItem":
				buildingGaugeEffectController = new CreateItemGaugeEffectController(ProductionModule, ProductionModule.ProductionModuleDefinition.BuildingGaugeEffectDefinition as CreateItemGaugeEffectDefinition);
				break;
			case "GainGold":
				buildingGaugeEffectController = new GainGoldController(ProductionModule, ProductionModule.ProductionModuleDefinition.BuildingGaugeEffectDefinition as GainGoldDefinition);
				break;
			case "GainMaterials":
				buildingGaugeEffectController = new GainMaterialsController(ProductionModule, ProductionModule.ProductionModuleDefinition.BuildingGaugeEffectDefinition as GainMaterialsDefinition);
				break;
			case "OpenMagicSeal":
				buildingGaugeEffectController = new OpenMagicSealController(ProductionModule, ProductionModule.ProductionModuleDefinition.BuildingGaugeEffectDefinition as OpenMagicSealDefinition);
				break;
			case "GlobalUpgradeStat":
				buildingGaugeEffectController = new UpgradeStatGaugeEffectController(ProductionModule, ProductionModule.ProductionModuleDefinition.BuildingGaugeEffectDefinition as UpgradeStatGaugeEffectDefinition);
				break;
			}
			ProductionModule.BuildingGaugeEffect = buildingGaugeEffectController.BuildingGaugeEffect;
		}
	}

	/// <summary>
	/// Khởi tạo các hành động sản xuất (BuildingActions) từ định nghĩa ProductionModuleDefinition.
	/// </summary>
	public void CreateActions()
	{
		if (ProductionModule.ProductionModuleDefinition?.BuildingActionDefinitions != null)
		{
			ProductionModule.BuildingActions = new List<TheLastStand.Model.Building.BuildingAction.BuildingAction>();
			int i = 0;
			for (int count = ProductionModule.ProductionModuleDefinition.BuildingActionDefinitions.Count; i < count; i++)
			{
				ProductionModule.BuildingActions.Add(new BuildingActionController(ProductionModule.ProductionModuleDefinition.BuildingActionDefinitions[i], ProductionModule).BuildingAction);
			}
		}
	}

	/// <summary>
	/// Khởi tạo Model ProductionModule tương ứng.
	/// </summary>
	protected override BuildingModule CreateModel(TheLastStand.Model.Building.Building building, BuildingModuleDefinition buildingModuleDefinition)
	{
		return new ProductionModule(building, buildingModuleDefinition as ProductionModuleDefinition, this);
	}
	#endregion

	#region Gauge & Production Units
	/// <summary>
	/// Thêm điểm sản xuất vào thanh tiến trình sản xuất của công trình.
	/// </summary>
	public void AddProductionUnits(int units, bool useRandomDelay = false)
	{
		if (ProductionModule.BuildingGaugeEffect != null)
		{
			SetProductionUnits(ProductionModule.BuildingGaugeEffect.Units + units, tween: true, useRandomDelay);
		}
	}

	/// <summary>
	/// Đặt giá trị điểm sản xuất và kích hoạt hiệu ứng hình ảnh (Tween/Animation),
	/// cũng như kích hoạt TriggerGaugeEffectDelayed nếu điểm vượt ngưỡng.
	/// </summary>
	public void SetProductionUnits(int units, bool tween = true, bool useRandomDelay = false)
	{
		int units2 = ProductionModule.BuildingGaugeEffect.Units;
		ProductionModule.BuildingGaugeEffect.Units = units;
		TPSingleton<BuildingManager>.Instance.WaitBuildingGauges.Add(ProductionModule.BuildingParent);
		if (ProductionModule.BuildingParent is MagicCircle magicCircle)
		{
			int num = 0;
			num += ProductionModule.BuildingGaugeEffect.Units - units2;
			magicCircle.MagicCircleView.MagicCircleHUD.ProductionPanelMagicCircle.AnimateUnitsIncrement(tween, num);
		}
		else
		{
			Vector2 randomProductionTriggerDelay = TPSingleton<BuildingManager>.Instance.RandomProductionTriggerDelay;
			base.BuildingControllerParent.BuildingView.StartCoroutine(TriggerGaugeEffectDelayed(useRandomDelay ? UnityEngine.Random.Range(randomProductionTriggerDelay.x, randomProductionTriggerDelay.y) : 0f));
		}
	}

	/// <summary>
	/// Kích hoạt thưởng sản xuất tức thì khi vừa xây dựng xong công trình nếu có bonus.
	/// </summary>
	public void OnConstruction()
	{
		if (TPSingleton<ConstructionManager>.Instance.HasInstantProductionBonusLeft(base.BuildingControllerParent.Building) && ProductionModule.BuildingGaugeEffect != null && ProductionModule.BuildingGaugeEffect.BuildingGaugeEffectDefinition.TriggeredOnConstruction)
		{
			SetProductionUnits(ProductionModule.BuildingGaugeEffect.BuildingGaugeEffectDefinition.FirstGaugeUnits);
			TPSingleton<ConstructionManager>.Instance.DecrementInstantProductionBonus(base.BuildingControllerParent.Building);
		}
	}

	/// <summary>
	/// Coroutine kiểm tra và xả hiệu ứng khi điểm sản xuất đạt hoặc vượt ngưỡng (UnitsThreshold).
	/// </summary>
	private IEnumerator TriggerGaugeEffectDelayed(float delay)
	{
		yield return new WaitForSeconds(delay);
		while (ProductionModule.BuildingGaugeEffect.Units >= ProductionModule.BuildingGaugeEffect.UnitsThreshold)
		{
			ProductionModule.BuildingGaugeEffect.Units -= ProductionModule.BuildingGaugeEffect.UnitsThreshold;
			EffectManager.Register(ProductionModule.BuildingGaugeEffect.BuildingGaugeEffectController.TriggerEffect());
		}
		EffectManager.DisplayEffects();
		TPSingleton<BuildingManager>.Instance.WaitBuildingGauges.Remove(ProductionModule.BuildingParent);
	}
	#endregion

	#region Actions Management
	/// <summary>
	/// Làm mới số lượt sử dụng hành động sản xuất mỗi khi sang lượt Production ban ngày.
	/// </summary>
	public void RefreshActionsUsesPerTurn()
	{
		if (ProductionModule.BuildingActions != null)
		{
			int i = 0;
			for (int count = ProductionModule.BuildingActions.Count; i < count; i++)
			{
				ProductionModule.BuildingActions[i].UsesPerTurnRemaining = ProductionModule.BuildingActions[i].BuildingActionDefinition.UsesPerTurnCount;
			}
		}
	}
	#endregion

	#region Turn Management
	/// <summary>
	/// Xử lý bắt đầu lượt: Hiển thị bảng sản xuất MagicCircle và hồi phục số lượt tương tác sản xuất.
	/// </summary>
	public void StartTurn()
	{
		(ProductionModule.BuildingParent as MagicCircle)?.MagicCircleView.MagicCircleHUD?.ProductionPanelMagicCircle?.DisplayIfNeeded();
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day && TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production)
		{
			RefreshActionsUsesPerTurn();
		}
	}
	#endregion

	#region Serialization & Deserialization
	/// <summary>
	/// Giải mã và khôi phục số lượt hành động đã sử dụng từ dữ liệu lưu trữ (Save Game).
	/// </summary>
	public void DeserializeUsedActions(List<SerializedBuildingAction> usedActionsElement)
	{
		if (usedActionsElement == null || ProductionModule.BuildingActions == null)
		{
			return;
		}
		foreach (SerializedBuildingAction serializedAction in usedActionsElement)
		{
			TheLastStand.Model.Building.BuildingAction.BuildingAction buildingAction;
			if ((buildingAction = ProductionModule.BuildingActions.Find((TheLastStand.Model.Building.BuildingAction.BuildingAction o) => o.BuildingActionDefinition.Id == serializedAction.Id)) != null)
			{
				buildingAction.UsesPerTurnRemaining -= serializedAction.TimesUsed;
			}
		}
	}

	/// <summary>
	/// Giải mã và khôi phục hiệu ứng thanh điểm sản xuất từ dữ liệu lưu trữ (Save Game).
	/// </summary>
	public void DeserializeGaugeEffect(SerializedGaugeEffect gaugeEffectElement)
	{
		if (gaugeEffectElement == null || ProductionModule.ProductionModuleDefinition?.BuildingGaugeEffectDefinition == null)
		{
			return;
		}
		string id = gaugeEffectElement.Id;
		if (id != null)
		{
			BuildingGaugeEffectController buildingGaugeEffectController = null;
			switch (id)
			{
			case "CreateItem":
				buildingGaugeEffectController = new CreateItemGaugeEffectController(gaugeEffectElement, ProductionModule, ProductionModule.ProductionModuleDefinition.BuildingGaugeEffectDefinition as CreateItemGaugeEffectDefinition);
				break;
			case "GainGold":
				buildingGaugeEffectController = new GainGoldController(gaugeEffectElement, ProductionModule, ProductionModule.ProductionModuleDefinition.BuildingGaugeEffectDefinition as GainGoldDefinition);
				break;
			case "GainMaterials":
				buildingGaugeEffectController = new GainMaterialsController(gaugeEffectElement, ProductionModule, ProductionModule.ProductionModuleDefinition.BuildingGaugeEffectDefinition as GainMaterialsDefinition);
				break;
			case "OpenMagicSeal":
				buildingGaugeEffectController = new OpenMagicSealController(gaugeEffectElement, ProductionModule, ProductionModule.ProductionModuleDefinition.BuildingGaugeEffectDefinition as OpenMagicSealDefinition);
				break;
			case "GlobalUpgradeStat":
				buildingGaugeEffectController = new UpgradeStatGaugeEffectController(gaugeEffectElement, ProductionModule, ProductionModule.ProductionModuleDefinition.BuildingGaugeEffectDefinition as UpgradeStatGaugeEffectDefinition);
				break;
			}
			ProductionModule.BuildingGaugeEffect = buildingGaugeEffectController.BuildingGaugeEffect;
		}
		base.BuildingControllerParent.BuildingView.Init();
		base.BuildingControllerParent.BuildingView.BuildingHUD.Building = base.BuildingControllerParent.Building;
		SetProductionUnits(ProductionModule.BuildingGaugeEffect.Units, tween: false);
	}

	/// <summary>
	/// Mã hóa thông tin các hành động sản xuất đã sử dụng để lưu lại.
	/// </summary>
	public IEnumerable<SerializedBuildingAction> SerializeUsedActions()
	{
		return ProductionModule.BuildingActions?.Select((TheLastStand.Model.Building.BuildingAction.BuildingAction action) => new SerializedBuildingAction
		{
			Id = action.BuildingActionDefinition.Id,
			TimesUsed = action.BuildingActionDefinition.UsesPerTurnCount - action.UsesPerTurnRemaining
		});
	}
	#endregion
}
