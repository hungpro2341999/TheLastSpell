using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class HeroDeadTrophyConditionController : ValueIntTrophyConditionController
{
	public override string Name => "HeroDead";

	public HeroDeadTrophyDefinition HeroDeadDefinition => base.TrophyConditionDefinition as HeroDeadTrophyDefinition;

	public HeroDeadTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}
}
