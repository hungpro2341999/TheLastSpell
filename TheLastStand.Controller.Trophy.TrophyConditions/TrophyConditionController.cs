using TPLib;
using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public abstract class TrophyConditionController : ISerializable, IDeserializable
{
	protected bool isCompleted;

	public TrophyConditionDefinition TrophyConditionDefinition { get; }

	protected bool PrevCompletedState { get; set; }

	public virtual bool IsCompleted
	{
		get
		{
			CheckCompleteState();
			return isCompleted;
		}
	}

	public bool IsCompletedWithoutUpdate => isCompleted;

	public abstract string Name { get; }

	public TheLastStand.Model.Trophy.Trophy TrophyModel { get; }

	public virtual bool ShouldCheckCompletionBeforeUpdate => true;

	public TrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
	{
		TrophyConditionDefinition = trophyConditionDefinition;
		TrophyModel = trophy;
	}

	public abstract void AppendValue(params object[] args);

	protected virtual void OnValueChange()
	{
		if (IsCompleted && !PrevCompletedState)
		{
			string message = $"New souls trophy completed: {TrophyModel.TrophyController}";
			TPSingleton<TrophyManager>.Instance.Log(message);
		}
	}

	public virtual void Clear()
	{
	}

	public abstract void Deserialize(ISerializedData container = null, int saveVersion = -1);

	public void ForceComplete()
	{
		isCompleted = true;
	}

	public abstract ISerializedData Serialize();

	public abstract void SetValue(params object[] args);

	public virtual void SetMaxValue(params object[] args)
	{
	}

	protected virtual void CheckCompleteState()
	{
		isCompleted = false;
	}
}
