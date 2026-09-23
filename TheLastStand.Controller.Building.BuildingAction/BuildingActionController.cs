using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Controller.CastFx;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Meta;
using TheLastStand.Model;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.CastFx;
using TheLastStand.Model.Meta;
using TheLastStand.Model.TileMap;
using UnityEngine;

namespace TheLastStand.Controller.Building.BuildingAction;

/// <summary>
/// Controller trung tâm điều phối Hành Động Công Trình (Building Action) trong The Last Spell.
/// Mỗi hành động công trình (ví dụ: Tạo vàng, Rèn đồ, Hồi máu Hero, Đẩy lùi sương mù, Khai quật tàn tích)
/// được quản lý bởi controller này, bao gồm:
/// - Kiểm tra điều kiện kích hoạt (số lượng thợ/Workers, số lần dùng còn lại trong lượt, phase ngày/đêm hợp lệ).
/// - Kiểm tra tính hợp lệ của ô Tile mục tiêu trên bản đồ.
/// - Trừ chi phí nhân công và kích hoạt tất cả các hiệu ứng con (BuildingActionEffect).
/// - Khởi tạo các Effect Controller cụ thể thông qua cơ chế Factory (GenerateActionEffects).
/// - Kích hoạt hoạt ảnh hiệu ứng thi triển (CastFx).
/// </summary>
public class BuildingActionController
{
	#region Properties & Model

	/// <summary>
	/// Model lưu trữ trạng thái và dữ liệu của hành động công trình.
	/// </summary>
	public TheLastStand.Model.Building.BuildingAction.BuildingAction BuildingAction { get; }

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo Controller từ file XML lưu trữ dữ liệu hoặc savse game.
	/// </summary>
	/// <param name="container">Phần tử XML chứa thông tin thuộc tính của hành động (ID, v.v.).</param>
	/// <param name="productionBuilding">Module sản xuất của công trình sở hữu hành động này.</param>
	public BuildingActionController(XContainer container, ProductionModule productionBuilding)
	{
		XElement xElement = container as XElement;
		BuildingActionDefinition buildingActionDefinition = BuildingDatabase.BuildingActionDefinitions[xElement.Attribute("Id").Value];
		BuildingAction = new TheLastStand.Model.Building.BuildingAction.BuildingAction(buildingActionDefinition, this, productionBuilding);
		GenerateActionEffects(buildingActionDefinition, productionBuilding);
		
		// Khởi tạo hiệu ứng diễn hoạt đồ họa (Cast FX) nếu hành động có định nghĩa hiệu ứng thị giác
		if (BuildingAction.BuildingActionDefinition.CastFxDefinition != null)
		{
			BuildingAction.CastFx = new CastFxController(BuildingAction.BuildingActionDefinition.CastFxDefinition).CastFx;
			BuildingAction.CastFx.SourceTile = BuildingAction.ProductionBuilding.BuildingParent.OriginTile;
			BuildingAction.CastFx.CastFXInterpreterContext = new CastFXInterpreterContext(BuildingAction.CastFx);
		}
	}

	/// <summary>
	/// Khởi tạo Controller trực tiếp từ đối tượng định nghĩa BuildingActionDefinition.
	/// </summary>
	/// <param name="actionDefinition">Định nghĩa dữ liệu của hành động.</param>
	/// <param name="productionBuilding">Module sản xuất của công trình liên kết.</param>
	public BuildingActionController(BuildingActionDefinition actionDefinition, ProductionModule productionBuilding)
	{
		BuildingAction = new TheLastStand.Model.Building.BuildingAction.BuildingAction(actionDefinition, this, productionBuilding);
		GenerateActionEffects(actionDefinition, productionBuilding);
		
		// Khởi tạo hiệu ứng diễn hoạt đồ họa (Cast FX)
		if (BuildingAction.BuildingActionDefinition.CastFxDefinition != null)
		{
			BuildingAction.CastFx = new CastFxController(BuildingAction.BuildingActionDefinition.CastFxDefinition).CastFx;
			BuildingAction.CastFx.SourceTile = BuildingAction.ProductionBuilding.BuildingParent.OriginTile;
			BuildingAction.CastFx.CastFXInterpreterContext = new CastFXInterpreterContext(BuildingAction.CastFx);
		}
	}

	#endregion

	#region Verification & State Check

	/// <summary>
	/// Kiểm tra xem hành động công trình có thể thực thi ở thời điểm hiện tại hay không:
	/// 1. Nếu hành động có hiệu ứng đẩy lùi sương mù, kiểm tra xem sương mù hiện tại có thể giảm mật độ được nữa không.
	/// 2. Người chơi có đủ số lượng công nhân (Workers) yêu cầu hay không.
	/// 3. Số lượt sử dụng hành động trong lượt này còn lại (> 0) hoặc không giới hạn (-1).
	/// 4. Trạng thái giai đoạn hiện tại (Phase: Day/Production/Deployment/Night) có cho phép dùng hành động hay không.
	/// </summary>
	/// <returns>True nếu thỏa mãn tất cả điều kiện để nhấn nút thực thi hành động.</returns>
	public bool CanExecuteAction()
	{
		// Kiểm tra điều kiện giới hạn sương mù
		if (BuildingAction.BuildingActionDefinition.ContainsRepelFogEffect && !TPSingleton<FogManager>.Instance.Fog.CanDecreaseFogDensity)
		{
			return false;
		}
		
		// Kiểm tra lượng thợ cần thiết và số lần sử dụng còn lại trong lượt
		if (TPSingleton<ResourceManager>.Instance.Workers >= ResourceManager.GetModifiedWorkersCost(BuildingAction.BuildingActionDefinition) && (BuildingAction.BuildingActionDefinition.UsesPerTurnCount == -1 || BuildingAction.UsesPerTurnRemaining > 0))
		{
			// Kiểm tra giai đoạn game hiện tại có được phép dùng không (ngoại trừ khi bật chế độ Debug bỏ qua phase)
			if (!BuildingManager.DebugUseForceBuildingActionsAllPhases)
			{
				return GetActionCurrentState() == PhaseStates.E_PhaseState.Available;
			}
			return true;
		}
		return false;
	}

	/// <summary>
	/// Kiểm tra xem trên toàn bộ bản đồ có tồn tại bất kỳ ô Tile nào hợp lệ để thi triển hành động này hay không.
	/// </summary>
	/// <returns>True nếu có ít nhất một ô hợp lệ.</returns>
	public bool CanExecuteActionOnAnyTile()
	{
		for (int num = TPSingleton<TileMapManager>.Instance.TileMap.Tiles.Length - 1; num >= 0; num--)
		{
			if (CanExecuteActionOnTile(TPSingleton<TileMapManager>.Instance.TileMap.Tiles[num]))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Kiểm tra xem ô Tile được chọn có thỏa mãn tất cả các hiệu ứng con trong hành động này hay không.
	/// </summary>
	/// <param name="tile">Ô Tile mục tiêu cần kiểm tra.</param>
	/// <returns>True nếu tất cả hiệu ứng con đều chấp nhận ô Tile này.</returns>
	public bool CanExecuteActionOnTile(Tile tile)
	{
		int i = 0;
		for (int count = BuildingAction.BuildingActionEffects.Count; i < count; i++)
		{
			if (!BuildingAction.BuildingActionEffects[i].BuildingActionEffectController.CanExecuteActionEffectOnTile(tile))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Xác định trạng thái khả dụng của hành động dựa trên chu kỳ ngày/đêm và pha chơi hiện tại:
	/// - Đêm (Night): Trả về trạng thái cấu hình trong NightState.
	/// - Ngày (Day) - Pha sản xuất (Production): Trả về ProductionState.
	/// - Ngày (Day) - Pha triển khai quân (Deployment): Trả về DeploymentState.
	/// </summary>
	/// <returns>Trạng thái pha của hành động (Available, Locked, v.v.).</returns>
	public PhaseStates.E_PhaseState GetActionCurrentState()
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night)
		{
			return BuildingAction.BuildingActionDefinition.PhaseStates.NightState;
		}
		if (TPSingleton<GameManager>.Instance.Game.DayTurn != Game.E_DayTurn.Deployment)
		{
			return BuildingAction.BuildingActionDefinition.PhaseStates.ProductionState;
		}
		return BuildingAction.BuildingActionDefinition.PhaseStates.DeploymentState;
	}

	#endregion

	#region Action Execution & Target Management

	/// <summary>
	/// Thiết lập ô Tile mục tiêu được chọn cho hành động chính và lan truyền xuống tất cả các hiệu ứng con.
	/// </summary>
	/// <param name="tile">Ô Tile được click chọn làm đích đến.</param>
	public void SetTarget(Tile tile)
	{
		BuildingAction.Target = tile;
		for (int num = BuildingAction.BuildingActionEffects.Count - 1; num >= 0; num--)
		{
			BuildingAction.BuildingActionEffects[num].Target = tile;
		}
	}

	/// <summary>
	/// Thực thi toàn bộ các hiệu ứng của hành động công trình:
	/// 1. Trừ số lượt sử dụng trong lượt (nếu có giới hạn lượt).
	/// 2. Tiêu hao số lượng công nhân (Workers) cần thiết và cập nhật tiến trình Meta/Glyph.
	/// 3. Duyệt và gọi ExecuteActionEffect() trên từng Controller hiệu ứng con.
	/// 4. Phát hiệu ứng thị giác (CastFx) lên các ô Tile chịu ảnh hưởng.
	/// </summary>
	public void ExecuteActionEffects()
	{
		// Giảm số lượt dùng còn lại trong lượt
		if (BuildingAction.BuildingActionDefinition.UsesPerTurnCount != -1)
		{
			BuildingAction.UsesPerTurnRemaining--;
		}
		
		// Tiêu hao công nhân (có tính đến bổ trợ giảm chi phí từ Glyph)
		int modifiedWorkersCost = ResourceManager.GetModifiedWorkersCost(BuildingAction.BuildingActionDefinition, updateGlyphLimits: true);
		TPSingleton<ResourceManager>.Instance.UseWorkers(modifiedWorkersCost);
		
		// Nếu là hành động dọn dẹp xác/tàn tích (Scavenge), ghi nhận số công nhân đã dùng vào thống kê Meta
		if (BuildingAction.BuildingActionEffects.Any((BuildingActionEffect o) => o is ScavengeBuildingActionEffect))
		{
			TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.ScavengeWorkers, modifiedWorkersCost);
			TPSingleton<ResourceManager>.Instance.ScavengeWorkersThisTurn += modifiedWorkersCost;
			int scavengeWorkersThisTurn = TPSingleton<ResourceManager>.Instance.ScavengeWorkersThisTurn;
			if (scavengeWorkersThisTurn > 0)
			{
				TPSingleton<MetaConditionManager>.Instance.RefreshMaxDoubleValue(MetaConditionSpecificContext.E_ValueCategory.MaxScavengeWorkersSingleProd, scavengeWorkersThisTurn);
			}
		}
		
		// Kích hoạt logic thực thi của toàn bộ danh sách các Effect con
		int num = 0;
		for (int count = BuildingAction.BuildingActionEffects.Count; num < count; num++)
		{
			BuildingAction.BuildingActionEffects[num].BuildingActionEffectController.ExecuteActionEffect();
		}
		
		// Diễn hoạt đồ họa FX (âm thanh, tia sáng, tia ma thuật)
		if (BuildingAction.BuildingActionDefinition.CastFxDefinition != null)
		{
			BuildingAction.CastFx.AffectedTiles.Clear();
			BuildingAction.CastFx.TargetTile = BuildingAction.Target;
			BuildingAction.CastFx.AffectedTiles.Add(new List<Tile>(1) { BuildingAction.Target });
			BuildingAction.CastFx.CastFxController.PlayCastFxs(TileObjectSelectionManager.E_Orientation.NONE, default(Vector2), BuildingAction.ProductionBuilding.BuildingParent);
		}
	}

	#endregion

	#region Action Effect Generation (Factory Pattern)

	/// <summary>
	/// Factory Method: Khởi tạo danh sách các Controller hiệu ứng con tương ứng dựa trên kiểu dữ liệu
	/// của từng BuildingActionEffectDefinition trong file cấu hình, đồng thời xác định xem hành động
	/// có thể thực thi ngay lập tức (IsExecutionInstant) hay yêu cầu tương tác chọn mục tiêu.
	/// </summary>
	/// <param name="actionDefinition">Định nghĩa dữ liệu của hành động.</param>
	/// <param name="productionBuilding">Module sản xuất của công trình.</param>
	private void GenerateActionEffects(BuildingActionDefinition actionDefinition, ProductionModule productionBuilding)
	{
		if (actionDefinition.BuildingActionEffectDefinition != null)
		{
			BuildingAction.BuildingActionEffects = new List<BuildingActionEffect>();
			int i = 0;
			for (int count = actionDefinition.BuildingActionEffectDefinition.Count; i < count; i++)
			{
				BuildingActionEffectDefinition buildingActionEffectDefinition = actionDefinition.BuildingActionEffectDefinition[i];
				
				// Phân giải và khởi tạo đúng Controller tương ứng với từng loại Effect Definition
				BuildingActionEffect buildingActionEffect = ((buildingActionEffectDefinition is FillGaugeBuildingActionEffectDefinition definition) ? new FillGaugeBuildingActionEffectController(definition, productionBuilding).FillGaugeBuildingActionEffect : ((buildingActionEffectDefinition is HealBuildingActionEffectDefinition definition2) ? new HealBuildingActionEffectController(definition2, productionBuilding).HealBuildingActionEffect : ((buildingActionEffectDefinition is HealManaBuildingActionEffectDefinition definition3) ? new HealManaBuildingActionEffectController(definition3, productionBuilding).HealManaBuildingActionEffect : ((buildingActionEffectDefinition is ScavengeBuildingActionEffectDefinition definition4) ? new ScavengeBuildingActionEffectController(definition4, productionBuilding).ScavengeBuildingActionEffect : ((buildingActionEffectDefinition is GainGoldBuildingActionEffectDefinition definition5) ? new GainGoldBuildingActionEffectController(definition5, productionBuilding).GainGoldBuildingActionEffect : ((buildingActionEffectDefinition is GainMaterialsBuildingActionEffectDefinition definition6) ? new GainMaterialsBuildingActionEffectController(definition6, productionBuilding).GainMaterialsBuildingActionEffect : ((buildingActionEffectDefinition is RepelFogBuildingActionEffectDefinition definition7) ? new RepelFogBuildingActionEffectController(definition7, productionBuilding).RepelFogBuildingActionEffect : ((buildingActionEffectDefinition is RevealDangerIndicatorsBuildingActionEffectDefinition definition8) ? new RevealDangerIndicatorsBuildingActionEffectController(definition8, productionBuilding).RevealWaveEnemiesRatioBuildingActionEffect : ((buildingActionEffectDefinition is RerollWaveBuildingActionEffectDefinition definition9) ? ((BuildingActionEffect)new RerollWaveBuildingActionEffectController(definition9, productionBuilding).RerollWaveBuildingActionEffect) : ((BuildingActionEffect)((!(buildingActionEffectDefinition is UpgradeStatBuildingActionEffectDefinition definition10)) ? null : new UpgradeStatBuildingActionEffectController(definition10, productionBuilding).UpgradeStatBuildingActionEffect)))))))))));
				BuildingActionEffect item = buildingActionEffect;
				TheLastStand.Model.Building.BuildingAction.BuildingAction buildingAction = BuildingAction;
				buildingActionEffectDefinition = actionDefinition.BuildingActionEffectDefinition[i];
				
				// Kiểm tra hành động có chạy ngay lập tức hay không (ví dụ: các hiệu ứng áp dụng lên toàn bộ (All) thay vì cần click chọn 1 Hero)
				bool isExecutionInstant = ((buildingActionEffectDefinition is HealBuildingActionEffectDefinition healBuildingActionEffectDefinition) ? (healBuildingActionEffectDefinition.BuildingActionTargeting == BuildingActionEffectDefinition.E_BuildingActionTargeting.All) : ((buildingActionEffectDefinition is HealManaBuildingActionEffectDefinition healManaBuildingActionEffectDefinition) ? (healManaBuildingActionEffectDefinition.BuildingActionTargeting == BuildingActionEffectDefinition.E_BuildingActionTargeting.All) : (!(buildingActionEffectDefinition is UpgradeStatBuildingActionEffectDefinition upgradeStatBuildingActionEffectDefinition) || upgradeStatBuildingActionEffectDefinition.BuildingActionTargeting == BuildingActionEffectDefinition.E_BuildingActionTargeting.All)));
				buildingAction.IsExecutionInstant = isExecutionInstant;
				BuildingAction.BuildingActionEffects.Add(item);
			}
		}
	}

	#endregion
}

