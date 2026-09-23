using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Unit.Enemy;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;
using TheLastStand.View.TileMap;

namespace TheLastStand.Controller.Building.Module;

public class BattleModuleController : BuildingModuleController, IBehaviorController, ISkillCasterController
{
	#region Properties & Fields
	/// <summary>
	/// Model dữ liệu chiến đấu của công trình (chứa danh sách skill, goals, số lượt bắn còn lại).
	/// </summary>
	public BattleModule BattleModule { get; }

	/// <summary>
	/// Thực thể thi triển kỹ năng của công trình (Cast skill caster).
	/// </summary>
	public ISkillCaster SkillCaster => BattleModule;
	#endregion

	#region Initialization & Factory
	/// <summary>
	/// Khởi tạo Controller chiến đấu, gán Model và ghi nhận giờ sinh ra trên bản đồ.
	/// </summary>
	public BattleModuleController(BuildingController buildingControllerParent, BattleModuleDefinition battleModuleDefinition)
		: base(buildingControllerParent, battleModuleDefinition)
	{
		BattleModule = base.BuildingModule as BattleModule;
		SetSpawnedHour();
	}

	/// <summary>
	/// Factory method khởi tạo Model BattleModule gắn liền với Building cha.
	/// </summary>
	protected override BuildingModule CreateModel(TheLastStand.Model.Building.Building building, BuildingModuleDefinition buildingModuleDefinition)
	{
		return new BattleModule(building, buildingModuleDefinition as BattleModuleDefinition, this);
	}

	/// <summary>
	/// Ghi nhận giờ đêm mà công trình bắt đầu tham chiến để tính toán điều kiện lượt theo thời gian.
	/// </summary>
	public void SetSpawnedHour(int spawnedHour = -1)
	{
		if (spawnedHour == -1)
		{
			if (TPSingleton<GameManager>.Instance.Game.NightTurn != Game.E_NightTurn.Undefined)
			{
				BattleModule.SpawnedHour = TPSingleton<GameManager>.Instance.Game.CurrentNightHour;
			}
			else
			{
				BattleModule.SpawnedHour = 0;
			}
		}
		else
		{
			BattleModule.SpawnedHour = spawnedHour;
		}
		BattleModule.InterpretedTurnConditionContext = new InterpretedTurnConditionContext(BattleModule.SpawnedHour);
	}
	#endregion

	#region AI Goal Generation & Selection
	/// <summary>
	/// Tạo danh sách các Goal (mục tiêu hành vi) từ định nghĩa BehaviorDefinition của công trình.
	/// </summary>
	public void CreateGoals()
	{
		if (BattleModule?.BehaviourDefinition != null)
		{
			int num = BattleModule.BehaviourDefinition.GoalDefinitions.Length;
			BattleModule.Goals = new Goal[num];
			for (int i = 0; i < num; i++)
			{
				BattleModule.Goals[i] = new GoalController(BattleModule.BehaviourDefinition.GoalDefinitions[i], BattleModule).Goal;
			}
		}
	}

	/// <summary>
	/// Xóa sạch mục tiêu đang nhắm tới khi chuẩn bị lượt mới hoặc reset trạng thái.
	/// </summary>
	public void ClearCurrentGoal()
	{
		BattleModule.Log("Cleared current goal", CLogLevel.DETAILED);
		BattleModule.TargetTile = null;
		BattleModule.CurrentGoals = new ComputedGoal[BattleModule.NumberOfGoalsToCompute];
	}

	/// <summary>
	/// Thuật toán AI: Quét toàn bộ các Goal có thể thực hiện và chọn ra mục tiêu tối ưu nhất
	/// (Kiểm tra tầm bắn, cản đường, và tránh trùng lặp mục tiêu với các tháp phòng thủ khác).
	/// </summary>
	public void ComputeCurrentGoals(Dictionary<IDamageable, GroupTargetingInfo> alreadyTargetedTiles = null)
	{
		ClearCurrentGoal();
		BattleModule.Log($"Computing current goals out of {BattleModule.BattleModuleDefinition.Behavior.GoalDefinitions.Length} possible goals", CLogLevel.DETAILED);
		for (int i = 0; i < BattleModule.NumberOfGoalsToCompute && (!BattleModule.BuildingParent.IsTrap || BattleModule.RemainingTrapCharges > i); i++)
		{
			int j = 0;
			for (int num = BattleModule.BattleModuleDefinition.Behavior.GoalDefinitions.Length; j < num; j++)
			{
				Goal goal = BattleModule.Goals[j];
				if (goal.GoalDefinition.GoalComputingStep.HasFlag(BattleModule.GoalComputingStep))
				{
					goal.Skill.SkillAction.SkillActionExecution.SkillExecutionController.PrepareSkill(BattleModule);
					SkillTargetedTileInfo skillTargetedTileInfo = goal.GoalController.ComputeTarget(alreadyTargetedTiles);
					if (skillTargetedTileInfo != null)
					{
						BattleModule.Log($"Validated {skillTargetedTileInfo.Tile.Position} Orientation: {skillTargetedTileInfo.Orientation} as best goal", CLogLevel.NORMAL, forcePrintInUnity: true);
						BattleModule.CurrentGoals[i] = new ComputedGoal(goal, skillTargetedTileInfo);
						break;
					}
				}
			}
		}
	}

	/// <summary>
	/// Giảm cooldown của các Goal khi bắt đầu lượt mới.
	/// </summary>
	public void DecrementGoalsCooldown()
	{
		Goal[] goals = BattleModule.Goals;
		for (int i = 0; i < goals.Length; i++)
		{
			goals[i].GoalController.StartTurn();
		}
	}
	#endregion

	#region Goal Execution (Tấn công)
	/// <summary>
	/// Thực thi tất cả các Goal đã tính toán được (Bắn toàn bộ loạt đạn đã nhắm).
	/// </summary>
	public void ExecuteAllGoals()
	{
		bool flag = false;
		if (BattleModule.CurrentGoals == null || BattleModule.CurrentGoals.Length == 0)
		{
			return;
		}
		List<ComputedGoal> list = BattleModule.CurrentGoals.ToList();
		if (list.Count == 0)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			ComputedGoal goalToExecute = list[i];
			if (ExecuteGoal(goalToExecute))
			{
				flag = true;
			}
		}
		if (BattleModule.IsDeathRattling && !flag)
		{
			FinalizeDeathRattling(BattleModule);
		}
	}

	/// <summary>
	/// Thực hiện bắn một kỹ năng vào mục tiêu cụ thể:
	/// Kiểm tra lại tầm bắn (Range) và đường đạn (Line of Sight) trước khi kích hoạt ExecuteSkill().
	/// </summary>
	public bool ExecuteGoal(ComputedGoal goalToExecute)
	{
		bool result = false;
		BattleModule.Log($"Executing Goal -> target tile is {goalToExecute.TargetTileInfo}, orientation is {goalToExecute.TargetTileInfo.Orientation}", CLogLevel.DETAILED);
		SkillAction skillAction = goalToExecute.Goal.Skill.SkillAction;
		skillAction.SkillActionExecution.SkillExecutionController.PrepareSkill(BattleModule);
		bool num = goalToExecute.Goal.Skill.SkillAction.SkillActionExecution.InRangeTiles.IsInRange(goalToExecute.TargetTileInfo.Tile);
		bool flag = goalToExecute.Goal.Skill.SkillAction.HasEffect("IgnoreLineOfSight") || goalToExecute.Goal.Skill.SkillAction.SkillActionExecution.InRangeTiles.IsInLineOfSight(goalToExecute.TargetTileInfo.Tile);
		if (num && flag)
		{
			skillAction.SkillActionExecution.TargetTiles.Add(goalToExecute.TargetTileInfo);
			skillAction.SkillActionExecution.SkillExecutionController.ExecuteSkill();
			result = true;
		}
		BattleModule.TargetTile = null;
		return result;
	}
	#endregion

	#region Death Rattling (Hiệu ứng Trăn trối)
	/// <summary>
	/// Hoàn tất chuỗi trăn trối và gỡ bỏ công trình khỏi danh sách chờ của BuildingManager.
	/// </summary>
	public static void FinalizeDeathRattling(BattleModule battleModule)
	{
		if (battleModule.IsDeathRattling)
		{
			battleModule.IsDeathRattling = false;
			TPSingleton<BuildingManager>.Instance.BuildingsDeathRattling.Remove(battleModule);
		}
	}

	/// <summary>
	/// Kích hoạt chuỗi hành vi trăn trối khi công trình bị quái đập vỡ (vd: Bẫy tự nổ hoặc Tháp tự sát).
	/// </summary>
	public void ExecuteDeathRattle()
	{
		if (BattleModule.IsDeathRattling)
		{
			BattleModule battleModule = BattleModule;
			if (battleModule.TargetTile == null)
			{
				Tile tile = (battleModule.TargetTile = BattleModule.OriginTile);
			}
			ExecuteAllGoals();
		}
	}

	/// <summary>
	/// Đăng ký công trình vào danh sách chờ nổ trăn trối của BuildingManager trước khi biến mất.
	/// </summary>
	public void PrepareForDeathRattle()
	{
		if (BattleModule.BehaviourDefinition == null || !BattleModule.ShouldTriggerDeathRattle)
		{
			return;
		}
		BattleModule.GoalComputingStep = IBehaviorModel.E_GoalComputingStep.OnDeath;
		ComputeCurrentGoals();
		if (BattleModule.CurrentGoals[0]?.Goal != null && BattleModule.CurrentGoals[0].TargetTileInfo != null)
		{
			if (!TPSingleton<BuildingManager>.Instance.BuildingsDeathRattling.Contains(BattleModule))
			{
				TPSingleton<BuildingManager>.Instance.BuildingsDeathRattling.Add(BattleModule);
			}
			BattleModule.TargetTile = BattleModule.OriginTile;
			BattleModule.IsDeathRattling = true;
		}
	}
	#endregion

	#region Turn & Cooldown Management
	/// <summary>
	/// Xử lý chuyển lượt: Bắt đầu đêm thì đếm giờ và giảm hồi chiêu, ban ngày thì hồi phục toàn bộ số lượt bắn.
	/// </summary>
	public void StartTurn()
	{
		if (TPSingleton<GameManager>.Instance.Game.CurrentNightHour == 1 && BattleModule.Goals != null)
		{
			SetSpawnedHour();
		}
		switch (TPSingleton<GameManager>.Instance.Game.Cycle)
		{
		case Game.E_Cycle.Night:
			if (TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.EnemyUnits && BattleModule.Goals != null)
			{
				for (int num = BattleModule.Goals.Length - 1; num >= 0; num--)
				{
					BattleModule.Goals[num].GoalController.StartTurn();
				}
			}
			break;
		case Game.E_Cycle.Day:
			if (TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production)
			{
				RefillSkillUsesPerTurn();
				RefillSkillsOverallUses();
			}
			break;
		}
	}

	/// <summary>
	/// Hồi phục số lượt bắn tối đa trong trận cho công trình.
	/// </summary>
	public void RefillSkillsOverallUses()
	{
		if (BattleModule.Skills != null && BattleModule.Skills.Count > 0)
		{
			bool flag = BattleModule.BuildingParent.IsHandledDefense && BattleModule.HasDisabledStateAndZeroRemainingCharges;
			for (int num = BattleModule.Skills.Count - 1; num >= 0; num--)
			{
				BattleModule.Skills[num].OverallUsesRemaining = BattleModule.Skills[num].ComputeTotalUses();
			}
			if (flag)
			{
				RefreshDisplayedBuilding();
			}
		}
	}

	/// <summary>
	/// Hồi phục số lượt bắn cho phép trong mỗi Turn (UsesPerTurn).
	/// </summary>
	public void RefillSkillUsesPerTurn()
	{
		if (BattleModule.Skills == null || BattleModule.Skills.Count <= 0)
		{
			return;
		}
		for (int num = BattleModule.Skills.Count - 1; num >= 0; num--)
		{
			if (BattleModule.Skills[num].SkillDefinition.UsesPerTurnCount != -1)
			{
				BattleModule.Skills[num].SetUsesPerTurnRemaining(BattleModule.Skills[num].SkillDefinition.UsesPerTurnCount);
			}
		}
	}

	/// <summary>
	/// Kích hoạt khi skill kết thúc thi triển: Nếu là tháp thủ công và hết sạch đạn thì đổi Sprite sang dạng Disabled.
	/// </summary>
	public void OnSkillCastEnded(TheLastStand.Model.Skill.Skill skill)
	{
		if (base.BuildingModule.BuildingParent.IsHandledDefense && BattleModule.HasDisabledStateAndZeroRemainingCharges)
		{
			RefreshDisplayedBuilding("_Disabled");
		}
	}

	/// <summary>
	/// Công trình không tốn Mana hay AP cá nhân như Hero nên hàm trả chi phí này để trống.
	/// </summary>
	public void PaySkillCost(TheLastStand.Model.Skill.Skill skill)
	{
	}
	#endregion

	#region Hero Perks & Modifiers Integration
	/// <summary>
	/// Kết nối với các Hero trong đội hình: Lấy toàn bộ các Perk có nội tại tăng sát thương công trình
	/// (ModifyDefensesDamageEffect) áp dụng vào BattleModule của tháp.
	/// </summary>
	public void HookToModifyingDamagePerks()
	{
		if (ApplicationManager.Application.State.GetName() == "LevelEditor" || TPSingleton<PlayableUnitManager>.Instance.PlayableUnits == null)
		{
			return;
		}
		for (int num = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count - 1; num >= 0; num--)
		{
			foreach (KeyValuePair<string, Perk> perk in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[num].Perks)
			{
				if (!perk.Value.Unlocked)
				{
					continue;
				}
				foreach (APerkModule perkModule in perk.Value.PerkModules)
				{
					foreach (APerkEffect perkEffect in perkModule.PerkEffects)
					{
						if (perkEffect is ModifyDefensesDamageEffect item)
						{
							BattleModule.ModifyDefensesDamagePerks.Add(item);
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Loại bỏ các ô nằm trong sương mù tím (Fog) khỏi tầm bắn của công trình.
	/// </summary>
	public void FilterTilesInRange(TilesInRangeInfos tilesInRangeInfos, List<Tile> skillSourceTiles)
	{
		foreach (KeyValuePair<Tile, TilesInRangeInfos.TileDisplayInfos> item in tilesInRangeInfos.Range)
		{
			if (item.Key.HasAnyFog && !skillSourceTiles.Contains(item.Key))
			{
				item.Value.HasLineOfSight = false;
				item.Value.TileColor = TileMapView.SkillHiddenRangeTilesColorInvalidOrientation._Color;
			}
		}
	}

	/// <summary>
	/// Cập nhật hình ảnh hiển thị của công trình trên TileMap (ví dụ: chuyển sang sprite hỏng / hết đạn).
	/// </summary>
	private void RefreshDisplayedBuilding(string suffix = "")
	{
		TheLastStand.Model.Building.Building buildingParent = BattleModule.BuildingParent;
		TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayBuildingInstantly(buildingParent, buildingParent.OriginTile, suffix);
	}
	#endregion
}
