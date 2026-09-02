using System;
using System.Collections.Generic;
using System.Linq;
using TPLib.Log;
using TheLastStand.Controller;
using TheLastStand.Controller.Status;
using TheLastStand.Controller.Unit;
using TheLastStand.Controller.Unit.Stat;
using TheLastStand.Definition;
using TheLastStand.Definition.Skill;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Race;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.Status;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Serialization.Unit;
using TheLastStand.View;
using TheLastStand.View.Unit;
using UnityEngine;

namespace TheLastStand.Model.Unit;

public abstract class Unit : FormulaInterpreterContext, ITileObject, ISkillCaster, IDamageable, IBarker, ISerializable, IDeserializable, IEntity
{
	public enum E_State
	{
		Error = -1,
		Ready,
		Dead
	}

	public Dictionary<E_EffectTime, Action<PerkDataContainer>> Events = new Dictionary<E_EffectTime, Action<PerkDataContainer>>
	{
		{
			E_EffectTime.OnDeath,
			null
		},
		{
			E_EffectTime.OnTileCrossed,
			null
		},
		{
			E_EffectTime.OnMovementEnd,
			null
		},
		{
			E_EffectTime.OnHitTaken,
			null
		},
		{
			E_EffectTime.OnDodge,
			null
		},
		{
			E_EffectTime.OnTargetHit,
			null
		},
		{
			E_EffectTime.OnTargetDodge,
			null
		},
		{
			E_EffectTime.OnTargetKilled,
			null
		},
		{
			E_EffectTime.OnAttackDataComputed,
			null
		},
		{
			E_EffectTime.OnSkillNextHit,
			null
		},
		{
			E_EffectTime.OnSkillCastBegin,
			null
		},
		{
			E_EffectTime.OnSkillCastEnd,
			null
		},
		{
			E_EffectTime.OnEnemyMovementEnd,
			null
		},
		{
			E_EffectTime.OnSkillStatusApplied,
			null
		},
		{
			E_EffectTime.OnStatusApplied,
			null
		},
		{
			E_EffectTime.OnSkillUndo,
			null
		},
		{
			E_EffectTime.OnRemoveStatus,
			null
		},
		{
			E_EffectTime.OnDealDamageTargetHit,
			null
		},
		{
			E_EffectTime.OnDealDamageTargetKill,
			null
		},
		{
			E_EffectTime.OnDealDamageExecutionEnd,
			null
		},
		{
			E_EffectTime.OnPerkApplyStatusEnd,
			null
		},
		{
			E_EffectTime.OnUnitSpawned,
			null
		},
		{
			E_EffectTime.OnTargetHitBeforeSkillEffectsApplied,
			null
		}
	};

	private bool isIsolated;

	private bool isIsolationStateFrozen;

	public float Accuracy => GetClampedStatValue(UnitStatDefinition.E_Stat.Accuracy);

	public float Armor
	{
		get
		{
			return GetClampedStatValue(UnitStatDefinition.E_Stat.Armor);
		}
		set
		{
			UnitStatsController.SetBaseStat(UnitStatDefinition.E_Stat.Armor, value);
		}
	}

	public float ArmorTotal => GetClampedStatValue(UnitStatDefinition.E_Stat.ArmorTotal);

	public float Block => GetClampedStatValue(UnitStatDefinition.E_Stat.Block);

	public float Dodge => GetClampedStatValue(UnitStatDefinition.E_Stat.Dodge);

	public float ExperienceGain => GetClampedStatValue(UnitStatDefinition.E_Stat.ExperienceGain);

	public float HealingReceived => GetClampedStatValue(UnitStatDefinition.E_Stat.HealingReceived);

	public float Health
	{
		get
		{
			return GetClampedStatValue(UnitStatDefinition.E_Stat.Health);
		}
		set
		{
			UnitStatsController.SetBaseStat(UnitStatDefinition.E_Stat.Health, value);
		}
	}

	public float HealthTotal => GetClampedStatValue(UnitStatDefinition.E_Stat.HealthTotal);

	public float IsolatedAttacks => GetClampedStatValue(UnitStatDefinition.E_Stat.IsolatedAttacks);

	public float Mana
	{
		get
		{
			return GetClampedStatValue(UnitStatDefinition.E_Stat.Mana);
		}
		set
		{
			UnitStatsController.SetBaseStat(UnitStatDefinition.E_Stat.Mana, value);
		}
	}

	public float ManaTotal => GetClampedStatValue(UnitStatDefinition.E_Stat.ManaTotal);

	public float MovePoints => GetClampedStatValue(UnitStatDefinition.E_Stat.MovePoints);

	public float MovePointsTotal => GetClampedStatValue(UnitStatDefinition.E_Stat.MovePointsTotal);

	public float OpportunisticAttacks => GetClampedStatValue(UnitStatDefinition.E_Stat.OpportunisticAttacks);

	public float Reliability => GetClampedStatValue(UnitStatDefinition.E_Stat.Reliability);

	public float Resistance => GetClampedStatValue(UnitStatDefinition.E_Stat.Resistance);

	public float StunChanceModifier => GetClampedStatValue(UnitStatDefinition.E_Stat.StunChanceModifier);

	public float Critical => GetClampedStatValue(UnitStatDefinition.E_Stat.Critical);

	public bool IsStunned => (StatusOwned & TheLastStand.Model.Status.Status.E_StatusType.Stun) != 0;

	public bool IsPoisoned => (StatusOwned & TheLastStand.Model.Status.Status.E_StatusType.Poison) != 0;

	public bool IsCharged => (StatusOwned & TheLastStand.Model.Status.Status.E_StatusType.Charged) != 0;

	public bool IsBuffed => (StatusOwned & TheLastStand.Model.Status.Status.E_StatusType.Buff) != 0;

	public bool IsDebuffed => (StatusOwned & TheLastStand.Model.Status.Status.E_StatusType.Debuff) != 0;

	public bool IsContagious => (StatusOwned & TheLastStand.Model.Status.Status.E_StatusType.Contagion) != 0;

	public bool IsInvulnerable => (StatusOwned & TheLastStand.Model.Status.Status.E_StatusType.Invulnerable) != 0;

	public bool IsImmune => (StatusOwned & TheLastStand.Model.Status.Status.E_StatusType.AllNegativeImmunity) != 0;

	public bool HasNegativeAlteration => (StatusOwned & TheLastStand.Model.Status.Status.E_StatusType.AllNegative) != 0;

	public int DistinctNegativeAlterationsCount => (IsStunned ? 1 : 0) + (IsPoisoned ? 1 : 0) + (IsDebuffed ? 1 : 0) + (IsContagious ? 1 : 0);

	public bool IsIsolated
	{
		get
		{
			if (!isIsolationStateFrozen)
			{
				isIsolated = ComputeIsolation();
			}
			return isIsolated;
		}
	}

	public bool IsAdjacentToBuilding => UnitController.GetAdjacentTiles().Any((Tile t) => t.Building != null);

	public int InjuryStage => UnitStatsController.UnitStats.InjuryStage;

	public AttackSkillActionDefinition.E_AttackType LastSkillType { get; set; }

	public virtual RaceDefinition BarkerRaceDefinition => null;

	public virtual Transform BarkViewFollowTarget => UnitView.transform;

	[Obsolete("Stats system has been changed, only use this for backward compatibility.")]
	public Dictionary<UnitStatDefinition.E_Stat, float> BaseStatValues { get; private set; } = new Dictionary<UnitStatDefinition.E_Stat, float>(UnitStatDefinition.SharedStatComparer);

	public virtual bool CanAffectTargetInFog => false;

	public virtual bool CanAffectTargetInLightFog => true;

	public IDamageableController DamageableController => UnitController;

	public ITileObjectDefinition TileObjectDefinition => UnitTemplateDefinition;

	public DamageableType DamageableType => UnitTemplateDefinition.UnitType;

	public IDamageableView DamageableView => UnitView;

	public Coroutine FinalizeDeathWhenNeededCoroutine { get; set; }

	public bool HasBark { get; set; }

	public bool HasBeenExiled { get; set; }

	public bool ExileForcePlayDieAnim { get; set; }

	public virtual Vector2 HUDOffset => UnitTemplateDefinition?.HUDOffset ?? Vector2.zero;

	public abstract string Id { get; }

	public bool IsDead => State == E_State.Dead;

	public virtual bool IsDeadOrDeathRattling => IsDead;

	public bool IsDying { get; set; }

	public bool IsExecutingSkill => SkillExecutionCoroutine != null;

	public bool IsInCity => OccupiedTiles.Any((Tile tile) => tile.GroundDefinition.GroundCategory == GroundDefinition.E_GroundCategory.City);

	public bool IsInWorld => OriginTile != null;

	public bool IsInWatchtower => OriginTile.Building?.IsWatchtower ?? false;

	public GameDefinition.E_Direction LookDirection { get; set; } = GameDefinition.E_Direction.South;

	public abstract string Name { get; }

	public List<Tile> OccupiedTiles => OriginTile.GetOccupiedTiles(UnitTemplateDefinition);

	public Tile OriginTile { get; set; }

	public virtual bool OverrideDefaultHUDOffset => UnitTemplateDefinition?.OverrideDefaultHUDOffset ?? false;

	public List<Tile> Path { get; set; }

	public Dictionary<string, List<SkillDefinition>> PreventedSkillsByGroupId { get; } = new Dictionary<string, List<SkillDefinition>>();

	public List<string> PreventedSkillsIds { get; } = new List<string>();

	public int RandomId { get; protected set; }

	public ITileObjectController TileObjectController => UnitController;

	public ITileObjectView TileObjectView => UnitView;

	public ISkillCasterController SkillCasterController => UnitController;

	public Coroutine SkillExecutionCoroutine { get; set; }

	public E_State State { get; set; }

	public List<TheLastStand.Model.Status.Status> StatusList { get; private set; } = new List<TheLastStand.Model.Status.Status>();

	public TheLastStand.Model.Status.Status.E_StatusType StatusOwned { get; set; }

	public Sprite UiSprite { get; set; }

	public virtual string UniqueIdentifier => $"{Id}_{RandomId}";

	public UnitController UnitController { get; private set; }

	public UnitStatsController UnitStatsController { get; set; }

	public UnitTemplateDefinition UnitTemplateDefinition { get; private set; }

	public virtual UnitView UnitView { get; set; }

	public bool WillDieByPoison { get; set; }

	public Unit(UnitTemplateDefinition unitTemplateDefinition, UnitController unitController)
	{
		UnitTemplateDefinition = unitTemplateDefinition;
		UnitController = unitController;
	}

	public Unit(UnitTemplateDefinition unitTemplateDefinition, UnitController unitController, UnitView unitView)
	{
		UnitTemplateDefinition = unitTemplateDefinition;
		UnitController = unitController;
		UnitView = unitView;
	}

	public bool CanStopOn(Tile tile)
	{
		return UnitTemplateDefinition.CanStopOn(tile, this);
	}

	public bool CanTravelThrough(Tile tile)
	{
		return UnitTemplateDefinition.CanTravelThrough(tile);
	}

	public bool CanTravelThrough(Tile tile, UnitTemplateDefinition.E_MoveMethod moveMethod, bool ignoreUnits = false, bool ignoreBuildings = false)
	{
		return UnitTemplateDefinition.CanTravelThrough(tile, moveMethod, ignoreUnits, ignoreBuildings);
	}

	public virtual bool CanBeDamaged()
	{
		return !IsInvulnerable;
	}

	public virtual int ComputeStatusDuration(TheLastStand.Model.Status.Status.E_StatusType statusType, int baseValue, PerkDataContainer perkDataContainer = null, Dictionary<UnitStatDefinition.E_Stat, float> statModifiers = null)
	{
		return baseValue;
	}

	public void FreezeIsolationState(bool freeze)
	{
		if (!IsDeadOrDeathRattling)
		{
			isIsolationStateFrozen = freeze;
			isIsolated = ComputeIsolation();
		}
	}

	public List<string> GetPreventedSkillsLocalizationIds(Dictionary<string, List<SkillDefinition>> preventedSkillsByGroupId = null)
	{
		List<string> list = new List<string>();
		if (preventedSkillsByGroupId == null)
		{
			preventedSkillsByGroupId = PreventedSkillsByGroupId;
		}
		foreach (string key in preventedSkillsByGroupId.Keys)
		{
			string text = string.Empty;
			foreach (SkillDefinition item in preventedSkillsByGroupId[key])
			{
				if (!(this is EnemyUnit enemyUnit))
				{
					continue;
				}
				bool flag = false;
				foreach (SkillProgression skillProgression in enemyUnit.EnemyUnitTemplateDefinition.SkillProgressions)
				{
					if (skillProgression.Id == key && skillProgression.AreConditionsValid(enemyUnit.ModifiedDayNumber, skillProgression.UnlockedAtDay))
					{
						text = skillProgression.SkillDefinition.LocalizationId;
						flag = true;
						break;
					}
				}
				if (string.IsNullOrEmpty(text) && !flag)
				{
					text = item.LocalizationId;
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				list.Add(text);
			}
		}
		return list;
	}

	public float GetNextTurnPoisonDamage()
	{
		float num = 0f;
		for (int i = 0; i < StatusList.Count; i++)
		{
			if (StatusList[i] is PoisonStatus poisonStatus)
			{
				num += poisonStatus.DamagePerTurn;
			}
		}
		return num;
	}

	public List<Tile> GetReachableTiles()
	{
		int num = (int)GetClampedStatValue(UnitStatDefinition.E_Stat.MovePoints);
		List<Tile> list = new List<Tile>();
		list.AddRange(OccupiedTiles);
		int i = 0;
		for (int j = 0; j < num; j++)
		{
			for (int count = list.Count; i < count; i++)
			{
				List<Tile> adjacentTiles = list[i].TileObjectController.GetAdjacentTiles();
				for (int k = 0; k < adjacentTiles.Count; k++)
				{
					Tile tile = adjacentTiles[k];
					if (!tile.HasFog && !list.Contains(tile) && CanTravelThrough(tile))
					{
						list.Add(tile);
					}
				}
			}
		}
		return list;
	}

	public float GetReducedResistance(float resistanceReduction, float reductionPercentage)
	{
		float num = GetClampedStatValue(UnitStatDefinition.E_Stat.Resistance);
		if (resistanceReduction > 0f)
		{
			num -= Mathf.Clamp(resistanceReduction, 0f, (num > 0f) ? num : 0f);
		}
		if (num > 0f)
		{
			num *= 1f - reductionPercentage / 100f;
		}
		return UnitStatsController.ClampToBoundaries(num, UnitStatDefinition.E_Stat.Resistance);
	}

	public float GetClampedStatValue(UnitStatDefinition.E_Stat stat)
	{
		return UnitStatsController.GetStat(stat).FinalClamped;
	}

	public float GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat stat, float? statModifier = null)
	{
		if (!statModifier.HasValue)
		{
			return UnitStatsController.GetStat(stat).FinalClamped;
		}
		return UnitStatsController.GetStat(stat).FinalClampedStatValueWithModifier(statModifier.Value);
	}

	public bool IsAdjacentToUnit(Unit unit)
	{
		List<Tile> adjacentTiles = UnitController.GetAdjacentTiles();
		for (int num = adjacentTiles.Count - 1; num >= 0; num--)
		{
			if (adjacentTiles[num].Unit == unit)
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool IsTargetableByAI()
	{
		if (Health > 0f)
		{
			return !OriginTile.HasFog;
		}
		return false;
	}

	protected abstract bool ComputeIsolation();

	protected virtual void Init()
	{
	}

	public abstract void Log(object message, CLogLevel logLevel = CLogLevel.NORMAL, bool forcePrintInUnity = false, bool printStackTrace = false);

	public abstract void LogError(object message, CLogLevel logLevel = CLogLevel.NORMAL, bool forcePrintInUnity = true, bool printStackTrace = true);

	public abstract void LogWarning(object message, CLogLevel logLevel = CLogLevel.NORMAL, bool forcePrintInUnity = true, bool printStackTrace = false);

	public virtual void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedUnit serializedUnit = container as SerializedUnit;
		RandomId = serializedUnit.RandomId;
		if (serializedUnit.Position.HasValue)
		{
			Tile originTile = ((saveVersion != 17 || serializedUnit.Position.Value.X != 0 || serializedUnit.Position.Value.Y != 0) ? TileMapManager.GetTile(serializedUnit.Position.Value.X, serializedUnit.Position.Value.Y) : null);
			OriginTile = originTile;
		}
		DeserializeStatus(container, saveVersion);
		LookDirection = serializedUnit.LookDirection;
		LastSkillType = serializedUnit.LastSkillType;
	}

	public void DeserializeStatus(ISerializedData container, int saveVersion)
	{
		foreach (SerializedUnitStatus item in (container as SerializedUnit).Status)
		{
			TheLastStand.Model.Status.Status status = StatusControllerFactory.DeserializeStatus(item, this);
			StatusList.Add(status);
			StatusOwned |= status.StatusType;
		}
	}

	public abstract void DeserializeStats(ISerializedData container, int saveVersion);

	public virtual void DeserializeAfterInit(ISerializedData container, int saveVersion)
	{
	}

	public virtual ISerializedData Serialize()
	{
		return new SerializedUnit
		{
			RandomId = RandomId,
			Position = ((OriginTile == null) ? ((SerializableVector2Int?)null) : new SerializableVector2Int?(new SerializableVector2Int(OriginTile.Position))),
			LookDirection = LookDirection,
			LastSkillType = LastSkillType,
			Status = StatusList.Select((TheLastStand.Model.Status.Status o) => o.Serialize() as SerializedUnitStatus).ToList()
		};
	}
}
