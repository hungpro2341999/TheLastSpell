using System;
using TPLib;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Manager;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Building.BuildingPassive;

/// <summary>
/// Bộ điều khiển hiệu ứng nội tại cập nhật cấp độ Cửa Hàng (Update Shop Level).
/// Dựa vào tiến trình số ngày sinh tồn (DayNumber) và bảng tiến hóa cửa hàng của thành phố (ShopEvolutionId)
/// để nâng cấp chất lượng và cấp độ các trang bị xuất hiện trong Shop.
/// </summary>
public class UpdateShopLevelController : BuildingPassiveEffectController
{
	#region Properties

	/// <summary>
	/// Model lưu trữ định nghĩa và dữ liệu cập nhật cấp độ Cửa hàng.
	/// </summary>
	public UpdateShopLevel UpdateShopLevel => base.BuildingPassiveEffect as UpdateShopLevel;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo UpdateShopLevelController với module nội tại và định nghĩa cấu hình.
	/// </summary>
	/// <param name="buildingPassivesModule">Module nội tại của công trình Cửa hàng.</param>
	/// <param name="updateShopLevelDefinition">Định nghĩa cấu hình cập nhật cấp độ Shop.</param>
	public UpdateShopLevelController(PassivesModule buildingPassivesModule, UpdateShopLevelDefinition updateShopLevelDefinition)
	{
		base.BuildingPassiveEffect = new UpdateShopLevel(buildingPassivesModule, updateShopLevelDefinition, this);
	}

	#endregion

	#region Passive Effect Execution

	/// <summary>
	/// Thực thi kiểm tra và cập nhật cấp độ của Cửa hàng theo số ngày trôi qua trong game.
	/// </summary>
	public override void Apply()
	{
		int level = 1;

		// Tra cứu bảng tiến hóa cấp độ Shop theo thành phố hiện tại
		if (BuildingDatabase.ShopDefinition.ShopEvolutionDefinitions.TryGetValue(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.ShopEvolutionId, out var value))
		{
			foreach (Tuple<int, int> item in value.LevelsPerDay)
			{
				// Item1: Cột mốc ngày (DayNumber), Item2: Cấp độ tương ứng của Shop (Level)
				if (TPSingleton<GameManager>.Instance.Game.DayNumber >= item.Item1)
				{
					level = item.Item2;
					continue;
				}
				break;
			}
		}

		// Gán cấp độ mới vào module sản xuất của Cửa hàng
		base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.ProductionModule.Level = level;
	}

	#endregion
}
