using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Manager;
using TheLastStand.Model.Trophy;
using TheLastStand.Serialization.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public abstract class ValueIntTrophyConditionController : TrophyConditionController
{
	public int ValueProgression { get; protected set; }

	protected ValueIntTrophyConditionDefinition ValueIntTrophyConditionDefinition => base.TrophyConditionDefinition as ValueIntTrophyConditionDefinition;

	protected ValueIntTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}

	public override void AppendValue(params object[] args)
	{
		if (args.Length != 1)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type " + Name + " need only 1 argument", CLogLevel.DETAILED);
			return;
		}
		if (!(args[0] is int num))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type " + Name + " should have as first argument an int !", CLogLevel.DETAILED);
			return;
		}
		ValueProgression += num;
		OnValueChange();
	}

	public override void Clear()
	{
		ValueProgression = 0;
		isCompleted = false;
	}

	public override void SetValue(params object[] args)
	{
		if (args.Length != 1)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type " + Name + " need only 1 argument", CLogLevel.DETAILED);
			return;
		}
		if (!(args[0] is int valueProgression))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type " + Name + " should have as first argument an int !", CLogLevel.DETAILED);
			return;
		}
		ValueProgression = valueProgression;
		OnValueChange();
	}

	public override string ToString()
	{
		return $"{Name}: {ValueProgression}/{ValueIntTrophyConditionDefinition.Value}" + "\r\n";
	}

	protected override void CheckCompleteState()
	{
		isCompleted = ValueProgression >= ValueIntTrophyConditionDefinition.Value;
	}

	public override void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedValueIntTrophy serializedValueIntTrophy = container as SerializedValueIntTrophy;
		ValueProgression = serializedValueIntTrophy.ValueProgression;
	}

	public override ISerializedData Serialize()
	{
		return new SerializedValueIntTrophy
		{
			Name = Name,
			ValueProgression = ValueProgression
		};
	}
}
