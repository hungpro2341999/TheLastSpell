using System;
using TPLib;
using TheLastStand.Database;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Manager;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Building.BuildingPassive;

/// <summary>
/// Bộ điều khiển hiệu ứng nội tại cập nhật cấp độ Đống Xương (Update Bone Pile Level).
/// Dựa vào tiến trình số ngày trôi qua (DayNumber) và bảng tiến hóa đống xương theo thành phố (BonePilesEvolutionId)
/// để nâng cấp độ sản xuất / khai quật tài nguyên của đống xương.
/// </summary>
public class UpdateBonePileLevelController : BuildingPassiveEffectController
{
	#region Properties

	/// <summary>
	/// Model lưu trữ định nghĩa và dữ liệu cập nhật cấp độ đống xương.
	/// </summary>
	public UpdateBonePileLevel UpdateBonePileLevel => base.BuildingPassiveEffect as UpdateBonePileLevel;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo UpdateBonePileLevelController với module nội tại và định nghĩa cấu hình.
	/// </summary>
	/// <param name="buildingPassivesModule">Module nội tại của công trình đống xương.</param>
	/// <param name="updateBonePileLevelDefinition">Định nghĩa cấu hình cập nhật cấp độ.</param>
	public UpdateBonePileLevelController(PassivesModule buildingPassivesModule, UpdateBonePileLevelDefinition updateBonePileLevelDefinition)
	{
		base.BuildingPassiveEffect = new UpdateBonePileLevel(buildingPassivesModule, updateBonePileLevelDefinition, this);
	}

	#endregion

	#region Passive Effect Execution

	/// <summary>
	/// Thực thi kiểm tra và cập nhật cấp độ của đống xương theo ngày hiện tại trong game.
	/// </summary>
	public override void Apply()
	{
		int level = base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.ProductionModule.Level;

		// Tra cứu quy tắc tiến hóa đống xương theo thành phố đang chọn trên bản đồ thế giới
		if (BonePileDatabase.BonePileGeneratorsDefinition.BonePileEvolutionDefinitions.TryGetValue(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.BonePilesEvolutionId, out var value))
		{
			foreach (Tuple<int, int> item in value)
			{
				// Item1: Cột mốc ngày (DayNumber), Item2: Cấp độ tương ứng (Level)
				if (TPSingleton<GameManager>.Instance.Game.DayNumber >= item.Item1)
				{
					level = item.Item2;
					continue;
				}
				break;
			}
		}

		// Thiết lập cấp độ mới cho module sản xuất của đống xương
		base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.ProductionModule.Level = level;
	}

	#endregion
}
