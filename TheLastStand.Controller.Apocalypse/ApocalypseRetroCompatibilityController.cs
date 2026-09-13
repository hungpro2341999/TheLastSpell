using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.Manager;
using TheLastStand.Model.Apocalypse;
using TheLastStand.Model.WorldMap;

namespace TheLastStand.Controller.Apocalypse;

/// <summary>
/// Bộ điều khiển xử lý tính tương thích ngược (Retro-compatibility / Backward Compatibility) của hệ thống Apocalypse.
/// Giúp chuyển đổi các bản lưu game (Save files) từ hệ thống cấp độ cố định cũ (Level 1, 2, 3...)
/// sang hệ thống Modifiers tự do tùy biến mới (Reworked Apocalypse System).
/// </summary>
public static class ApocalypseRetroCompatibilityController
{
	#region Fields & State

	/// <summary>Cờ đánh dấu bảng ánh xạ tương đương giữa các level đã được khởi tạo hay chưa.</summary>
	private static bool isInitialized;

	/// <summary>Danh sách các quy tắc quy đổi tương đương giữa cấp độ Apocalypse cũ và các Modifier mới.</summary>
	private static List<ApocalypseRetroCompatibilityLevelEquivalence> apocalypseLevelsEquivalences = new List<ApocalypseRetroCompatibilityLevelEquivalence>();

	#endregion

	#region Public API - Retro-Compatibility Execution

	/// <summary>
	/// Áp dụng tương thích ngược cho dữ liệu Apocalypse trong bản lưu game (Game Save).
	/// Nếu phiên bản save cũ (<= 23), hàm sẽ chuyển đổi cấp độ Apocalypse cũ thành danh sách các Modifier Step tương ứng.
	/// </summary>
	/// <param name="saveVersion">Phiên bản lưu của file save hiện tại.</param>
	/// <param name="apocalypseLevel">Cấp độ Apocalypse trong bản lưu cũ.</param>
	public static void ApplyRetroCompatibilityToGameSaveApocalypse(int saveVersion, int apocalypseLevel)
	{
		// Từ save version 24 trở đi, game đã sử dụng hệ thống Apocalypse mới nên không cần quy đổi
		if (saveVersion > 23)
		{
			return;
		}
		
		// Tìm quy tắc tương đương cho cấp độ cũ
		ApocalypseRetroCompatibilityLevelEquivalence apocalypseLevelEquivalence = GetApocalypseLevelEquivalence(apocalypseLevel);
		if (apocalypseLevelEquivalence != null)
		{
			if (TPSingleton<ApocalypseManager>.Exist())
			{
				TPSingleton<ApocalypseManager>.Instance.Log($"Trying to apply apocalypse retro compatibility to selected apocalypse modifiers on apocalypse level '{apocalypseLevel}' with save version {saveVersion} !", CLogLevel.MAJOR, forcePrintInUnity: true);
				TPSingleton<ApocalypseManager>.Instance.Log(apocalypseLevelEquivalence.ToString(), CLogLevel.MAJOR, forcePrintInUnity: true);
			}
			
			// Thiết lập lại danh sách modifier trong ApocalypseManager dựa trên quy đổi
			ApocalypseManager.SetApocalypse(GetApocalypseModifierStepDefinitionsFromLevelEquivalence(apocalypseLevelEquivalence), computeLevel: true, computeEffects: false);
		}
	}

	/// <summary>
	/// Áp dụng tương thích ngược cho tiến trình của từng thành phố trên World Map.
	/// Chuyển đổi chỉ số MaxApocalypsePassed của thành phố và đánh dấu hoàn thành các bước Modifier tương ứng.
	/// </summary>
	/// <param name="saveVersion">Phiên bản lưu của file save.</param>
	/// <param name="worldMapCity">Đối tượng thành phố trên World Map cần cập nhật tiến trình.</param>
	public static void ApplyRetroCompatibilityToWorldMapCity(int saveVersion, WorldMapCity worldMapCity)
	{
		// Từ save version 14 trở đi, tiến trình thành phố đã tuân theo định dạng mới
		if (saveVersion > 13)
		{
			return;
		}
		
		ApocalypseRetroCompatibilityLevelEquivalence apocalypseLevelEquivalence = GetApocalypseLevelEquivalence(worldMapCity.MaxApocalypsePassed);
		if (apocalypseLevelEquivalence == null)
		{
			return;
		}
		
		int maxApocalypsePassed = worldMapCity.MaxApocalypsePassed;
		int correspondingLevel = apocalypseLevelEquivalence.CorrespondingLevel;
		if (TPSingleton<ApocalypseManager>.Exist())
		{
			TPSingleton<ApocalypseManager>.Instance.Log($"Trying to apply apocalypse retro compatibility to city '{worldMapCity.CityDefinition.Id}' with save version {saveVersion} ! (from apocalypse level {maxApocalypsePassed} to {correspondingLevel})", CLogLevel.MAJOR, forcePrintInUnity: true);
			TPSingleton<ApocalypseManager>.Instance.Log(apocalypseLevelEquivalence.ToString(), CLogLevel.DETAILED, forcePrintInUnity: true);
		}
		
		// Cập nhật cấp độ vượt qua tối đa sang cấp tương ứng mới
		worldMapCity.MaxApocalypsePassed = correspondingLevel;
		
		// Đánh dấu đã hoàn thành từng bước modifier quy đổi trong dữ liệu thành phố
		foreach (ApocalypseModifierIdAndStepIndex item in apocalypseLevelEquivalence.CorrespondingModifiersStep)
		{
			worldMapCity.WorldMapCityController.TryAddingCompletedApocalypseModifierStep(item.ModifierId, item.StepIndex);
		}
	}

	#endregion

	#region Level Equivalences & Definition Mapping

	/// <summary>
	/// Lấy đối tượng quy đổi tương đương cho một cấp độ Apocalypse cũ.
	/// Phương thức này cộng dồn (accumulate) tất cả các modifier của các cấp độ cũ từ 1 đến oldApocalypseLevel.
	/// </summary>
	/// <param name="oldApocalypseLevel">Cấp độ Apocalypse trong hệ thống cũ.</param>
	/// <returns>Đối tượng tương đương chứa tổng cấp độ mới và toàn bộ danh sách ModifierStep, hoặc null nếu không khớp.</returns>
	public static ApocalypseRetroCompatibilityLevelEquivalence GetApocalypseLevelEquivalence(int oldApocalypseLevel)
	{
		InitializeIfNeeded();
		bool flag = false;
		ApocalypseRetroCompatibilityLevelEquivalence apocalypseRetroCompatibilityLevelEquivalence = new ApocalypseRetroCompatibilityLevelEquivalence(oldApocalypseLevel, new List<ApocalypseModifierIdAndStepIndex>());
		
		// Duyệt qua tất cả các mốc quy đổi và cộng dồn các mốc <= oldApocalypseLevel
		for (int i = 0; i < apocalypseLevelsEquivalences.Count; i++)
		{
			ApocalypseRetroCompatibilityLevelEquivalence apocalypseRetroCompatibilityLevelEquivalence2 = apocalypseLevelsEquivalences[i];
			if (apocalypseRetroCompatibilityLevelEquivalence2.OldSystemLevel <= oldApocalypseLevel)
			{
				apocalypseRetroCompatibilityLevelEquivalence.CorrespondingLevel += apocalypseRetroCompatibilityLevelEquivalence2.CorrespondingLevel;
				apocalypseRetroCompatibilityLevelEquivalence.CorrespondingModifiersStep.AddRange(apocalypseRetroCompatibilityLevelEquivalence2.CorrespondingModifiersStep);
				flag = true;
			}
		}
		if (flag)
		{
			return apocalypseRetroCompatibilityLevelEquivalence;
		}
		return null;
	}

	/// <summary>
	/// Chuyển đổi danh sách (ModifierId, StepIndex) từ đối tượng quy đổi thành danh sách các ApocalypseModifierStepDefinition cụ thể từ Database.
	/// </summary>
	/// <param name="levelEquivalence">Dữ liệu quy đổi cấp độ tương đương.</param>
	/// <returns>Danh sách các đối tượng ApocalypseModifierStepDefinition tương ứng.</returns>
	public static List<ApocalypseModifierStepDefinition> GetApocalypseModifierStepDefinitionsFromLevelEquivalence(ApocalypseRetroCompatibilityLevelEquivalence levelEquivalence)
	{
		List<ApocalypseModifierStepDefinition> list = new List<ApocalypseModifierStepDefinition>();
		if (levelEquivalence == null)
		{
			return list;
		}
		foreach (ApocalypseModifierIdAndStepIndex item in levelEquivalence.CorrespondingModifiersStep)
		{
			if (ApocalypseDatabase.ModifierDefinitions.TryGetValue(item.ModifierId, out var value))
			{
				// Kiểm tra index hợp lệ, nếu vượt quá thì lấy bước cuối cùng
				if (item.StepIndex < value.StepDefinitions.Count)
				{
					list.Add(value.StepDefinitions[item.StepIndex]);
				}
				else
				{
					list.Add(value.StepDefinitions[^1]);
				}
			}
		}
		return list;
	}

	#endregion

	#region Initialization & Internal Helpers

	/// <summary>
	/// Khởi tạo bảng ánh xạ quy đổi giữa hệ thống cấp độ cũ (1 đến 6) sang hệ thống Modifiers mới nếu chưa được khởi tạo.
	/// </summary>
	private static void InitializeIfNeeded()
	{
		if (!isInitialized)
		{
			apocalypseLevelsEquivalences.Clear();
			
			// Cấp 1 cũ: Tăng máu quái (EnemyHealthModifier - step 0)
			ApocalypseRetroCompatibilityLevelEquivalence item = new ApocalypseRetroCompatibilityLevelEquivalence(1, GetModifiersAtStep(new ApocalypseModifierIdAndStepIndex("EnemyHealthModifier", 0)));
			apocalypseLevelsEquivalences.Add(item);
			
			// Cấp 2 cũ: Tăng quy mô đợt quái (WaveSizeModifier - step 1)
			item = new ApocalypseRetroCompatibilityLevelEquivalence(2, GetModifiersAtStep(new ApocalypseModifierIdAndStepIndex("WaveSizeModifier", 1)));
			apocalypseLevelsEquivalences.Add(item);
			
			// Cấp 3 cũ: Thêm cọc sinh sương mù (FogSpawnerModifier - step 0)
			item = new ApocalypseRetroCompatibilityLevelEquivalence(3, GetModifiersAtStep(new ApocalypseModifierIdAndStepIndex("FogSpawnerModifier", 0)));
			apocalypseLevelsEquivalences.Add(item);
			
			// Cấp 4 cũ: Tăng chi phí sản xuất và phòng thủ (ProductionCostModifier - step 1, DefenseCostModifier - step 0)
			item = new ApocalypseRetroCompatibilityLevelEquivalence(4, GetModifiersAtStep(new ApocalypseModifierIdAndStepIndex("ProductionCostModifier", 1), new ApocalypseModifierIdAndStepIndex("DefenseCostModifier", 0)));
			apocalypseLevelsEquivalences.Add(item);
			
			// Cấp 5 cũ: Quái di chuyển nhanh hơn (FasterEnemiesModifier - step 0)
			item = new ApocalypseRetroCompatibilityLevelEquivalence(5, GetModifiersAtStep(new ApocalypseModifierIdAndStepIndex("FasterEnemiesModifier", 0)));
			apocalypseLevelsEquivalences.Add(item);
			
			// Cấp 6 cũ: Vật phẩm rớt ra có thuộc tính tiêu cực (ItemsNegativeAffixesModifier - step 0)
			item = new ApocalypseRetroCompatibilityLevelEquivalence(6, GetModifiersAtStep(new ApocalypseModifierIdAndStepIndex("ItemsNegativeAffixesModifier", 0)));
			apocalypseLevelsEquivalences.Add(item);
			
			isInitialized = true;
		}
	}

	/// <summary>
	/// Hàm an toàn hỗ trợ lọc và kiểm tra tính hợp lệ của các cặp (ModifierId, StepIndex).
	/// Nếu StepIndex vượt quá số bước có sẵn trong database, tự động điều chỉnh về bước cao nhất hiện có.
	/// </summary>
	/// <param name="modifiersIdsAndStepIndex">Danh sách các cặp ID và StepIndex cần kiểm tra.</param>
	/// <returns>Danh sách các cặp hợp lệ.</returns>
	private static List<ApocalypseModifierIdAndStepIndex> GetModifiersAtStep(params ApocalypseModifierIdAndStepIndex[] modifiersIdsAndStepIndex)
	{
		List<ApocalypseModifierIdAndStepIndex> list = new List<ApocalypseModifierIdAndStepIndex>();
		if (modifiersIdsAndStepIndex == null || modifiersIdsAndStepIndex.Length == 0)
		{
			return list;
		}
		foreach (ApocalypseModifierIdAndStepIndex apocalypseModifierIdAndStepIndex in modifiersIdsAndStepIndex)
		{
			// Kiểm tra modifier có tồn tại trong database không
			if (!ApocalypseDatabase.ModifierDefinitions.TryGetValue(apocalypseModifierIdAndStepIndex.ModifierId, out var value))
			{
				TPSingleton<ApocalypseManager>.Instance.LogError("Apocalypse rework retro compatibility issue, couldn't find modifier '" + apocalypseModifierIdAndStepIndex.ModifierId + "' !", CLogLevel.MAJOR);
			}
			// Kiểm tra nếu step index vượt quá số lượng step được định nghĩa
			else if (apocalypseModifierIdAndStepIndex.StepIndex >= value.StepDefinitions.Count)
			{
				TPSingleton<ApocalypseManager>.Instance.LogError($"Apocalypse rework retro compatibility issue, couldn't find modifier '{apocalypseModifierIdAndStepIndex.ModifierId}' with step '{apocalypseModifierIdAndStepIndex.StepIndex}', retrieving last available index !", CLogLevel.MAJOR);
				list.Add(new ApocalypseModifierIdAndStepIndex(apocalypseModifierIdAndStepIndex.ModifierId, value.StepDefinitions.Count - 1));
			}
			else
			{
				list.Add(apocalypseModifierIdAndStepIndex);
			}
		}
		return list;
	}

	#endregion
}
