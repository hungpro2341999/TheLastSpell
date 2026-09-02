using System.Collections.Generic;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Controller;
using TheLastStand.Controller.Unit;
using TheLastStand.Controller.Unit.Enemy;
using TheLastStand.Controller.Unit.Enemy.Affix;
using TheLastStand.Controller.Unit.Stat;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.Building;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Enemy.Affix;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Unit;
using TheLastStand.View.Unit;
using UnityEngine;

namespace TheLastStand.Model.Unit.Enemy;

public class EnemyUnit : Unit, IBehaviorModel, ISkillCaster, ITileObject, IEntity, IBossPhaseActor
{
	public class StringToEnemyUnitTemplateIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(EnemyUnitDatabase.EnemyUnitTemplateDefinitions.Keys);
	}

	public TheLastStand.Model.Building.Building LinkedBuilding;

	private string bossPhaseActorId;

	public List<EnemyAffix> Affixes { get; private set; } = new List<EnemyAffix>();

	public int AlliesDamaged { get; set; }

	public int AlliesKilled { get; set; }

	public bool BossActorDeathPrepared { get; private set; }

	public bool IsBossPhaseActor => BossPhaseActorId != null;

	public string BossPhaseActorId
	{
		get
		{
			return bossPhaseActorId;
		}
		set
		{
			if (!(BossPhaseActorId == value))
			{
				if (BossPhaseActorId != null)
				{
					TPSingleton<BossManager>.Instance.BossPhaseActors.TryRemoveAtKey(BossPhaseActorId, this);
				}
				bossPhaseActorId = value;
				TPSingleton<BossManager>.Instance.BossPhaseActors.AddAtKey(BossPhaseActorId, (IBossPhaseActor)this);
			}
		}
	}

	public IBehaviorController BehaviorController => base.UnitController as IBehaviorController;

	public BehaviorDefinition BehaviourDefinition => EnemyUnitTemplateDefinition.Behavior;

	public override bool CanAffectTargetInFog => true;

	public int CurrentVariantIndex { get; set; } = -1;

	public ComputedGoal[] CurrentGoals { get; set; }

	public Sprite DefaultSpriteBack { get; set; }

	public Sprite DefaultSpriteFront { get; set; }

	public virtual string Description => Localizer.Get("EnemyDescription_" + EnemyUnitTemplateDefinition.Id);

	public EnemyUnitController EnemyUnitController => base.UnitController as EnemyUnitController;

	public EnemyUnitStatsController EnemyUnitStatsController => base.UnitStatsController as EnemyUnitStatsController;

	public EnemyUnitTemplateDefinition EnemyUnitTemplateDefinition => base.UnitTemplateDefinition as EnemyUnitTemplateDefinition;

	public EnemyUnitView EnemyUnitView => UnitView as EnemyUnitView;

	public PlayableUnit ExplosionResponsible { get; set; }

	public IBehaviorModel.E_GoalComputingStep GoalComputingStep { get; set; }

	public Goal[] Goals { get; set; }

	public bool HasBeenDestroyed { get; set; }

	public bool HasDodged { get; set; }

	public override string Id => EnemyUnitTemplateDefinition.Id;

	public bool IgnoreFromEnemyUnitsCount { get; }

	public InterpretedTurnConditionContext InterpretedTurnConditionContext { get; set; }

	public override bool IsDeadOrDeathRattling
	{
		get
		{
			if (!IsDeathRattling)
			{
				return base.IsDeadOrDeathRattling;
			}
			return true;
		}
	}

	public bool IsDeathRattling { get; set; }

	public bool IsExecutingSkillOnSpawn { get; set; }

	public bool IsGuardian { get; }

	public bool IsInCitySincePlayerTurnStart { get; set; }

	public int ModifiedDayNumber => TPSingleton<GameManager>.Instance.Game.DayNumber + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.EnemiesProgressionOffset + ApocalypseManager.CurrentApocalypse.EnemiesProgressionOffset;

	public int LastHourInFog { get; set; } = -1;

	public int LastHourInAnyFog { get; set; } = -1;

	public override string Name => Localizer.Get("EnemyName_" + EnemyUnitTemplateDefinition.Id);

	public int NumberOfGoalsToCompute { get; set; } = 1;

	public bool CanCausePanic => GetClampedStatValue(UnitStatDefinition.E_Stat.Panic) > 0f;

	public int SpawnedHour { get; set; }

	public int TurnsToSkipOnSpawn { get; set; }

	public virtual string SpecificId => Id;

	public string AssetsId => EnemyUnitTemplateDefinition.AssetsId;

	public string SpecificAssetsId => EnemyUnitTemplateDefinition.SpecificAssetsId;

	public List<SkillProgression> SkillProgressions => EnemyUnitTemplateDefinition.SkillProgressions;

	public Tile TargetTile { get; set; }

	public override string UniqueIdentifier => $"{SpecificId}_{base.RandomId}";

	public string VariantId { get; set; } = string.Empty;

	public bool ShouldCausePanic
	{
		get
		{
			if (base.IsInCity)
			{
				return CanCausePanic;
			}
			return false;
		}
	}

	public EnemyUnit(UnitTemplateDefinition unitTemplateDefinition, UnitController unitController, UnitView unitView, UnitCreationSettings unitCreationSettings)
		: base(unitTemplateDefinition, unitController, unitView)
	{
		Init();
		LinkedBuilding = unitCreationSettings.LinkedBuilding;
		IsGuardian = unitCreationSettings.IsGuardian;
		IgnoreFromEnemyUnitsCount = unitCreationSettings.IgnoreFromEnemyUnitsCount || unitCreationSettings.IsGuardian;
	}

	public EnemyUnit(UnitTemplateDefinition unitTemplateDefinition, SerializedEnemyUnit serializedUnit, UnitController unitController, UnitView unitView, UnitCreationSettings unitCreationSettings, int saveVersion)
		: base(unitTemplateDefinition, unitController, unitView)
	{
		Deserialize(serializedUnit, saveVersion);
		Init();
		LinkedBuilding = unitCreationSettings.LinkedBuilding;
		IsGuardian = unitCreationSettings.IsGuardian;
		IgnoreFromEnemyUnitsCount = unitCreationSettings.IgnoreFromEnemyUnitsCount || unitCreationSettings.IsGuardian;
	}

	public override bool CanBeDamaged()
	{
		if (base.CanBeDamaged())
		{
			return !EnemyUnitTemplateDefinition.IsInvulnerable;
		}
		return false;
	}

	public bool HasLightFogSupplier(out ILightFogSupplier lightFogSupplier)
	{
		foreach (EnemyAffix affix in Affixes)
		{
			if (affix is ILightFogSupplier lightFogSupplier2)
			{
				lightFogSupplier = lightFogSupplier2;
				return true;
			}
		}
		lightFogSupplier = null;
		return false;
	}

	public bool HasSpawnGoals()
	{
		Goal[] goals = Goals;
		for (int i = 0; i < goals.Length; i++)
		{
			if (goals[i].GoalDefinition.GoalComputingStep.HasFlag(IBehaviorModel.E_GoalComputingStep.OnSpawn))
			{
				return true;
			}
		}
		return false;
	}

	public override bool IsTargetableByAI()
	{
		if (EnemyUnitTemplateDefinition.IsTargetableByAI)
		{
			return base.IsTargetableByAI();
		}
		return false;
	}

	public void PrepareBossActorDeath()
	{
		if (!BossActorDeathPrepared)
		{
			TPSingleton<BossManager>.Instance.BossPhaseActorsKills.AddValueOrCreateKey(BossPhaseActorId, 1, (int a, int b) => a + b);
			BossActorDeathPrepared = true;
		}
	}

	protected override bool ComputeIsolation()
	{
		List<Tile> adjacentTiles = base.UnitController.GetAdjacentTiles();
		for (int num = adjacentTiles.Count - 1; num >= 0; num--)
		{
			if (adjacentTiles[num]?.Unit != null && adjacentTiles[num].Unit != this && adjacentTiles[num].Unit is EnemyUnit)
			{
				return false;
			}
		}
		return true;
	}

	protected EnemyAffix CreateAffix(EnemyAffixDefinition affixDefinition)
	{
		return affixDefinition.EnemyAffixEffectDefinition.EnemyAffixEffect switch
		{
			EnemyAffixEffectDefinition.E_EnemyAffixEffect.Reinforced => new EnemyReinforcedAffixController(affixDefinition, this).EnemyAffix, 
			EnemyAffixEffectDefinition.E_EnemyAffixEffect.Aura => new EnemyAuraAffixController(affixDefinition, this).EnemyAffix, 
			EnemyAffixEffectDefinition.E_EnemyAffixEffect.Misty => new EnemyMistyAffixController(affixDefinition, this).EnemyAffix, 
			EnemyAffixEffectDefinition.E_EnemyAffixEffect.Regenerative => new EnemyRegenerativeAffixController(affixDefinition, this).EnemyAffix, 
			EnemyAffixEffectDefinition.E_EnemyAffixEffect.Mirror => new EnemyMirrorAffixController(affixDefinition, this).EnemyAffix, 
			EnemyAffixEffectDefinition.E_EnemyAffixEffect.Energetic => new EnemyEnergeticAffixController(affixDefinition, this).EnemyAffix, 
			EnemyAffixEffectDefinition.E_EnemyAffixEffect.Revenge => new EnemyRevengeAffixController(affixDefinition, this).EnemyAffix, 
			EnemyAffixEffectDefinition.E_EnemyAffixEffect.Purge => new EnemyPurgeAffixController(affixDefinition, this).EnemyAffix, 
			EnemyAffixEffectDefinition.E_EnemyAffixEffect.Barrier => new EnemyBarrierAffixController(affixDefinition, this).EnemyAffix, 
			EnemyAffixEffectDefinition.E_EnemyAffixEffect.HigherPlane => new EnemyHigherPlaneAffixController(affixDefinition, this).EnemyAffix, 
			EnemyAffixEffectDefinition.E_EnemyAffixEffect.HealthChunks => new EnemyHealthChunksAffixController(affixDefinition, this).EnemyAffix, 
			_ => null, 
		};
	}

	protected override void Init()
	{
		base.Init();
		base.RandomId = RandomManager.GetRandomRange(TPSingleton<EnemyUnitManager>.Instance, 0, int.MaxValue);
		NumberOfGoalsToCompute = ((BehaviourDefinition.NumberOfGoalsToExecute == 0) ? 1 : BehaviourDefinition.NumberOfGoalsToExecute);
		TurnsToSkipOnSpawn = BehaviourDefinition.TurnsToSkipOnSpawn;
		foreach (EnemyAffixDefinition affixDefinition in EnemyUnitTemplateDefinition.AffixDefinitions)
		{
			Affixes.Add(CreateAffix(affixDefinition));
		}
	}

	public override void Log(object message, CLogLevel logLevel = CLogLevel.NORMAL, bool forcePrintInUnity = false, bool printStackTrace = false)
	{
		TPSingleton<EnemyUnitManager>.Instance.Log($"[{UniqueIdentifier}]: {message}", UnitView, logLevel, forcePrintInUnity, printStackTrace);
	}

	public override void LogError(object message, CLogLevel logLevel = CLogLevel.NORMAL, bool forcePrintInUnity = true, bool printStackTrace = true)
	{
		TPSingleton<EnemyUnitManager>.Instance.LogError($"[{UniqueIdentifier}]: {message}", UnitView, logLevel, forcePrintInUnity, printStackTrace);
	}

	public override void LogWarning(object message, CLogLevel logLevel = CLogLevel.NORMAL, bool forcePrintInUnity = true, bool printStackTrace = false)
	{
		TPSingleton<EnemyUnitManager>.Instance.LogWarning($"[{UniqueIdentifier}]: {message}", UnitView, logLevel, forcePrintInUnity, printStackTrace);
	}

	protected SerializedUnit SerializeUnit()
	{
		return base.Serialize() as SerializedUnit;
	}

	public override ISerializedData Serialize()
	{
		return new SerializedEnemyUnit(SerializeUnit())
		{
			Id = EnemyUnitTemplateDefinition.Id,
			BossPhaseActorId = BossPhaseActorId,
			OverrideVariantId = CurrentVariantIndex,
			LinkedBuilding = LinkedBuilding?.RandomId,
			IsGuardian = IsGuardian,
			IgnoreFromEnemyUnitsCount = IgnoreFromEnemyUnitsCount,
			EnemyUnitStats = (EnemyUnitStatsController.EnemyUnitStats.Serialize() as SerializedEnemyUnitStats),
			LastHourInFog = LastHourInFog,
			LastHourInAnyFog = LastHourInAnyFog,
			SerializedBehavior = new SerializedBehavior(this)
		};
	}

	protected void ADeserialize(ISerializedData container = null, int saveVersion = -1)
	{
		base.Deserialize((container as ASerializedEnemyUnit)?.Unit, saveVersion);
	}

	public override void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedEnemyUnit container2 = container as SerializedEnemyUnit;
		ADeserialize(container2, saveVersion);
	}

	public override void DeserializeAfterInit(ISerializedData container, int saveVersion)
	{
		DeserializeStats((container as SerializedEnemyUnit)?.EnemyUnitStats, saveVersion);
	}

	public void DeserializeBehavior(SerializedBehavior serializedBehavior, int saveVersion)
	{
		SpawnedHour = ((saveVersion >= 23) ? serializedBehavior.SpawnedHour : TPSingleton<GameManager>.Instance.Game.CurrentNightHour);
		TurnsToSkipOnSpawn = serializedBehavior.TurnToSkipOnSpawn;
		int num = EnemyUnitTemplateDefinition.Behavior.GoalDefinitions.Length;
		Goals = new Goal[num];
		for (int i = 0; i < num; i++)
		{
			Goals[i] = new GoalController(EnemyUnitTemplateDefinition.Behavior.GoalDefinitions[i], this).Goal;
			foreach (SerializedGoal serializedGoal in serializedBehavior.SerializedGoals)
			{
				if (serializedGoal.Id == Goals[i].Id)
				{
					Goals[i].Deserialize(serializedGoal);
					break;
				}
			}
		}
	}

	public override void DeserializeStats(ISerializedData serializedUnitStats, int saveVersion)
	{
		base.UnitStatsController = new EnemyUnitStatsController(serializedUnitStats as SerializedEnemyUnitStats, this, saveVersion);
	}
}
