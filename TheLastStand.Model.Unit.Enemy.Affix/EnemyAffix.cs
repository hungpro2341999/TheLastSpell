using TheLastStand.Controller.Unit.Enemy.Affix;
using TheLastStand.Definition.Unit.Enemy.Affix;

namespace TheLastStand.Model.Unit.Enemy.Affix;

public abstract class EnemyAffix
{
	public readonly EnemyAffixInterpreter Interpreter = new EnemyAffixInterpreter();

	public EnemyAffixController EnemyAffixController { get; protected set; }

	public EnemyAffixDefinition EnemyAffixDefinition { get; protected set; }

	public EnemyUnit EnemyUnit { get; private set; }

	public EnemyAffix(EnemyAffixController enemyAffixController, EnemyAffixDefinition enemyAffixDefinition, EnemyUnit enemyUnit)
	{
		EnemyAffixController = enemyAffixController;
		EnemyAffixDefinition = enemyAffixDefinition;
		EnemyUnit = enemyUnit;
	}
}
