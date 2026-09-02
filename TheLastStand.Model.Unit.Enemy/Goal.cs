using TheLastStand.Controller.Unit.Enemy;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Skill;
using TheLastStand.Serialization.Unit;

namespace TheLastStand.Model.Unit.Enemy;

public class Goal : FormulaInterpreterContext, ISkillContainer, ISerializable, IDeserializable
{
	public int Cooldown { get; set; } = -1;

	public GoalController GoalController { get; }

	public GoalDefinition GoalDefinition { get; }

	public GoalInterpreterContext GoalInterpreterContext { get; }

	public string Id => GoalDefinition.Id;

	public IBehaviorModel Owner { get; }

	public ISkillCaster Holder => Owner;

	public TheLastStand.Model.Skill.Skill Skill { get; set; }

	public float SkillMinRange => Skill.MinRange;

	public float SkillMaxRange
	{
		get
		{
			if (!(Owner is Unit unit))
			{
				return Skill.BaseMaxRange;
			}
			return unit.UnitController.GetModifiedMaxRange(Skill);
		}
	}

	public Goal(GoalDefinition goalDefinition, GoalController goalController, IBehaviorModel owner)
		: base(owner)
	{
		GoalDefinition = goalDefinition;
		GoalController = goalController;
		Owner = owner;
		GoalInterpreterContext = new GoalInterpreterContext(this);
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedGoal serializedGoal = container as SerializedGoal;
		Cooldown = serializedGoal.Cooldown;
	}

	public ISerializedData Serialize()
	{
		return new SerializedGoal
		{
			Cooldown = Cooldown,
			Id = GoalDefinition.Id
		};
	}
}
