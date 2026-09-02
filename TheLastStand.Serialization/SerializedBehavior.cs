using System;
using System.Collections.Generic;
using System.Linq;
using TheLastStand.Model;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Serialization.Unit;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedBehavior : ISerializedData
{
	public int SpawnedHour;

	public int TurnToSkipOnSpawn;

	public List<SerializedGoal> SerializedGoals;

	public SerializedBehavior()
	{
	}

	public SerializedBehavior(IBehaviorModel behaviorModel)
	{
		SpawnedHour = behaviorModel.SpawnedHour;
		TurnToSkipOnSpawn = behaviorModel.TurnsToSkipOnSpawn;
		SerializedGoals = (from g in behaviorModel.Goals?.Where((Goal g) => g.Cooldown != -1)
			select g.Serialize()).Cast<SerializedGoal>().ToList();
	}
}
