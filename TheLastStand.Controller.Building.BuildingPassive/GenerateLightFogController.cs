using System.Collections.Generic;
using System.Linq;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Manager;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;
using TheLastStand.Serialization.Building.BuildingPassive.PassiveEffect;
using UnityEngine;

namespace TheLastStand.Controller.Building.BuildingPassive;

/// <summary>
/// Bộ điều khiển hiệu ứng nội tại sinh sương mù ánh sáng (Light Fog).
/// Tạo ra và duy trì vùng sương ánh sáng xung quanh công trình theo ma trận mẫu (Pattern),
/// đăng ký nguồn cung cấp với FogController và xử lý hiệu ứng mờ dần (Fade in / Fade out).
/// </summary>
public class GenerateLightFogController : BuildingPassiveEffectController
{
	#region Properties

	/// <summary>
	/// Model lưu trữ dữ liệu hiệu ứng sinh sương mù ánh sáng và mẫu hoa văn (Pattern).
	/// </summary>
	public GenerateLightFog GenerateLightFog => base.BuildingPassiveEffect as GenerateLightFog;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo GenerateLightFogController từ dữ liệu lưu (Save Deserialization) và đăng ký nhà cung cấp sương với FogController.
	/// </summary>
	/// <param name="container">Dữ liệu tuần tự hóa của hiệu ứng.</param>
	/// <param name="buildingPassivesModule">Module nội tại của công trình.</param>
	/// <param name="generateLightFogDefinition">Định nghĩa cấu hình sinh sương ánh sáng.</param>
	public GenerateLightFogController(SerializedGenerateLightFog container, PassivesModule buildingPassivesModule, GenerateLightFogDefinition generateLightFogDefinition)
	{
		base.BuildingPassiveEffect = new GenerateLightFog(container, buildingPassivesModule, generateLightFogDefinition, this);
		FogController.RegisterSupplier(GenerateLightFog);
	}

	/// <summary>
	/// Khởi tạo mới GenerateLightFogController trong ván đấu.
	/// </summary>
	/// <param name="buildingPassivesModule">Module nội tại của công trình.</param>
	/// <param name="generateLightFogDefinition">Định nghĩa cấu hình sinh sương ánh sáng.</param>
	public GenerateLightFogController(PassivesModule buildingPassivesModule, GenerateLightFogDefinition generateLightFogDefinition)
	{
		base.BuildingPassiveEffect = new GenerateLightFog(buildingPassivesModule, generateLightFogDefinition, this);
	}

	#endregion

	#region Passive Effect Lifecycle

	/// <summary>
	/// Áp dụng hiệu ứng: Tính toán các ô tile thuộc pattern và tăng buffer sương mù ánh sáng tương ứng.
	/// </summary>
	public override void Apply()
	{
		// Cập nhật vùng sương mù ánh sáng theo mẫu lên bản đồ
		FogController.SetLightFogTilesFromDictionnary(FogController.IncrementLightFogTilesBuffer(GetTilesFromPattern()), FogManager.LightFogFadeInEaseAndDuration, FogManager.LightFogFadeOutEaseAndDuration, FogManager.LightFogDisappearEaseAndDuration);

		// Nếu công trình không cho phép sương mù tồn tại trên chính thân mình, bật cờ loại trừ
		if (!GenerateLightFog.GenerateLightFogDefinition.CanLightFogExistOnSelf)
		{
			FogController.SetLightFogTilesFromDictionnary(FogController.ToggleLightFogTiles(base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.OccupiedTiles), FogManager.LightFogFadeInEaseAndDuration, FogManager.LightFogFadeOutEaseAndDuration, FogManager.LightFogDisappearEaseAndDuration);
		}
	}

	/// <summary>
	/// Hoàn tác / gỡ bỏ hiệu ứng: Hủy đăng ký nhà cung cấp và giảm bộ đệm sương mù ánh sáng trên các ô tương ứng.
	/// </summary>
	public override void Unapply()
	{
		FogController.UnregisterSupplier(GenerateLightFog);
		FogController.SetLightFogTilesFromDictionnary(FogController.DecrementLightFogTilesBuffer(GetTilesFromPattern()), FogManager.LightFogFadeInEaseAndDuration, FogManager.LightFogFadeOutEaseAndDuration, FogManager.LightFogDisappearEaseAndDuration);
	}

	#endregion

	#region Pattern Calculation

	/// <summary>
	/// Lấy danh sách các ô Tile trên bản đồ theo ma trận tọa độ tương đối (Pattern) của công trình.
	/// </summary>
	/// <returns>Danh sách các ô Tile được bao phủ bởi sương mù ánh sáng.</returns>
	private List<Tile> GetTilesFromPattern()
	{
		List<Tile> list = new List<Tile>();
		Vector2Int position = base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.OriginTile.Position;

		foreach (Vector2Int item in GenerateLightFog.Pattern)
		{
			Tile tile = TileMapManager.GetTile(position.x + item.x, position.y + item.y);
			if (tile != null)
			{
				list.Add(tile);
			}
		}

		// Xử lý bao gồm hoặc loại trừ các ô chính công trình đang chiếm dụng
		if (GenerateLightFog.GenerateLightFogDefinition.CanLightFogExistOnSelf)
		{
			list.AddRange(base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.OccupiedTiles);
			return list.Distinct().ToList();
		}

		return list.Except(base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.OccupiedTiles).ToList();
	}

	#endregion
}
