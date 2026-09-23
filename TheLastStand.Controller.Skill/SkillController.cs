using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Definition.Skill;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Skill;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Serialization;
using UnityEngine;

namespace TheLastStand.Controller.Skill;

/// <summary>
/// Bộ điều khiển cho một thực thể kỹ năng cụ thể (Skill Controller).
/// <para>Chịu trách nhiệm xử lý toàn bộ logic nội tại của một chiêu thức:</para>
/// <list type="bullet">
///   <item><description>Kiểm tra điều kiện thi triển (tài nguyên AP, MP, Mana, Health, trạng thái Stun).</description></item>
///   <item><description>Kiểm tra điều kiện ngữ cảnh môi trường (đứng trong tháp canh, cạnh công trình, gần đồng đội...).</description></item>
///   <item><description>Kiểm tra giai đoạn/phase hợp lệ (ban ngày, ban đêm, triển khai đội hình...).</description></item>
///   <item><description>Tính toán tầm thi triển tối đa (ComputeMaxRange) có xét các chỉ số bonus của Unit.</description></item>
///   <item><description>Tính toán và lọc danh sách mục tiêu hợp lệ trên bản đồ (ComputeTargetsAndValidity, TryAddTarget).</description></item>
///   <item><description>Khởi tạo bộ xử lý hành vi kỹ năng tương ứng (Attack, Buff/Debuff, Xây dựng, Tiếp tế, Vào/Ra tháp...).</description></item>
/// </list>
/// </summary>
public class SkillController
{
	/// <summary>
	/// Tham chiếu tới dữ liệu Model của kỹ năng mà Controller này điều khiển.
	/// </summary>
	public TheLastStand.Model.Skill.Skill Skill { get; private set; }

	/// <summary>
	/// Khởi tạo SkillController từ dữ liệu đã lưu trữ/tuần tự hóa (Save/Load).
	/// </summary>
	/// <param name="container">Dữ liệu tuần tự hóa của kỹ năng.</param>
	/// <param name="skillContainer">Đối tượng chứa kỹ năng (Tướng, Trang bị, Công trình...).</param>
	public SkillController(SerializedSkill container, ISkillContainer skillContainer)
	{
		Skill = new TheLastStand.Model.Skill.Skill(container, this, skillContainer);
		CreateSkillEffects();
	}

	/// <summary>
	/// Khởi tạo SkillController từ bản thiết kế kỹ năng (SkillDefinition).
	/// </summary>
	/// <param name="skillDefinition">Bản thiết kế cấu hình kỹ năng từ XML.</param>
	/// <param name="skillContainer">Đối tượng sở hữu kỹ năng.</param>
	/// <param name="overallUsesCount">Số lần dùng tối đa cả trận (-1 nếu lấy theo definition).</param>
	/// <param name="usesPerTurnCount">Số lần dùng tối đa mỗi turn (-1 nếu lấy theo definition).</param>
	public SkillController(SkillDefinition skillDefinition, ISkillContainer skillContainer, int overallUsesCount = -1, int usesPerTurnCount = -1)
	{
		Skill = new TheLastStand.Model.Skill.Skill(skillDefinition, this, skillContainer, overallUsesCount, usesPerTurnCount);
		CreateSkillEffects();
	}

	/// <summary>
	/// Kiểm tra xem nhân vật có đủ điều kiện tài nguyên để thực thi kỹ năng hay không.
	/// </summary>
	/// <param name="actionPoints">Điểm hành động (AP) hiện có (-1f nếu bỏ qua).</param>
	/// <param name="movePoints">Điểm di chuyển (MP) hiện có (-1f nếu bỏ qua).</param>
	/// <param name="mana">Lượng Mana hiện có (-1f nếu bỏ qua).</param>
	/// <param name="health">Lượng Máu hiện có (-1f nếu bỏ qua). Lưu ý: HealthCost phải nhỏ hơn Health để tránh tự sát.</param>
	/// <param name="isStun">Trạng thái bị choáng của nhân vật (nếu true thì không thể dùng chiêu).</param>
	/// <returns>True nếu thỏa mãn mọi chi phí và pha chơi cho phép.</returns>
	public bool CanExecuteSkill(float actionPoints, float movePoints, float mana, float health, bool isStun)
	{
		if (!isStun && Skill.UsesPerTurnRemaining != 0 && Skill.OverallUsesRemaining != 0 && ((float)Skill.ActionPointsCost <= actionPoints || actionPoints == -1f) && ((float)Skill.MovePointsCost <= movePoints || movePoints == -1f) && ((float)Skill.HealthCost < health || health == -1f) && ((float)Skill.ManaCost <= mana || mana == -1f))
		{
			return Skill.SkillController.CheckPhaseAllowed();
		}
		return false;
	}

	/// <summary>
	/// Kiểm tra tổng thể các điều kiện ngữ cảnh, pha hiển thị và trạng thái khóa của kỹ năng.
	/// </summary>
	/// <param name="playableUnit">Tướng sở hữu kỹ năng.</param>
	/// <param name="dontCheckPhase">Nếu true, bỏ qua việc kiểm tra giai đoạn ngày/đêm.</param>
	/// <returns>True nếu kỹ năng đủ điều kiện sẵn sàng sử dụng.</returns>
	public bool CheckConditions(PlayableUnit playableUnit, bool dontCheckPhase = false)
	{
		if (CheckContextualConditions(playableUnit) && (dontCheckPhase || CheckPhaseDisplay()))
		{
			return Skill.PerkLocksBuffer <= 0;
		}
		return false;
	}

	/// <summary>
	/// Kiểm tra các điều kiện theo ngữ cảnh chiến trường (Contextual Conditions) được cấu hình trong XML.
	/// <para>Bao gồm: kiểm tra công trình tồn tại, đang đứng trong tháp canh, đứng cạnh công trình, đứng gần đồng đội...</para>
	/// </summary>
	/// <param name="playableUnit">Tướng đang kiểm tra.</param>
	public bool CheckContextualConditions(PlayableUnit playableUnit)
	{
		bool flag = true;
		foreach (SkillConditionDefinition contextualCondition in Skill.SkillDefinition.ContextualConditions)
		{
			switch (contextualCondition.Name)
			{
			case "BuildingExist":
			{
				// Kiểm tra trên bản đồ có công trình chỉ định hay không
				BuildingExistConditionDefinition buildingExistConditionDefinition = contextualCondition as BuildingExistConditionDefinition;
				foreach (TheLastStand.Model.Building.Building building in TPSingleton<BuildingManager>.Instance.Buildings)
				{
					if (building.BuildingDefinition.Id == buildingExistConditionDefinition.BuildingDefinitionId)
					{
						flag = true;
						break;
					}
				}
				break;
			}
			case "InWatchtower":
				// Kiểm tra tướng có đang đứng trên tháp canh (Watchtower) hay không
				flag = playableUnit.OriginTile.Building != null && playableUnit.OriginTile.Building.IsWatchtower;
				break;
			case "NextToBuilding":
			{
				// Kiểm tra tướng có đang đứng liền kề (khoảng cách Manhattan = 1 ô) với công trình chỉ định hay không
				NextToBuildingConditionDefinition nextToBuildingConditionDefinition = contextualCondition as NextToBuildingConditionDefinition;
				flag = false;
				for (int i = -1; i <= 1; i++)
				{
					for (int j = -1; j <= 1; j++)
					{
						if (Mathf.Abs(i) + Mathf.Abs(j) == 1)
						{
							Tile tile2 = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(playableUnit.OriginTile.Position.x + i, playableUnit.OriginTile.Position.y + j);
							if (tile2 != null && !tile2.HasFog && tile2.Building != null && tile2.Building.BuildingDefinition.Id == nextToBuildingConditionDefinition.BuildingDefinitionId)
							{
								flag = true;
							}
						}
					}
					if (flag)
					{
						break;
					}
				}
				break;
			}
			case "InPlayableUnitRange":
			{
				// Kiểm tra có đồng đội nào khác nằm trong phạm vi chỉ định hay không
				InPlayableUnitRangConditionDefinition inPlayableUnitRangConditionDefinition = contextualCondition as InPlayableUnitRangConditionDefinition;
				flag = playableUnit.OccupiedTiles.GetTilesInRange(inPlayableUnitRangConditionDefinition.MaxRange, 1).Any((Tile tile3) => tile3.Unit is PlayableUnit);
				break;
			}
			case "NotInBuilding":
				// Kiểm tra tướng không đứng trong bất kỳ công trình nào
				flag = playableUnit.OriginTile.Building == null;
				break;
			case "OntoBuilding":
			{
				// Kiểm tra tướng đang đứng ngay trên ô của công trình cụ thể
				OntoBuildingConditionDefinition ontoBuildingConditionDefinition = contextualCondition as OntoBuildingConditionDefinition;
				flag = false;
				Tile tile = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(playableUnit.OriginTile.Position.x, playableUnit.OriginTile.Position.y);
				if (!tile.HasFog && tile.Building != null && tile.Building.BuildingDefinition.Id == ontoBuildingConditionDefinition.BuildingDefinitionId)
				{
					flag = true;
				}
				break;
			}
			}
			if (!flag)
			{
				break;
			}
		}
		// Kết hợp với kiểm tra xem kỹ năng có bị khóa bởi hệ thống Perk hay không
		return flag & !playableUnit.PerkTree.UnitPerkTreeController.IsSkillLockedByPerks(Skill);
	}

	/// <summary>
	/// Kiểm tra xem kỹ năng có được phép kích hoạt trong pha hiện tại của game hay không.
	/// </summary>
	public bool CheckPhaseAllowed()
	{
		return CheckPhaseFlags(Skill.SkillDefinition.AllowDuringPhase);
	}

	/// <summary>
	/// Kiểm tra xem kỹ năng có được phép hiển thị lên thanh kỹ năng UI trong pha hiện tại hay không.
	/// </summary>
	public bool CheckPhaseDisplay()
	{
		return CheckPhaseFlags(Skill.SkillDefinition.DisplayDuringPhase);
	}

	/// <summary>
	/// Tính toán tầm thi triển xa nhất của kỹ năng (Range.y), có tính đến các chỉ số tăng tầm đánh của Unit.
	/// </summary>
	public int ComputeMaxRange()
	{
		int result = Skill.SkillDefinition.Range.y;
		if (Skill.SkillAction.SkillActionExecution.Caster is TheLastStand.Model.Unit.Unit unit)
		{
			result = unit.UnitController.GetModifiedMaxRange(Skill);
		}
		return result;
	}

	/// <summary>
	/// Quét và tính toán toàn bộ các mục tiêu hợp lệ của kỹ năng trên bản đồ dựa trên tầm đánh và tầm nhìn.
	/// </summary>
	/// <param name="skillCaster">Đối tượng thi triển kỹ năng.</param>
	/// <param name="shouldUpdateView">Nếu true, cập nhật giao diện hiển thị đánh dấu mục tiêu (Targeting Mark).</param>
	/// <returns>True nếu tìm thấy ít nhất 1 mục tiêu hợp lệ.</returns>
	public bool ComputeTargetsAndValidity(ISkillCaster skillCaster, bool shouldUpdateView = false)
	{
		bool result = false;
		if (Skill.Targets == null)
		{
			Skill.Targets = new List<ITileObject>();
		}
		else
		{
			Skill.Targets.Clear();
		}
		if (Skill.SkillDefinition.ValidTargets == null)
		{
			return true;
		}

		// Xử lý kỹ năng có tầm đánh vô hạn (toàn bản đồ)
		if (Skill.SkillDefinition.InfiniteRange)
		{
			Tile[] tiles = TPSingleton<TileMapManager>.Instance.TileMap.Tiles;
			foreach (Tile tile in tiles)
			{
				if (tile.CanAffectThroughFog(skillCaster) && TryAddTarget(tile))
				{
					result = true;
				}
			}
		}
		else
		{
			// Xử lý kỹ năng có phạm vi giới hạn: chỉ quét các ô trong InRangeTiles và có Line of Sight
			foreach (KeyValuePair<Tile, TilesInRangeInfos.TileDisplayInfos> item in Skill.SkillAction.SkillActionExecution.InRangeTiles.Range)
			{
				if (item.Key != null && item.Value.HasLineOfSight && TryAddTarget(item.Key))
				{
					result = true;
				}
			}
		}

		// Cập nhật hiển thị vòng nhắm mục tiêu nếu có yêu cầu
		if (shouldUpdateView)
		{
			foreach (ITileObject target in Skill.Targets)
			{
				if (RequiresTargetValidationFeedback(target.OriginTile))
				{
					target.TileObjectView.ToggleSkillTargeting(display: true);
				}
			}
		}
		return result;
	}

	/// <summary>
	/// Kiểm tra xem có ít nhất một ô trong danh sách điểm đến nằm trong tầm thi triển của kỹ năng hay không.
	/// </summary>
	public bool HasAtLeastOneTileInRange(Tile sourceTile, Tile[] destinationTiles)
	{
		foreach (Tile targetTile in destinationTiles)
		{
			if (Skill.SkillAction.SkillActionExecution.InRangeTiles.IsInRange(targetTile))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Kiểm tra ô mục tiêu có thỏa mãn các ràng buộc hợp lệ của kỹ năng hay không
	/// (ô trống, ô có thể đi, ô địa hình cản trở, tướng đồng minh, quái vật hay công trình).
	/// </summary>
	/// <param name="targetTile">Ô mục tiêu đang xét.</param>
	/// <param name="isSkillTargetTile">Nếu true, kiểm tra xem mục tiêu đã có trong danh sách Skill.Targets chưa.</param>
	public bool IsValidatingTargetingConstraints(Tile targetTile, bool isSkillTargetTile = true)
	{
		if (Skill.SkillDefinition.ValidTargets == null)
		{
			return true;
		}
		if (isSkillTargetTile && Skill.Targets != null && Skill.Targets.Count > 0)
		{
			if (!Skill.Targets.Contains(targetTile.Building) && !Skill.Targets.Contains(targetTile.Unit))
			{
				return Skill.Targets.Contains(targetTile);
			}
			return true;
		}
		if (Skill.SkillDefinition.ValidTargets.EmptyTiles && targetTile.IsCrossable && targetTile.IsEmpty())
		{
			return true;
		}
		if ((Skill.SkillDefinition.ValidTargets.WalkableTiles || (Skill.SkillDefinition.ValidTargets.WalkableCityTiles && targetTile.IsCityTile)) && targetTile.IsCrossable && targetTile.Unit == null && Skill.SkillAction.SkillActionExecution.Caster is TheLastStand.Model.Unit.Unit unit && unit.CanStopOn(targetTile))
		{
			return true;
		}
		if (Skill.SkillDefinition.ValidTargets.UncrossableGrounds && !targetTile.IsCrossable && targetTile.Unit == null)
		{
			return true;
		}
		if (Skill.SkillDefinition.ValidTargets.PlayableUnits && targetTile.Unit is PlayableUnit)
		{
			return true;
		}
		if (Skill.SkillDefinition.ValidTargets.EnemyUnits && targetTile.Unit is EnemyUnit)
		{
			return true;
		}
		if (targetTile.Building != null && Skill.SkillDefinition.ValidTargets.Buildings.TryGetValue(targetTile.Building.Id, out var _))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Thay đổi số lần sử dụng tối đa của kỹ năng trong toàn bộ trận đánh.
	/// </summary>
	public void ModifyOverallUses(int newOverallUsesValue)
	{
		Skill.OverallUses = newOverallUsesValue;
	}

	/// <summary>
	/// Kiểm tra xem ô mục tiêu có cần hiển thị hiệu ứng/phản hồi nhắm bắn đặc biệt hay không
	/// (ví dụ: công trình cần sửa chữa, tướng cần tiếp tế...).
	/// </summary>
	public bool RequiresTargetValidationFeedback(Tile targetTile)
	{
		if (Skill.SkillDefinition.ValidTargets == null)
		{
			return false;
		}
		if ((targetTile.Unit is PlayableUnit && Skill.SkillDefinition.ValidTargets.PlayableUnits) || (targetTile.Unit is EnemyUnit && Skill.SkillDefinition.ValidTargets.EnemyUnits))
		{
			if (Skill.SkillAction is ResupplySkillAction resupplySkillAction && !resupplySkillAction.CheckUnitNeedResupply(targetTile.Unit))
			{
				return false;
			}
			return true;
		}
		if (targetTile.Building != null && Skill.SkillDefinition.ValidTargets.Buildings.Count > 0)
		{
			ValidTargets.Constraints value;
			bool result = (targetTile.Building.BlueprintModule.IsIndestructible || !targetTile.Building.DamageableModule.IsDead) && Skill.SkillDefinition.ValidTargets.Buildings.TryGetValue(targetTile.Building.BuildingDefinition.Id, out value) && (!value.MustBeEmpty || targetTile.Unit == null) && (!value.NeedRepair || (Skill.SkillAction is ResupplySkillAction resupplySkillAction2 && resupplySkillAction2.CheckBuildingNeedRepair(targetTile.Building)));
			TileObjectSelectionManager.E_Orientation specificOrientation = Skill.TileDependantOrientation(targetTile);
			if (!Skill.SkillAction.SkillActionExecution.SkillExecutionController.IsManeuverValid(targetTile, specificOrientation))
			{
				return false;
			}
			return result;
		}
		if (targetTile.Unit == null && targetTile.Building == null && Skill.SkillDefinition.ValidTargets.EmptyTiles)
		{
			return false;
		}
		if (Skill.SkillDefinition.ValidTargets.EmptyTiles && (targetTile.Unit != null || targetTile.Building != null))
		{
			return true;
		}
		if (Skill.SkillDefinition.ValidTargets.UncrossableGrounds && targetTile.IsEmpty() && !targetTile.IsCrossable)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Thiết lập kỹ năng liên kết cùng chia sẻ số lượt dùng (ví dụ: vũ khí 2 tay chia sẻ lượt đánh giữa các skill).
	/// </summary>
	public void SetLinkedSkillForUses(TheLastStand.Model.Skill.Skill linkedSkill)
	{
		Skill.LinkedSkillForUses = linkedSkill;
	}

	/// <summary>
	/// Ghi đè chủ thể thi triển kỹ năng (Overriden Owner).
	/// </summary>
	public void SetOverridenOwner(ISkillCaster overridenOwner)
	{
		Skill.OverridenOwner = overridenOwner;
	}

	/// <summary>
	/// Kiểm tra cờ pha (Phase Flags) so sánh với chu kỳ thực tế của game (Night, Production, Deployment).
	/// </summary>
	private bool CheckPhaseFlags(SkillDefinition.E_Phase flags)
	{
		if (SkillManager.DebugSkillsAllowAllPhases)
		{
			return true;
		}
		if (!flags.HasFlag(SkillDefinition.E_Phase.All) && (!flags.HasFlag(SkillDefinition.E_Phase.Night) || TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night) && (!flags.HasFlag(SkillDefinition.E_Phase.Production) || TPSingleton<GameManager>.Instance.Game.DayTurn != Game.E_DayTurn.Production))
		{
			if (flags.HasFlag(SkillDefinition.E_Phase.Deployment))
			{
				return TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Deployment;
			}
			return false;
		}
		return true;
	}

	/// <summary>
	/// Khởi tạo bộ xử lý hành vi (SkillActionController) tương ứng theo định nghĩa SkillActionDefinition từ XML.
	/// </summary>
	private void CreateSkillEffects()
	{
		if (Skill.SkillDefinition.SkillActionDefinition is AttackSkillActionDefinition)
		{
			Skill.SkillAction = new AttackSkillActionController(Skill.SkillDefinition.SkillActionDefinition, Skill).SkillAction;
		}
		else if (Skill.SkillDefinition.SkillActionDefinition is GenericSkillActionDefinition)
		{
			Skill.SkillAction = new GenericSkillActionController(Skill.SkillDefinition.SkillActionDefinition, Skill).SkillAction;
		}
		else if (Skill.SkillDefinition.SkillActionDefinition is GoIntoWatchtowerSkillActionDefinition)
		{
			Skill.SkillAction = new GoIntoWatchtowerSkillActionController(Skill.SkillDefinition.SkillActionDefinition, Skill).SkillAction;
		}
		else if (Skill.SkillDefinition.SkillActionDefinition is QuitWatchtowerSkillActionDefinition)
		{
			Skill.SkillAction = new QuitWatchtowerSkillActionController(Skill.SkillDefinition.SkillActionDefinition, Skill).SkillAction;
		}
		else if (Skill.SkillDefinition.SkillActionDefinition is SkipTurnSkillActionDefinition)
		{
			Skill.SkillAction = new SkipTurnSkillActionController(Skill.SkillDefinition.SkillActionDefinition, Skill).SkillAction;
		}
		else if (Skill.SkillDefinition.SkillActionDefinition is SpawnSkillActionDefinition)
		{
			Skill.SkillAction = new SpawnSkillActionController(Skill.SkillDefinition.SkillActionDefinition, Skill).SkillAction;
		}
		else if (Skill.SkillDefinition.SkillActionDefinition is BuildSkillActionDefinition)
		{
			Skill.SkillAction = new BuildSkillActionController(Skill.SkillDefinition.SkillActionDefinition, Skill).SkillAction;
		}
		else if (Skill.SkillDefinition.SkillActionDefinition is ResupplySkillActionDefinition)
		{
			Skill.SkillAction = new ResupplySkillActionController(Skill.SkillDefinition.SkillActionDefinition, Skill).SkillAction;
		}
		else
		{
			TPSingleton<SkillManager>.Instance.LogError("Unknown skill " + Skill.SkillDefinition.SkillActionDefinition.GetType().Name, CLogLevel.MAJOR);
		}
	}

	/// <summary>
	/// Kiểm tra và thêm ô mục tiêu vào danh sách hợp lệ Skill.Targets nếu thỏa mãn mọi tiêu chuẩn.
	/// </summary>
	private bool TryAddTarget(Tile tile)
	{
		TileObjectSelectionManager.E_Orientation specificOrientation = Skill.TileDependantOrientation(tile);
		// Kiểm tra tính hợp lệ của kỹ năng cơ động/lướt (Maneuver)
		if (!Skill.SkillAction.SkillActionExecution.SkillExecutionController.IsManeuverValid(tile, specificOrientation))
		{
			return false;
		}

		// Xử lý khi ô mục tiêu là ô trống (Empty Tile)
		if (tile.IsEmpty())
		{
			if (Skill.SkillDefinition.ValidTargets.EmptyTiles || ((Skill.SkillDefinition.ValidTargets.WalkableTiles || (Skill.SkillDefinition.ValidTargets.WalkableCityTiles && tile.IsCityTile)) && (!(Skill.Owner is TheLastStand.Model.Unit.Unit unit) || unit.CanStopOn(tile))))
			{
				Skill.Targets.Add(tile);
				return true;
			}
			if (Skill.SkillDefinition.ValidTargets.UncrossableGrounds && !tile.IsCrossable)
			{
				Skill.Targets.Add(tile);
				return true;
			}
		}
		else
		{
			// Xử lý khi mục tiêu là Công trình (Building)
			if (tile.Building != null && (tile.Building.BlueprintModule.IsIndestructible || !tile.Building.DamageableModule.IsDead) && !Skill.Targets.Contains(tile.Building) && Skill.SkillDefinition.ValidTargets != null)
			{
				ResupplySkillAction resupplySkillAction = Skill.SkillAction as ResupplySkillAction;
				if (Skill.SkillDefinition.ValidTargets.Buildings.TryGetValue(tile.Building.BuildingDefinition.Id, out var value) && (!value.MustBeEmpty || tile.Unit == null) && (!value.NeedRepair || (resupplySkillAction != null && resupplySkillAction.CheckBuildingNeedRepair(tile.Building))))
				{
					List<ITileObject> targets = Skill.Targets;
					ITileObject item;
					if (!(Skill.SkillAction.SkillActionExecution.Caster is PlayableUnit))
					{
						ITileObject building = tile.Building;
						item = building;
					}
					else
					{
						ITileObject building = tile;
						item = building;
					}
					targets.Add(item);
					return true;
				}
				if ((Skill.SkillDefinition.ValidTargets.WalkableTiles || (Skill.SkillDefinition.ValidTargets.WalkableCityTiles && tile.IsCityTile)) && (!(Skill.Owner is TheLastStand.Model.Unit.Unit unit2) || unit2.CanStopOn(tile)))
				{
					Skill.Targets.Add(tile);
					return true;
				}
			}

			// Xử lý khi mục tiêu là Đơn vị nhân vật (PlayableUnit hoặc EnemyUnit)
			if (tile.Unit != null && !Skill.Targets.Contains(tile.Unit) && Skill.SkillDefinition.ValidTargets != null)
			{
				// Kiểm tra điều kiện giai đoạn thương tật tối thiểu (MinTargetInjuryStage)
				if (((tile.Unit is PlayableUnit && Skill.SkillDefinition.ValidTargets.PlayableUnits) || (tile.Unit is EnemyUnit && Skill.SkillDefinition.ValidTargets.EnemyUnits)) && Skill.SkillDefinition.ContextualConditions.Find((SkillConditionDefinition o) => o.Name == "MinTargetInjuryStage") is MinTargetInjuryStageConditionDefinition minTargetInjuryStageConditionDefinition)
				{
					if (tile.Unit.InjuryStage < minTargetInjuryStageConditionDefinition.RequiredInjuryStage.EvalToInt())
					{
						return false;
					}
					Skill.Targets.Add(tile.Unit);
					return true;
				}

				// Kiểm tra kỹ năng tiếp tế cho tướng đồng minh
				ResupplySkillAction resupplySkillAction2 = Skill.SkillAction as ResupplySkillAction;
				if (tile.Unit is PlayableUnit && Skill.SkillDefinition.ValidTargets.PlayableUnits && (resupplySkillAction2 == null || resupplySkillAction2.CheckUnitNeedResupply(tile.Unit)))
				{
					Skill.Targets.Add(tile.Unit);
					return true;
				}

				// Kiểm tra tấn công kẻ địch (kèm điều kiện ngưỡng máu tối đa MaxTargetHealthLeft và tính bất tử IsInvulnerable)
				if (tile.Unit is EnemyUnit && Skill.SkillDefinition.ValidTargets.EnemyUnits && Skill.Owner is PlayableUnit context)
				{
					SkillConditionDefinition skillConditionDefinition = Skill.SkillDefinition.ContextualConditions.Find((SkillConditionDefinition o) => o.Name == "MaxTargetHealthLeft");
					if (skillConditionDefinition != null && (tile.Unit.Health == 0f || tile.Unit.Health > ((MaxTargetHealthLeftConditionDefinition)skillConditionDefinition).HealthThreshold.EvalToFloat(context) || (tile.Unit is EnemyUnit enemyUnit && enemyUnit.EnemyUnitTemplateDefinition.IsInvulnerable)))
					{
						return false;
					}
					Skill.Targets.Add(tile.Unit);
					return true;
				}
			}
		}
		return false;
	}
}
