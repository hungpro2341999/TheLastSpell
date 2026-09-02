using System.Collections.Generic;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;
using TheLastStand.Model.Status;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Movement;

namespace TheLastStand.Model.Unit.Perk;

public class PerkDataContainer
{
	public AttackSkillActionExecutionTileData AttackData;

	public ITileObject Caster;

	public TheLastStand.Model.Skill.Skill Skill;

	public IDamageable TargetDamageable;

	public Tile TargetTile;

	public bool IsTriggeredByPerk;

	public TheLastStand.Model.Status.Status StatusApplied;

	public TheLastStand.Model.Status.Status.E_StatusType TargetUnitPreviousStatuses;

	public HashSet<AttackSkillActionExecutionTileData> AllAttackData;

	public SkillCommand.UndoneSkillData UndoneSkillData;

	public int DistanceFromCaster => Skill.SkillAction.SkillActionExecution.SkillSourceTiles.GetFirstClosestTile(TargetTile).distance;

	public int TargetUnitDistanceFromCaster => Skill.SkillAction.SkillActionExecution.SkillSourceTiles.GetFirstClosestTile(TargetUnit.OriginTile).distance;

	public Unit TargetUnit => TargetDamageable as Unit;

	public TheLastStand.Model.Building.Building TargetBuilding => (TargetDamageable as DamageableModule)?.BuildingParent;

	public bool IsSelfTargeting
	{
		get
		{
			if (TargetDamageable != null)
			{
				return Caster == TargetDamageable;
			}
			return false;
		}
	}

	public bool IsTargetUnitPrimaryTarget
	{
		get
		{
			Unit targetUnit = TargetUnit;
			if (targetUnit == null)
			{
				return false;
			}
			return targetUnit.OccupiedTiles?.Contains(TargetTile) == true;
		}
	}

	public bool HasTargetTile => TargetTile != null;

	public bool HasSkill => Skill != null;

	public bool IsTargetUnit => TargetDamageable is Unit;

	public bool IsTargetEnemyUnit => TargetDamageable is EnemyUnit;

	public bool IsTargetPlayableUnit => TargetDamageable is PlayableUnit;

	public bool IsTargetBuilding => TargetDamageable is DamageableModule;

	public bool IsCasterUnit => Caster is Unit;

	public bool IsCasterEnemyUnit => Caster is EnemyUnit;

	public bool IsCasterPlayableUnit => Caster is PlayableUnit;

	public bool AppliedStun
	{
		get
		{
			TheLastStand.Model.Status.Status statusApplied = StatusApplied;
			if (statusApplied == null)
			{
				return false;
			}
			return statusApplied.StatusType == TheLastStand.Model.Status.Status.E_StatusType.Stun;
		}
	}

	public bool AppliedPoison
	{
		get
		{
			TheLastStand.Model.Status.Status statusApplied = StatusApplied;
			if (statusApplied == null)
			{
				return false;
			}
			return statusApplied.StatusType == TheLastStand.Model.Status.Status.E_StatusType.Poison;
		}
	}

	public bool AppliedCharge
	{
		get
		{
			TheLastStand.Model.Status.Status statusApplied = StatusApplied;
			if (statusApplied == null)
			{
				return false;
			}
			return statusApplied.StatusType == TheLastStand.Model.Status.Status.E_StatusType.Charged;
		}
	}

	public bool AppliedBuff
	{
		get
		{
			TheLastStand.Model.Status.Status statusApplied = StatusApplied;
			if (statusApplied == null)
			{
				return false;
			}
			return statusApplied.StatusType == TheLastStand.Model.Status.Status.E_StatusType.Buff;
		}
	}

	public bool AppliedDebuff
	{
		get
		{
			TheLastStand.Model.Status.Status statusApplied = StatusApplied;
			if (statusApplied == null)
			{
				return false;
			}
			return statusApplied.StatusType == TheLastStand.Model.Status.Status.E_StatusType.Debuff;
		}
	}

	public bool AppliedContagion
	{
		get
		{
			TheLastStand.Model.Status.Status statusApplied = StatusApplied;
			if (statusApplied == null)
			{
				return false;
			}
			return statusApplied.StatusType == TheLastStand.Model.Status.Status.E_StatusType.Contagion;
		}
	}

	public bool AppliedInvulnerable
	{
		get
		{
			TheLastStand.Model.Status.Status statusApplied = StatusApplied;
			if (statusApplied == null)
			{
				return false;
			}
			return statusApplied.StatusType == TheLastStand.Model.Status.Status.E_StatusType.Invulnerable;
		}
	}

	public bool AppliedNegativeAlteration
	{
		get
		{
			if (StatusApplied != null)
			{
				return TheLastStand.Model.Status.Status.E_StatusType.AllNegative.HasFlag(StatusApplied.StatusType);
			}
			return false;
		}
	}

	public bool TargetWasStunned => (TargetUnitPreviousStatuses & TheLastStand.Model.Status.Status.E_StatusType.Stun) != 0;

	public bool TargetWasPoisoned => (TargetUnitPreviousStatuses & TheLastStand.Model.Status.Status.E_StatusType.Poison) != 0;

	public bool TargetWasCharged => (TargetUnitPreviousStatuses & TheLastStand.Model.Status.Status.E_StatusType.Charged) != 0;

	public bool TargetWasBuffed => (TargetUnitPreviousStatuses & TheLastStand.Model.Status.Status.E_StatusType.Buff) != 0;

	public bool TargetWasDebuffed => (TargetUnitPreviousStatuses & TheLastStand.Model.Status.Status.E_StatusType.Debuff) != 0;

	public bool TargetWasContagious => (TargetUnitPreviousStatuses & TheLastStand.Model.Status.Status.E_StatusType.Contagion) != 0;

	public bool TargetWasInvulnerable => (TargetUnitPreviousStatuses & TheLastStand.Model.Status.Status.E_StatusType.Invulnerable) != 0;

	public bool TargetHadNegativeAlteration => (TargetUnitPreviousStatuses & TheLastStand.Model.Status.Status.E_StatusType.AllNegative) != 0;
}
