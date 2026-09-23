using System;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Definition.TileMap;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;

namespace TheLastStand.Controller.Skill.SkillAction;

/// <summary>
/// Bộ điều khiển cho hành vi kỹ năng Triệu hồi quái vật (Spawn Skill Action).
/// <para>Chịu trách nhiệm thực thi các kỹ năng gọi quái (thường dùng bởi Boss, quái tinh anh hoặc hiệu ứng môi trường):</para>
/// <list type="bullet">
///   <item><description>Triệu hồi quái vật theo số lượng cố định (EnemiesByAmount).</description></item>
///   <item><description>Triệu hồi quái vật ngẫu nhiên theo bảng trọng số (EnemiesByWeight, RandomEnemies).</description></item>
///   <item><description>Tìm ô đất hợp lệ có gắn cờ TileFlagTag hoặc phá hủy công trình cản trở để sinh quái.</description></item>
///   <item><description>Phân bổ quái triệu hồi theo khu vực (Sector) cho Boss.</description></item>
/// </list>
/// </summary>
public class SpawnSkillActionController : SkillActionController
{
	public SpawnSkillAction SpawnSkillAction => base.SkillAction as SpawnSkillAction;

	public SpawnSkillActionController(SkillActionDefinition skillActionDefinition, TheLastStand.Model.Skill.Skill skill)
	{
		base.SkillAction = new SpawnSkillAction(skillActionDefinition, this, skill);
		base.SkillAction.SkillActionExecution = new SpawnSkillActionExecutionController(base.SkillAction.Skill).SkillActionExecution;
	}

	/// <summary>
	/// Kiểm tra kỹ năng triệu hồi có được phép phá hủy công trình đang chiếm giữ ô đất hay không.
	/// </summary>
	public bool CanDestroyBuilding(TheLastStand.Model.Building.Building building)
	{
		if (building == null)
		{
			return true;
		}
		return SpawnSkillAction.SpawnSkillActionDefinition.BuildingIdsToDestroy.Contains(building.Id);
	}

	/// <summary>
	/// Tính toán trước loại quái vật sẽ được sinh ra theo trọng số.
	/// </summary>
	public void ComputeUnitsToSpawn()
	{
		if (!SpawnSkillAction.ComputedUnitsToSpawn)
		{
			if (!SpawnSkillAction.SpawnSkillActionDefinition.IsByAmount)
			{
				SpawnSkillAction.UnitToSpawnByWeight = ComputeEnemyToSpawnByWeight();
			}
			SpawnSkillAction.ComputedUnitsToSpawn = true;
		}
	}

	public override bool IsBuildingAffected(Tile targetTile)
	{
		return false;
	}

	/// <summary>
	/// Kiểm tra unit trên ô mục tiêu có bị tác động (ví dụ: bị hạ gục ngay bởi hiệu ứng Kill khi quái trồi lên).
	/// </summary>
	public override bool IsUnitAffected(Tile targetTile)
	{
		TheLastStand.Model.Unit.Unit unit = targetTile.Unit;
		KillSkillEffectDefinition effect;
		if (unit != null && !unit.IsDead)
		{
			return SpawnSkillAction.TryGetFirstEffect<KillSkillEffectDefinition>("Kill", out effect);
		}
		return false;
	}

	public override void Reset()
	{
		base.Reset();
		SpawnSkillAction.ComputedUnitsToSpawn = false;
		SpawnSkillAction.UnitToSpawnByWeight = null;
	}

	/// <summary>
	/// Xác thực ô mục tiêu ứng viên có thể dùng để sinh quái vật hay không.
	/// </summary>
	public bool ValidateCandidateTargetTile(Tile candidateTargetTile)
	{
		if (SpawnSkillAction.SpawnSkillActionDefinition.IsByAmount)
		{
			return true;
		}
		if (SpawnSkillAction.UnitToSpawnByWeight != null)
		{
			UnitTemplateDefinition unitTemplateDefinition = EnemyUnitDatabase.EliteEnemyUnitTemplateDefinitions.GetValueOrDefault(SpawnSkillAction.UnitToSpawnByWeight.Item1) ?? EnemyUnitDatabase.EnemyUnitTemplateDefinitions.GetValueOrDefault(SpawnSkillAction.UnitToSpawnByWeight.Item1);
			if (CanDestroyBuilding(candidateTargetTile.Building))
			{
				return unitTemplateDefinition?.CanSpawnOn(candidateTargetTile, isPhaseActor: false, ignoreUnits: false, ignoreBuildings: true) ?? false;
			}
			return false;
		}
		return false;
	}

	/// <summary>
	/// Thực thi hành động triệu hồi quái lên ô mục tiêu.
	/// </summary>
	protected override SkillActionResultDatas ApplyActionOnTile(Tile targetTile, ISkillCaster caster)
	{
		SkillActionResultDatas resultData = new SkillActionResultDatas();
		bool flag = IsUnitAffected(targetTile);
		IsBuildingAffected(targetTile);

		// Nếu kỹ năng có hiệu ứng Kill, tiêu diệt đơn vị hiện tại trên ô trước khi triệu hồi
		if (flag && SpawnSkillAction.TryGetFirstEffect<KillSkillEffectDefinition>("Kill", out var effect))
		{
			ApplySkillEffectKill(caster, targetTile.Unit, effect, resultData);
		}

		// Triệu hồi theo số lượng hoặc theo trọng số
		if (SpawnSkillAction.SpawnSkillActionDefinition.IsByAmount)
		{
			SpawnEnemiesByAmount(caster, ref resultData);
			SpawnRandomEnemiesByAmount(caster, ref resultData);
		}
		else
		{
			SpawnAnEnemyByWeight(targetTile, ref resultData);
		}

		if (caster is BattleModule battleModule && battleModule.BuildingParent.IsCrystal && flag)
		{
			TPSingleton<AchievementManager>.Instance.HandleCrystalCorruptedEnemy();
		}
		return resultData;
	}

	protected override SkillActionResultDatas ApplyActionOnSurroundingTile(Tile targetTile, ISkillCaster caster)
	{
		return new SkillActionResultDatas();
	}

	/// <summary>
	/// Chọn loại quái vật ngẫu nhiên dựa trên bảng trọng số Weight trong cấu hình XML.
	/// </summary>
	private Tuple<string, UnitCreationSettings> ComputeEnemyToSpawnByWeight()
	{
		string item = string.Empty;
		UnitCreationSettings unitCreationSettings = null;
		int max = SpawnSkillAction.SpawnSkillActionDefinition.EnemiesByWeight.Sum((EnemySpawnData enemySpawnData) => enemySpawnData.Weight);
		int randomRange = RandomManager.GetRandomRange(TPSingleton<EnemyUnitManager>.Instance, 0, max);
		int num = 0;
		foreach (EnemySpawnData item2 in SpawnSkillAction.SpawnSkillActionDefinition.EnemiesByWeight)
		{
			if (randomRange >= num && randomRange < item2.Weight + num)
			{
				item = item2.Id;
				unitCreationSettings = item2.UnitCreationSettings;
				break;
			}
			num += item2.Weight;
		}
		return new Tuple<string, UnitCreationSettings>(item, unitCreationSettings ?? new UnitCreationSettings());
	}

	/// <summary>
	/// Tìm ô đất hợp lệ trên bản đồ để sinh quái vật theo cờ TileFlagTag hoặc lấy ô mục tiêu chỉ định.
	/// </summary>
	private Tile GetSpawnTile(EnemySpawnData enemySpawnData, SkillActionResultDatas resultData)
	{
		UnitTemplateDefinition unitTemplateDefinition = EnemyUnitDatabase.EliteEnemyUnitTemplateDefinitions.GetValueOrDefault(enemySpawnData.Id) ?? EnemyUnitDatabase.EnemyUnitTemplateDefinitions[enemySpawnData.Id];
		if (enemySpawnData.TileFlag == TileFlagDefinition.E_TileFlagTag.None)
		{
			if (base.SkillAction.SkillActionExecution.TargetTiles.Count <= 0)
			{
				return null;
			}
			return base.SkillAction.SkillActionExecution.TargetTiles[0].Tile;
		}
		return TileMapManager.GetRandomSpawnableTileWithFlag(enemySpawnData.TileFlag, unitTemplateDefinition, ValidatePredicate);
		
		bool ValidatePredicate(Tile tile)
		{
			if (CanDestroyBuilding(tile.Building))
			{
				return !resultData.UnitsToSpawnTarget.ContainsKey(tile);
			}
			return false;
		}
	}

	/// <summary>
	/// Triệu hồi ngẫu nhiên các loại quái vật theo số lượng cấu hình.
	/// </summary>
	private void SpawnRandomEnemiesByAmount(ISkillCaster caster, ref SkillActionResultDatas resultData)
	{
		int count = SpawnSkillAction.SpawnSkillActionDefinition.RandomEnemies.Count;
		int num = SpawnSkillAction.SpawnSkillActionDefinition.RandomEnemiesAmount.EvalToInt(GameManager.FormulaInterpreterContext);
		if (count == 0 && num > 0)
		{
			TPSingleton<SkillManager>.Instance.LogError("Trying to spawn random enemies by amount with an empty enemies list! Skill Id: " + base.SkillAction.Skill.SkillDefinition.Id);
			return;
		}
		int index = 0;
		for (int i = 0; i < num; i++)
		{
			int num2 = 0;
			for (int j = 0; j < count; j++)
			{
				num2 += SpawnSkillAction.SpawnSkillActionDefinition.RandomEnemies[j].Weight;
			}
			int randomRange = RandomManager.GetRandomRange(TPSingleton<EnemyUnitManager>.Instance, 0, num2);
			int num3 = 0;
			for (int k = 0; k < count; k++)
			{
				if (randomRange >= num3 && randomRange < SpawnSkillAction.SpawnSkillActionDefinition.RandomEnemies[k].Weight + num3)
				{
					index = k;
					break;
				}
				num3 += SpawnSkillAction.SpawnSkillActionDefinition.RandomEnemies[k].Weight;
			}
			EnemySpawnData enemySpawnData = SpawnSkillAction.SpawnSkillActionDefinition.RandomEnemies[index];
			Tile spawnTile = GetSpawnTile(enemySpawnData, resultData);
			SpawnEnemy(enemySpawnData, spawnTile, caster, ref resultData);
		}
	}

	/// <summary>
	/// Triệu hồi các nhóm quái vật cố định theo danh sách EnemiesByAmount.
	/// </summary>
	private void SpawnEnemiesByAmount(ISkillCaster caster, ref SkillActionResultDatas resultData)
	{
		if (SpawnSkillAction.SpawnSkillActionDefinition.EnemiesByAmount.Count == 0)
		{
			if (SpawnSkillAction.SpawnSkillActionDefinition.RandomEnemiesAmount.EvalToInt(GameManager.FormulaInterpreterContext) == 0)
			{
				TPSingleton<SkillManager>.Instance.LogError("Trying to spawn enemies by amount with an empty enemies list and no random enemies to compensate! Skill Id: " + base.SkillAction.Skill.SkillDefinition.Id);
			}
			return;
		}
		for (int i = 0; i < SpawnSkillAction.SpawnSkillActionDefinition.EnemiesByAmount.Count; i++)
		{
			EnemySpawnData enemySpawnData = SpawnSkillAction.SpawnSkillActionDefinition.EnemiesByAmount[i];
			int num = enemySpawnData.Amount.EvalToInt(GameManager.FormulaInterpreterContext);
			for (int j = 0; j < num; j++)
			{
				Tile spawnTile = GetSpawnTile(enemySpawnData, resultData);
				SpawnEnemy(enemySpawnData, spawnTile, caster, ref resultData);
			}
		}
	}

	/// <summary>
	/// Triệu hồi 1 quái vật duy nhất tại ô mục tiêu theo trọng số.
	/// </summary>
	private void SpawnAnEnemyByWeight(Tile targetTile, ref SkillActionResultDatas resultData)
	{
		if (SpawnSkillAction.SpawnSkillActionDefinition.EnemiesByWeight.Count == 0)
		{
			TPSingleton<SkillManager>.Instance.LogError("Trying to spawn enemies by weight with an empty enemies list! Skill Id: " + base.SkillAction.Skill.SkillDefinition.Id);
		}
		else if (targetTile.Unit == null)
		{
			TheLastStand.Model.Building.Building building = targetTile.Building;
			if (building != null && !building.CanSpawnEnemyOnIt)
			{
				BuildingManager.DestroyBuilding(targetTile);
			}
			ComputeUnitsToSpawn();
			resultData.UnitsToSpawnTarget.Add(targetTile, (SpawnSkillAction.UnitToSpawnByWeight.Item1, SpawnSkillAction.UnitToSpawnByWeight.Item2));
		}
	}

	/// <summary>
	/// Khởi tạo quái vật tại ô chỉ định và ghi nhận vào hệ thống quản lý Sector nếu Caster là Boss.
	/// </summary>
	private void SpawnEnemy(EnemySpawnData enemySpawnData, Tile tile, ISkillCaster caster, ref SkillActionResultDatas resultData)
	{
		if (tile == null)
		{
			return;
		}
		TheLastStand.Model.Building.Building building = tile.Building;
		if (building != null && !building.CanSpawnEnemyOnIt)
		{
			BuildingManager.DestroyBuilding(tile);
		}
		// Nếu Caster là Boss, gom quái theo từng Sector bản đồ
		if (caster is BossUnit { IsDeathRattling: false })
		{
			int sectorIndexForTile = TPSingleton<SectorManager>.Instance.GetSectorIndexForTile(tile);
			if (TPSingleton<BossManager>.Instance.RecentlySpawnedUnitsBySector[sectorIndexForTile].ContainsKey(base.SkillAction.Skill))
			{
				TPSingleton<BossManager>.Instance.RecentlySpawnedUnitsBySector[sectorIndexForTile][base.SkillAction.Skill].Item2.AddAtKey((enemySpawnData.Id, enemySpawnData.UnitCreationSettings), tile);
				return;
			}
			TPSingleton<BossManager>.Instance.RecentlySpawnedUnitsBySector[sectorIndexForTile].Add(base.SkillAction.Skill, new Tuple<ISkillCaster, Dictionary<(string, UnitCreationSettings), List<Tile>>>(caster, new Dictionary<(string, UnitCreationSettings), List<Tile>> { 
			{
				(enemySpawnData.Id, enemySpawnData.UnitCreationSettings),
				new List<Tile> { tile }
			} }));
		}
		else
		{
			resultData.UnitsToSpawnTarget.Add(tile, (enemySpawnData.Id, enemySpawnData.UnitCreationSettings));
		}
	}
}
