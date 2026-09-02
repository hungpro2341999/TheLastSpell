using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller;
using TheLastStand.Controller.Building.Module;
using TheLastStand.Controller.Skill;
using TheLastStand.Controller.TileMap;
using TheLastStand.Controller.Unit.Enemy;
using TheLastStand.Definition;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Status;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Building;
using TheLastStand.Serialization.Unit;
using TheLastStand.View;
using UnityEngine;

namespace TheLastStand.Model.Building.Module;

public class BattleModule : BuildingModule, ISkillContainer, IBehaviorModel, ISkillCaster, ITileObject, IEntity
{
	public BattleModuleController BattleModuleController => base.BuildingModuleController as BattleModuleController;

	public BattleModuleDefinition BattleModuleDefinition => base.BuildingModuleDefinition as BattleModuleDefinition;

	public bool CanAffectTargetInFog => false;

	public bool CanAffectTargetInLightFog => true;

	public string Name => base.BuildingParent.Name;

	public bool HasDisabledStateAndZeroRemainingCharges
	{
		get
		{
			if (BattleModuleDefinition.HasDisabledState)
			{
				return GetHandledDefenseRemainingCharges() == 0;
			}
			return false;
		}
	}

	public ISkillCaster Holder => this;

	public List<Tile> OccupiedTiles => base.BuildingParent.OccupiedTiles;

	public Tile OriginTile => base.BuildingParent.OriginTile;

	public List<string> PreventedSkillsIds { get; } = new List<string>();

	public string Id => base.BuildingParent.Id;

	public bool IsInCity => base.BuildingParent.IsInCity;

	public bool IsInWorld => base.BuildingParent.IsInWorld;

	public int ModifiedDayNumber => TPSingleton<GameManager>.Instance.Game.DayNumber;

	public List<ModifyDefensesDamageEffect> ModifyDefensesDamagePerks { get; } = new List<ModifyDefensesDamageEffect>();

	public int RandomId => base.BuildingParent.RandomId;

	public List<SkillProgression> SkillProgressions => BattleModuleDefinition.SkillProgressions;

	public ITileObjectController TileObjectController => base.BuildingParent.TileObjectController;

	public ITileObjectDefinition TileObjectDefinition => base.BuildingParent.TileObjectDefinition;

	public ITileObjectView TileObjectView => base.BuildingParent.TileObjectView;

	public IBehaviorController BehaviorController => BattleModuleController;

	public BehaviorDefinition BehaviourDefinition => BattleModuleDefinition?.Behavior;

	public ComputedGoal[] CurrentGoals { get; set; }

	public IBehaviorModel.E_GoalComputingStep GoalComputingStep { get; set; } = IBehaviorModel.E_GoalComputingStep.AfterMoving;

	public Goal[] Goals { get; set; }

	public InterpretedTurnConditionContext InterpretedTurnConditionContext { get; set; }

	public bool IsDeathRattling { get; set; }

	public int NumberOfGoalsToCompute { get; set; } = 1;

	public int SpawnedHour { get; set; }

	public int TurnsToSkipOnSpawn { get; set; }

	public Tile TargetTile { get; set; }

	public bool IsExecutingSkill => SkillExecutionCoroutine != null;

	public List<TheLastStand.Model.Skill.Skill> Skills { get; set; } = new List<TheLastStand.Model.Skill.Skill>();

	public Coroutine SkillExecutionCoroutine { get; set; }

	public int RemainingTrapCharges { get; set; }

	public bool ShouldTriggerDeathRattle { get; set; } = true;

	public ISkillCasterController SkillCasterController => BattleModuleController;

	public string UniqueIdentifier => base.BuildingParent.UniqueIdentifier;

	public BattleModule(Building buildingParent, BattleModuleDefinition battleModuleDefinition, BattleModuleController battleModuleController)
		: base(buildingParent, battleModuleDefinition, battleModuleController)
	{
		RemainingTrapCharges = BattleModuleDefinition.MaximumTrapCharges;
		TurnsToSkipOnSpawn = BattleModuleDefinition.Behavior?.TurnsToSkipOnSpawn ?? 0;
	}

	public int ComputeStatusDuration(TheLastStand.Model.Status.Status.E_StatusType statusType, int baseValue, PerkDataContainer perkDataContainer = null, Dictionary<UnitStatDefinition.E_Stat, float> statModifiers = null)
	{
		return baseValue;
	}

	public int ComputeModifyDefensesDamagePerksPercentage()
	{
		int num = 0;
		for (int num2 = ModifyDefensesDamagePerks.Count - 1; num2 >= 0; num2--)
		{
			ModifyDefensesDamageEffect modifyDefensesDamageEffect = ModifyDefensesDamagePerks[num2];
			if (TileMapController.IsAnyTileInRange(modifyDefensesDamageEffect.APerkModule.Perk.Owner.OriginTile, base.BuildingParent.OccupiedTiles, modifyDefensesDamageEffect.ModifyDefensesDamageEffectDefinition.RangeExpression.EvalToInt(ModifyDefensesDamagePerks[num2])))
			{
				num += modifyDefensesDamageEffect.ModifyDefensesDamageEffectDefinition.PercentageExpression.EvalToInt(ModifyDefensesDamagePerks[num2]);
			}
		}
		return num;
	}

	public void Deserialize(SerializedBuilding buildingElement)
	{
		RemainingTrapCharges = buildingElement.RemainingTrapUsage;
		foreach (SerializedSkill skill in buildingElement.Skills)
		{
			Skills.Add(new SkillController(skill, this).Skill);
		}
		DeserializeBehavior(buildingElement.SerializedBehavior, -1);
	}

	public void DeserializeBehavior(SerializedBehavior serializedBehavior, int saveVersion)
	{
		if (BehaviourDefinition == null)
		{
			return;
		}
		SpawnedHour = ((serializedBehavior != null && saveVersion >= 23) ? serializedBehavior.SpawnedHour : TPSingleton<GameManager>.Instance.Game.CurrentNightHour);
		InterpretedTurnConditionContext = new InterpretedTurnConditionContext(SpawnedHour);
		TurnsToSkipOnSpawn = serializedBehavior?.TurnToSkipOnSpawn ?? 0;
		int num = BattleModuleDefinition.Behavior.GoalDefinitions.Length;
		Goals = new Goal[num];
		for (int i = 0; i < num; i++)
		{
			Goals[i] = new GoalController(BattleModuleDefinition.Behavior.GoalDefinitions[i], this).Goal;
			if (serializedBehavior?.SerializedGoals == null)
			{
				continue;
			}
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

	public void FindSkillsFromDefinition()
	{
		if (BattleModuleDefinition.Skills == null)
		{
			return;
		}
		foreach (KeyValuePair<string, int> skill in BattleModuleDefinition.Skills)
		{
			if (SkillManager.TryGetSkillDefinitionOrDatabase(SkillProgressions, skill.Key, ModifiedDayNumber, out var skillDefinition))
			{
				Skills.Add(new SkillController(skillDefinition, this, skill.Value, skillDefinition.UsesPerTurnCount).Skill);
			}
		}
	}

	public int GetHandledDefenseRemainingCharges()
	{
		if (!base.BuildingParent.IsHandledDefense)
		{
			return -1;
		}
		bool flag = false;
		int num = 0;
		foreach (TheLastStand.Model.Skill.Skill skill in Skills)
		{
			if (skill != null && skill.OverallUses != 0)
			{
				num += skill.OverallUsesRemaining;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return -1;
		}
		return num;
	}

	public bool IsTargetableByAI()
	{
		return base.BuildingParent.IsTargetableByAI();
	}

	public bool IsTrapFullyCharged()
	{
		return RemainingTrapCharges == BattleModuleDefinition.MaximumTrapCharges;
	}

	public void Serialize(SerializedBuilding buildingElement)
	{
		buildingElement.RemainingTrapUsage = RemainingTrapCharges;
		buildingElement.Skills = Skills.Select((TheLastStand.Model.Skill.Skill o) => o.Serialize() as SerializedSkill).ToList();
		buildingElement.SerializedBehavior = new SerializedBehavior(this);
	}

	public void Log(object message, CLogLevel logLevel = CLogLevel.NORMAL, bool forcePrintInUnity = false, bool printStackTrace = false)
	{
		base.BuildingParent.Log(message, logLevel, forcePrintInUnity, printStackTrace);
	}

	public void LogError(object message, CLogLevel logLevel = CLogLevel.NORMAL, bool forcePrintInUnity = true, bool printStackTrace = true)
	{
		base.BuildingParent.LogError(message, logLevel, forcePrintInUnity, printStackTrace);
	}

	public void LogWarning(object message, CLogLevel logLevel = CLogLevel.NORMAL, bool forcePrintInUnity = true, bool printStackTrace = false)
	{
		base.BuildingParent.LogWarning(message, logLevel, forcePrintInUnity, printStackTrace);
	}
}
