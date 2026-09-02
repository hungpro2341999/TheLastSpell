using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class ArmoredEnemiesDamagedByArmorShreddingTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "ArmoredEnemiesDamagedByArmorShredding";

	public ArmoredEnemiesDamagedByArmorShreddingTrophyDefinition ArmoredEnemiesDamagedByArmorShreddingTrophyDefinition => base.TrophyConditionDefinition as ArmoredEnemiesDamagedByArmorShreddingTrophyDefinition;

	public ArmoredEnemiesDamagedByArmorShreddingTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}
}
