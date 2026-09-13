using System;
using System.Collections.Generic;
using TheLastStand.Controller.Apocalypse.ApocalypseEffects;
using TheLastStand.Database;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.Definition.Apocalypse.ApocalypseEffects;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.Model.Apocalypse;
using TheLastStand.Model.Apocalypse.ApocalypseEffects;
using UnityEngine;

namespace TheLastStand.Controller.Apocalypse;

/// <summary>
/// Bộ điều khiển trung tâm (Core Controller) của hệ thống Apocalypse (Độ khó Tận Thế).
/// Chịu trách nhiệm:
/// 1. Quản lý các bước tùy biến độ khó đã chọn (Modifier Steps).
/// 2. Tính toán tổng cấp độ Apocalypse hiện tại (Current Level).
/// 3. Tiền xử lý và gom nhóm (Pre-compute / Cache) toàn bộ các hiệu ứng (Effects/Debuffs)
///    vào Model Apocalypse để các hệ thống khác (Combat, Unit, Building, WorldMap...) tra cứu tức thì với độ phức tạp O(1).
/// 4. Quản lý vòng đời kích hoạt/hủy kích hoạt của các hiệu ứng theo lượt.
/// </summary>
public class ApocalypseController
{
	#region Properties & Events

	/// <summary>
	/// Model lưu trữ toàn bộ dữ liệu trạng thái và các bảng tra cứu hiệu ứng của Apocalypse.
	/// </summary>
	public TheLastStand.Model.Apocalypse.Apocalypse Apocalypse { get; }

	/// <summary>
	/// Sự kiện được kích hoạt mỗi khi cấp độ Apocalypse được tính toán hoặc cập nhật lại.
	/// </summary>
	public event Action OnApocalypseLevelComputed = delegate
	{
	};

	#endregion

	#region Constructor

	/// <summary>
	/// Khởi tạo một phiên bản ApocalypseController mới.
	/// Tự động khởi tạo Model Apocalypse và thực hiện tiền tính toán toàn bộ dữ liệu.
	/// </summary>
	/// <param name="stepDefinitions">Danh sách các bước modifier khởi đầu (nếu có).</param>
	public ApocalypseController(List<ApocalypseModifierStepDefinition> stepDefinitions = null)
	{
		Apocalypse = new TheLastStand.Model.Apocalypse.Apocalypse(this, stepDefinitions);
		ComputeAllData();
	}

	#endregion

	#region Public API - Modifiers Selection & Management

	/// <summary>
	/// Thêm một bước tùy biến (Modifier Step) vào cấu hình Apocalypse đang chọn.
	/// </summary>
	/// <param name="modifierStepDefinition">Định nghĩa bước modifier cần thêm.</param>
	/// <param name="computeLevel">Có tính toán lại tổng cấp độ Apocalypse ngay hay không (mặc định: true).</param>
	/// <param name="computeEffects">Có tính toán lại toàn bộ hiệu ứng ngay hay không (mặc định: false).</param>
	/// <param name="removeExistingModifier">Nếu true, tự động gỡ bỏ các bước khác thuộc cùng Modifier trước khi thêm bước mới (mặc định: true).</param>
	public void AddSelectedModifierStep(ApocalypseModifierStepDefinition modifierStepDefinition, bool computeLevel = true, bool computeEffects = false, bool removeExistingModifier = true)
	{
		// Nếu bước này đã được chọn từ trước, bỏ qua để tránh trùng lặp
		if (Apocalypse.ModifierStepDefinitions.Contains(modifierStepDefinition))
		{
			return;
		}
		
		// Nếu yêu cầu loại bỏ bước cũ cùng loại: tìm định nghĩa modifier cha và gỡ bỏ tất cả các bước của nó đã chọn trước đó
		if (removeExistingModifier && ApocalypseDatabase.ModifierDefinitionsFromStepDefinitions.TryGetValue(modifierStepDefinition, out var value) && value != null)
		{
			foreach (ApocalypseModifierStepDefinition stepDefinition in value.StepDefinitions)
			{
				RemoveSelectedModifierStep(stepDefinition, computeLevel: false);
			}
		}
		
		// Thêm bước mới vào danh sách
		Apocalypse.ModifierStepDefinitions.Add(modifierStepDefinition);
		ComputeAllData(computeLevel, computeEffects);
	}

	/// <summary>
	/// Gỡ bỏ một bước Modifier cụ thể khỏi danh sách đang chọn.
	/// </summary>
	/// <param name="modifierStepDefinition">Bước modifier cần gỡ bỏ.</param>
	/// <param name="computeLevel">Có tính lại tổng cấp độ ngay không.</param>
	/// <param name="computeEffects">Có tính lại toàn bộ hiệu ứng ngay không.</param>
	public void RemoveSelectedModifierStep(ApocalypseModifierStepDefinition modifierStepDefinition, bool computeLevel = true, bool computeEffects = false)
	{
		if (Apocalypse.ModifierStepDefinitions.Contains(modifierStepDefinition))
		{
			Apocalypse.ModifierStepDefinitions.Remove(modifierStepDefinition);
			ComputeAllData(computeLevel, computeEffects);
		}
	}

	/// <summary>
	/// Thiết lập lại toàn bộ danh sách các bước Modifier đã chọn từ một danh sách mới.
	/// </summary>
	/// <param name="stepDefinitions">Danh sách các bước modifier mới cần gán.</param>
	/// <param name="computeLevel">Có tính lại tổng cấp độ ngay không (mặc định: true).</param>
	/// <param name="computeEffects">Có tính lại toàn bộ hiệu ứng ngay không (mặc định: true).</param>
	public void SetSelectedModifierSteps(List<ApocalypseModifierStepDefinition> stepDefinitions, bool computeLevel = true, bool computeEffects = true)
	{
		Apocalypse.ModifierStepDefinitions.Clear();
		if (stepDefinitions != null)
		{
			foreach (ApocalypseModifierStepDefinition stepDefinition in stepDefinitions)
			{
				AddSelectedModifierStep(stepDefinition, computeLevel: false);
			}
		}
		ComputeAllData(computeLevel, computeEffects);
	}

	/// <summary>
	/// Xóa bỏ toàn bộ các bước Modifier đang chọn trong cấu hình Apocalypse.
	/// </summary>
	/// <param name="computeLevel">Có tính lại tổng cấp độ ngay không (mặc định: true).</param>
	/// <param name="computeEffects">Có tính lại toàn bộ hiệu ứng ngay không (mặc định: false).</param>
	public void ClearAllSelectedModifierSteps(bool computeLevel = true, bool computeEffects = false)
	{
		Apocalypse.ModifierStepDefinitions.Clear();
		ComputeAllData(computeLevel, computeEffects);
	}

	#endregion

	#region Public API - Effects & Turn Checking

	/// <summary>
	/// Kiểm tra và kích hoạt lại các hiệu ứng bổ sung chỉ số quái theo lượt (Turn Condition) khi tải game (Save/Load).
	/// </summary>
	public void CheckEffectsActivationOnTurnCondition()
	{
		foreach (AddEnemiesStatModifierFromTurnApocalypseEffect addEnemiesStatModifierFromTurnEffect in Apocalypse.AddEnemiesStatModifierFromTurnEffects)
		{
			if (!addEnemiesStatModifierFromTurnEffect.IsActive)
			{
				addEnemiesStatModifierFromTurnEffect.StatModifierEffectController.CheckIfCanActivate(onLoad: true);
			}
		}
	}

	#endregion

	#region Core Computation Pipeline

	/// <summary>
	/// Điều phối tiền tính toán (Pre-compute) dữ liệu cấp độ và/hoặc hiệu ứng cho Apocalypse.
	/// </summary>
	/// <param name="computeLevel">Cờ cho biết có tính lại tổng cấp độ Apocalypse không.</param>
	/// <param name="computeEffects">Cờ cho biết có tính lại toàn bộ danh sách hiệu ứng không.</param>
	public void ComputeAllData(bool computeLevel = true, bool computeEffects = true)
	{
		if (computeLevel)
		{
			ComputeCurrentLevel();
		}
		if (computeEffects)
		{
			ComputeEffectsAndModifiers();
		}
	}

	/// <summary>
	/// Tính toán tổng cấp độ Apocalypse hiện tại bằng cách cộng dồn điểm cấp độ (ApocalypseLevel) của từng bước.
	/// Sau khi tính xong, kích hoạt sự kiện OnApocalypseLevelComputed.
	/// </summary>
	private void ComputeCurrentLevel()
	{
		Apocalypse.CurrentLevel = 0;
		if (Apocalypse.ModifierStepDefinitions == null || Apocalypse.ModifierStepDefinitions.Count == 0)
		{
			this.OnApocalypseLevelComputed();
			return;
		}
		int count = Apocalypse.ModifierStepDefinitions.Count;
		for (int i = 0; i < count; i++)
		{
			Apocalypse.CurrentLevel += Apocalypse.ModifierStepDefinitions[i].ApocalypseLevel;
		}
		this.OnApocalypseLevelComputed();
	}

	/// <summary>
	/// Gom tất cả các hiệu ứng (Effects) từ các bước Modifier đã chọn vào danh sách tổng AllEffects.
	/// </summary>
	private void ComputeAllEffects()
	{
		Apocalypse.AllEffects.Clear();
		foreach (ApocalypseModifierStepDefinition modifierStepDefinition in Apocalypse.ModifierStepDefinitions)
		{
			if (modifierStepDefinition.Effects != null && modifierStepDefinition.Effects.Count > 0)
			{
				Apocalypse.AllEffects.AddRange(modifierStepDefinition.Effects);
			}
		}
	}

	/// <summary>
	/// Chuỗi tổng hợp: gom tất cả hiệu ứng và phân loại tính toán chi tiết từng nhóm chỉ số.
	/// </summary>
	private void ComputeEffectsAndModifiers()
	{
		ComputeAllEffects();
		ComputeEffectsModifiers();
	}

	/// <summary>
	/// Thực hiện tiền tính toán và phân bổ toàn bộ các loại hiệu ứng vào các cấu trúc dữ liệu tra cứu trong Model.
	/// </summary>
	private void ComputeEffectsModifiers()
	{
		ComputeAffixesFlags();
		ComputeBuildingsDeadZoneRangeModifiers();
		ComputeBuildingsNotDemolishable();
		ComputeEnemiesInjuryStageStatModifiers();
		ComputeEnemiesProgressionOffset();
		ComputeEnemiesSpawnWaveWeightMultiplier();
		ComputeEnemiesStatsBaseValueModifiers();
		ComputeEnemiesStatModifierAfterTurn();
		ComputeFogSpawnersMultiplier();
		ComputeForbiddenBuildingCategoryAroundMagicCircle();
		ComputeInnSlotsRecruitmentLevel();
		ComputeMagicCircleStartingHealthMultiplier();
		ComputePanicGainMultiplier();
		ComputePlayableUnitBlockLineOfSight();
		ComputeRemoveStartingPlayableUnit();
		ComputeSkillProgressionFlags();
	}

	#endregion

	#region Specific Effect Computations - Buildings & Magic Circle

	/// <summary>
	/// Tính toán bán kính vùng chết (Dead Zone - vùng cấm xây xung quanh) cho từng loại công trình và xác định bán kính lớn nhất.
	/// </summary>
	private void ComputeBuildingsDeadZoneRangeModifiers()
	{
		Apocalypse.BuildingsModifiedDeadZoneRange.Clear();
		int num = 1;
		foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
		{
			if (!(allEffect is ModifyBuildingsDeadZoneRangeApocalypseEffectDefinition modifyBuildingsDeadZoneRangeApocalypseEffectDefinition))
			{
				continue;
			}
			foreach (string buildingsId in modifyBuildingsDeadZoneRangeApocalypseEffectDefinition.BuildingsIds)
			{
				if (Apocalypse.BuildingsModifiedDeadZoneRange.ContainsKey(buildingsId))
				{
					Apocalypse.BuildingsModifiedDeadZoneRange[buildingsId] = modifyBuildingsDeadZoneRangeApocalypseEffectDefinition.Range;
				}
				else
				{
					Apocalypse.BuildingsModifiedDeadZoneRange.Add(buildingsId, modifyBuildingsDeadZoneRangeApocalypseEffectDefinition.Range);
				}
			}
			if (num < modifyBuildingsDeadZoneRangeApocalypseEffectDefinition.Range)
			{
				num = modifyBuildingsDeadZoneRangeApocalypseEffectDefinition.Range;
			}
		}
		Apocalypse.BuildingsMaxDeadZoneRange = num;
	}

	/// <summary>
	/// Gom danh sách ID các công trình bị cấm phá hủy (Not Demolishable) để ngăn người chơi dỡ nhà thu hồi nguyên liệu.
	/// </summary>
	private void ComputeBuildingsNotDemolishable()
	{
		Apocalypse.BuildingsNotDemolishableAnymore.Clear();
		foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
		{
			if (!(allEffect is SetBuildingsNotDemolishableApocalypseEffectDefinition setBuildingsNotDemolishableApocalypseEffectDefinition))
			{
				continue;
			}
			foreach (string buildingsId in setBuildingsNotDemolishableApocalypseEffectDefinition.BuildingsIds)
			{
				if (!Apocalypse.BuildingsNotDemolishableAnymore.Contains(buildingsId))
				{
					Apocalypse.BuildingsNotDemolishableAnymore.Add(buildingsId);
				}
			}
		}
	}

	/// <summary>
	/// Xác định danh mục công trình bị cấm xây dựng xung quanh Vòng tròn Ma thuật (Magic Circle) theo từng cấp bán kính.
	/// </summary>
	private void ComputeForbiddenBuildingCategoryAroundMagicCircle()
	{
		Apocalypse.ForbiddenBuildingCategoriesAroundMagicCircleByRange.Clear();
		foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
		{
			if (!(allEffect is ForbidBuildingCategoryAroundMagicCircleApocalypseEffectDefinition forbidBuildingCategoryAroundMagicCircleApocalypseEffectDefinition))
			{
				continue;
			}
			int i = 1;
			int radiusRange = forbidBuildingCategoryAroundMagicCircleApocalypseEffectDefinition.RadiusRange;
			if (i > radiusRange)
			{
				continue;
			}
			// Điền danh mục cấm vào từng ô bán kính từ 1 đến RadiusRange
			for (; i <= radiusRange; i++)
			{
				if (Apocalypse.ForbiddenBuildingCategoriesAroundMagicCircleByRange.ContainsKey(i))
				{
					if (!Apocalypse.ForbiddenBuildingCategoriesAroundMagicCircleByRange[i].Contains(forbidBuildingCategoryAroundMagicCircleApocalypseEffectDefinition.BuildingCategory))
					{
						Apocalypse.ForbiddenBuildingCategoriesAroundMagicCircleByRange[i].Add(forbidBuildingCategoryAroundMagicCircleApocalypseEffectDefinition.BuildingCategory);
					}
				}
				else
				{
					Apocalypse.ForbiddenBuildingCategoriesAroundMagicCircleByRange.Add(i, new List<BuildingDefinition.E_BuildingCategory> { forbidBuildingCategoryAroundMagicCircleApocalypseEffectDefinition.BuildingCategory });
				}
			}
		}
	}

	/// <summary>
	/// Tính toán hệ số nhân lượng máu ban đầu của Vòng tròn Ma thuật (Magic Circle) khi bắt đầu màn chơi.
	/// </summary>
	private void ComputeMagicCircleStartingHealthMultiplier()
	{
		Apocalypse.MagicCircleStartingHealthMultiplier = 1f;
		foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
		{
			if (allEffect is ModifyMagicCircleStartingHealthApocalypseEffectDefinition modifyMagicCircleStartingHealthApocalypseEffectDefinition)
			{
				Apocalypse.MagicCircleStartingHealthMultiplier = modifyMagicCircleStartingHealthApocalypseEffectDefinition.HealthMultiplier;
			}
		}
	}

	#endregion

	#region Specific Effect Computations - Enemies

	/// <summary>
	/// Tính toán các điều chỉnh tăng/giảm chỉ số cơ bản (Máu, Damage, Giáp, Di chuyển...) cho từng loại quái cụ thể.
	/// </summary>
	private void ComputeEnemiesStatsBaseValueModifiers()
	{
		Apocalypse.EnemiesStatsBaseValueModifiers.Clear();
		for (int num = Apocalypse.AllEffects.Count - 1; num >= 0; num--)
		{
			if (Apocalypse.AllEffects[num] is EnemiesStatBaseValueModifierApocalypseEffectDefinition enemiesStatBaseValueModifierApocalypseEffectDefinition)
			{
				for (int num2 = enemiesStatBaseValueModifierApocalypseEffectDefinition.AffectedEnemies.Count - 1; num2 >= 0; num2--)
				{
					if (!Apocalypse.EnemiesStatsBaseValueModifiers.ContainsKey(enemiesStatBaseValueModifierApocalypseEffectDefinition.AffectedEnemies[num2]))
					{
						Apocalypse.EnemiesStatsBaseValueModifiers.Add(enemiesStatBaseValueModifierApocalypseEffectDefinition.AffectedEnemies[num2], new Dictionary<UnitStatDefinition.E_Stat, float>());
					}
					if (!Apocalypse.EnemiesStatsBaseValueModifiers[enemiesStatBaseValueModifierApocalypseEffectDefinition.AffectedEnemies[num2]].ContainsKey(enemiesStatBaseValueModifierApocalypseEffectDefinition.Stat))
					{
						Apocalypse.EnemiesStatsBaseValueModifiers[enemiesStatBaseValueModifierApocalypseEffectDefinition.AffectedEnemies[num2]].Add(enemiesStatBaseValueModifierApocalypseEffectDefinition.Stat, enemiesStatBaseValueModifierApocalypseEffectDefinition.Value);
					}
					else
					{
						Apocalypse.EnemiesStatsBaseValueModifiers[enemiesStatBaseValueModifierApocalypseEffectDefinition.AffectedEnemies[num2]][enemiesStatBaseValueModifierApocalypseEffectDefinition.Stat] += enemiesStatBaseValueModifierApocalypseEffectDefinition.Value;
					}
				}
			}
		}
	}

	/// <summary>
	/// Tính toán hệ số nhân trọng số (Spawn Weight Multiplier) trong các đợt xuất hiện của từng loại đơn vị quái vật.
	/// </summary>
	private void ComputeEnemiesSpawnWaveWeightMultiplier()
	{
		Apocalypse.EnemiesSpawnWaveWeightMultiplier.Clear();
		for (int num = Apocalypse.AllEffects.Count - 1; num >= 0; num--)
		{
			if (Apocalypse.AllEffects[num] is MultiplyEnemyUnitSpawnWaveWeightApocalypseEffectDefinition multiplyEnemyUnitSpawnWaveWeightApocalypseEffectDefinition)
			{
				for (int num2 = multiplyEnemyUnitSpawnWaveWeightApocalypseEffectDefinition.EnemyUnitIds.Count - 1; num2 >= 0; num2--)
				{
					string key = multiplyEnemyUnitSpawnWaveWeightApocalypseEffectDefinition.EnemyUnitIds[num2];
					if (!Apocalypse.EnemiesSpawnWaveWeightMultiplier.ContainsKey(key))
					{
						Apocalypse.EnemiesSpawnWaveWeightMultiplier.Add(key, multiplyEnemyUnitSpawnWaveWeightApocalypseEffectDefinition.Multiplier);
					}
					else
					{
						Apocalypse.EnemiesSpawnWaveWeightMultiplier[key] = multiplyEnemyUnitSpawnWaveWeightApocalypseEffectDefinition.Multiplier;
					}
				}
			}
		}
	}

	/// <summary>
	/// Tính toán độ lệch tiến trình xuất hiện quái vật (Progression Offset) - đẩy nhanh tiến độ làm quái vật nguy hiểm xuất hiện sớm hơn các đêm dự kiến.
	/// </summary>
	private void ComputeEnemiesProgressionOffset()
	{
		Apocalypse.EnemiesProgressionOffset = 0;
		foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
		{
			if (allEffect is IncreaseEnemiesProgressionOffsetApocalypseEffectDefinition increaseEnemiesProgressionOffsetApocalypseEffectDefinition)
			{
				Apocalypse.EnemiesProgressionOffset += increaseEnemiesProgressionOffsetApocalypseEffectDefinition.Value;
			}
		}
	}

	/// <summary>
	/// Tính toán các chỉ số bổ sung của quái vật theo từng mốc chấn thương (Injury Stages) - ví dụ khi quái mất máu sẽ cuồng bạo hơn.
	/// Duyệt ngược từ cuối danh sách để giữ đúng thứ tự ưu tiên.
	/// </summary>
	private void ComputeEnemiesInjuryStageStatModifiers()
	{
		Apocalypse.EnemiesInjuryStageInjuryStatModifiers.Clear();
		for (int num = Apocalypse.AllEffects.Count - 1; num >= 0; num--)
		{
			if (Apocalypse.AllEffects[num] is ModifyEnemiesInjuryStageApocalypseEffectDefinition modifyEnemiesInjuryStageApocalypseEffectDefinition)
			{
				if (!Apocalypse.EnemiesInjuryStageInjuryStatModifiers.ContainsKey(modifyEnemiesInjuryStageApocalypseEffectDefinition.InjuryStage))
				{
					Apocalypse.EnemiesInjuryStageInjuryStatModifiers.Add(modifyEnemiesInjuryStageApocalypseEffectDefinition.InjuryStage, new List<ModifyEnemiesInjuryStageApocalypseEffectDefinition.ApocalypseInjuryStatModifier>());
				}
				for (int num2 = modifyEnemiesInjuryStageApocalypseEffectDefinition.InjuryStatModifiers.Count - 1; num2 >= 0; num2--)
				{
					Apocalypse.EnemiesInjuryStageInjuryStatModifiers[modifyEnemiesInjuryStageApocalypseEffectDefinition.InjuryStage].Add(modifyEnemiesInjuryStageApocalypseEffectDefinition.InjuryStatModifiers[num2]);
				}
			}
		}
	}

	/// <summary>
	/// Quản lý các hiệu ứng tăng chỉ số quái vật sau một số lượt nhất định trong đêm.
	/// Hủy kích hoạt và gỡ hook (unhook) các hiệu ứng cũ, tạo mới controller cho các hiệu ứng hợp lệ, hook điều kiện kích hoạt/hủy kích hoạt.
	/// </summary>
	private void ComputeEnemiesStatModifierAfterTurn()
	{
		// Dọn dẹp và hủy liên kết các hiệu ứng đang có
		foreach (AddEnemiesStatModifierFromTurnApocalypseEffect addEnemiesStatModifierFromTurnEffect in Apocalypse.AddEnemiesStatModifierFromTurnEffects)
		{
			if (addEnemiesStatModifierFromTurnEffect.IsActive)
			{
				addEnemiesStatModifierFromTurnEffect.StatModifierEffectController.DeActivate(onLoad: false);
			}
			if (addEnemiesStatModifierFromTurnEffect.IsHookedForActivation)
			{
				addEnemiesStatModifierFromTurnEffect.StatModifierEffectController.UnHookActivationConditions();
			}
			if (addEnemiesStatModifierFromTurnEffect.IsHookedForDeactivation)
			{
				addEnemiesStatModifierFromTurnEffect.StatModifierEffectController.UnHookDeactivationConditions();
			}
		}
		Apocalypse.AddEnemiesStatModifierFromTurnEffects.Clear();
		Apocalypse.ActiveStatModifierFromTurn.Clear();
		
		// Khởi tạo các hiệu ứng mới và gắn các hook điều kiện
		for (int num = Apocalypse.AllEffects.Count - 1; num >= 0; num--)
		{
			if (Apocalypse.AllEffects[num] is AddEnemiesStatModifierFromTurnApocalypseEffectDefinition effectDefinition)
			{
				AddEnemiesStatModifierFromTurnApocalypseEffect statModifierEffect = new AddEnemiesStatModifierFromTurnApocalypseEffectController(effectDefinition).StatModifierEffect;
				Apocalypse.AddEnemiesStatModifierFromTurnEffects.Add(statModifierEffect);
				statModifierEffect.StatModifierEffectController.HookActivationConditions();
				statModifierEffect.StatModifierEffectController.HookDeactivationConditions();
				
				// Nếu game đang trong trận đấu, kiểm tra ngay xem đã thỏa mãn điều kiện kích hoạt chưa
				if (ApplicationManager.Application.IsGameState)
				{
					statModifierEffect.StatModifierEffectController.CheckIfCanActivate(onLoad: false);
				}
			}
		}
	}

	#endregion

	#region Specific Effect Computations - Heroes & Units

	/// <summary>
	/// Kiểm tra và thiết lập cờ: Tướng của người chơi có chắn tầm nhìn (Block Line of Sight) của nhau khi nhắm bắn hay không.
	/// </summary>
	private void ComputePlayableUnitBlockLineOfSight()
	{
		Apocalypse.PlayableUnitBlockLineOfSight = false;
		foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
		{
			if (allEffect is PlayableUnitBlockLineOfSightApocalypseEffectDefinition)
			{
				Apocalypse.PlayableUnitBlockLineOfSight = true;
			}
		}
	}

	/// <summary>
	/// Tính toán số lượng tướng khởi đầu bị loại bỏ (ví dụ: bình thường bắt đầu với 3 tướng, debuff này có thể trừ bớt 1).
	/// </summary>
	private void ComputeRemoveStartingPlayableUnit()
	{
		Apocalypse.RemoveStartingPlayableUnitAmount = 0;
		foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
		{
			if (allEffect is RemoveStartingPlayableUnitApocalypseEffectDefinition removeStartingPlayableUnitApocalypseEffectDefinition)
			{
				Apocalypse.RemoveStartingPlayableUnitAmount += Mathf.Abs(removeStartingPlayableUnitApocalypseEffectDefinition.Amount);
			}
		}
	}

	/// <summary>
	/// Tính toán các cờ hạn chế hoặc thay đổi tiến trình nâng cấp kỹ năng của tướng (Skill Progression Flags).
	/// </summary>
	private void ComputeSkillProgressionFlags()
	{
		Apocalypse.SkillProgressionFlags.Clear();
		foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
		{
			if (allEffect is AddSkillProgressionFlagApocalypseEffectDefinition addSkillProgressionFlagApocalypseEffectDefinition && !Apocalypse.SkillProgressionFlags.Contains(addSkillProgressionFlagApocalypseEffectDefinition.Flag))
			{
				Apocalypse.SkillProgressionFlags.Add(addSkillProgressionFlagApocalypseEffectDefinition.Flag);
			}
		}
	}

	/// <summary>
	/// Ghi đè cấp độ khởi điểm của tướng khi chiêu mộ tại từng ô cụ thể trong Nhà trọ (Inn Slots).
	/// </summary>
	private void ComputeInnSlotsRecruitmentLevel()
	{
		Apocalypse.InnSlotsOverridenRecruitmentLevelId.Clear();
		foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
		{
			if (allEffect is ModifyInnSlotRecruitmentLevelApocalypseEffectDefinition modifyInnSlotRecruitmentLevelApocalypseEffectDefinition)
			{
				if (Apocalypse.InnSlotsOverridenRecruitmentLevelId.ContainsKey(modifyInnSlotRecruitmentLevelApocalypseEffectDefinition.SlotIndex))
				{
					Apocalypse.InnSlotsOverridenRecruitmentLevelId[modifyInnSlotRecruitmentLevelApocalypseEffectDefinition.SlotIndex] = modifyInnSlotRecruitmentLevelApocalypseEffectDefinition.LevelId;
				}
				else
				{
					Apocalypse.InnSlotsOverridenRecruitmentLevelId.Add(modifyInnSlotRecruitmentLevelApocalypseEffectDefinition.SlotIndex, modifyInnSlotRecruitmentLevelApocalypseEffectDefinition.LevelId);
				}
			}
		}
	}

	#endregion

	#region Specific Effect Computations - Environment, Items & Panic

	/// <summary>
	/// Tính toán các cờ thuộc tính (Affixes Flags) áp dụng khi sinh trang bị/vật phẩm (ví dụ: dòng chỉ số bị nguyền/tiêu cực).
	/// </summary>
	private void ComputeAffixesFlags()
	{
		Apocalypse.AffixesFlags.Clear();
		foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
		{
			if (allEffect is AddAffixFlagApocalypseEffectDefinition addAffixFlagApocalypseEffectDefinition && !Apocalypse.AffixesFlags.Contains(addAffixFlagApocalypseEffectDefinition.Flag))
			{
				Apocalypse.AffixesFlags.Add(addAffixFlagApocalypseEffectDefinition.Flag);
			}
		}
	}

	/// <summary>
	/// Tính toán hệ số nhân số lượng cọc tạo sương mù (Fog Spawners) sinh ra trên bản đồ.
	/// </summary>
	private void ComputeFogSpawnersMultiplier()
	{
		Apocalypse.FogSpawnersApocalypseMultiplier = 0f;
		foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
		{
			if (allEffect is GenerateFogSpawnersApocalypseEffectDefinition generateFogSpawnersApocalypseEffectDefinition)
			{
				Apocalypse.FogSpawnersApocalypseMultiplier = generateFogSpawnersApocalypseEffectDefinition.Multiplier;
			}
		}
	}

	/// <summary>
	/// Tính toán hệ số nhân điểm hoảng loạn (Panic Gain Multiplier) nhận thêm mỗi đêm. Mặc định là 1 nếu không có debuff.
	/// </summary>
	private void ComputePanicGainMultiplier()
	{
		bool flag = false;
		Apocalypse.PanicGainMultiplier = 0f;
		foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
		{
			if (allEffect is MultiplyPanicGainApocalypseEffectDefinition multiplyPanicGainApocalypseEffectDefinition)
			{
				flag = true;
				Apocalypse.PanicGainMultiplier += multiplyPanicGainApocalypseEffectDefinition.Multiplier;
			}
		}
		if (!flag)
		{
			Apocalypse.PanicGainMultiplier = 1f;
		}
	}

	#endregion
}
