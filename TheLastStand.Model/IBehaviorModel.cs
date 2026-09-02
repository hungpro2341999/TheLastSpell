using System;
using System.Collections.Generic;
using TheLastStand.Controller;
using TheLastStand.Controller.Unit.Enemy;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Serialization;

namespace TheLastStand.Model;

public interface IBehaviorModel : ISkillCaster, ITileObject, IEntity
{
	[Flags]
	public enum E_GoalComputingStep
	{
		BeforeMoving = 1,
		AfterMoving = 2,
		OnSpawn = 4,
		OnDeath = 8,
		DuringTurn = 3
	}

	IBehaviorController BehaviorController { get; }

	BehaviorDefinition BehaviourDefinition { get; }

	ComputedGoal[] CurrentGoals { get; set; }

	E_GoalComputingStep GoalComputingStep { get; set; }

	Goal[] Goals { get; set; }

	InterpretedTurnConditionContext InterpretedTurnConditionContext { get; set; }

	bool IsDeathRattling { get; set; }

	int ModifiedDayNumber { get; }

	List<SkillProgression> SkillProgressions { get; }

	int NumberOfGoalsToCompute { get; set; }

	int SpawnedHour { get; set; }

	int TurnsToSkipOnSpawn { get; set; }

	Tile TargetTile { get; set; }

	void DeserializeBehavior(SerializedBehavior serializedBehavior, int saveVersion);
}
