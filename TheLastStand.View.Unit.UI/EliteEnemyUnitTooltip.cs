using TheLastStand.Definition.Unit.Enemy.Affix;

namespace TheLastStand.View.Unit.UI;

public class EliteEnemyUnitTooltip : EnemyUnitTooltip
{
	protected override EnemyAffixEffectDefinition.E_EnemyAffixBoxType GetAffixBoxType(EnemyAffixDefinition affixDefinition, int index)
	{
		if (index != 0)
		{
			return base.GetAffixBoxType(affixDefinition, index);
		}
		return EnemyAffixEffectDefinition.E_EnemyAffixBoxType.Elite;
	}
}
