using TPLib;
using TheLastStand.Manager;

namespace TheLastStand.Model.Unit.Enemy.Affix;

public class EnemyAffixInterpreter : FormulaInterpreterContext
{
	public int Day => TPSingleton<GameManager>.Instance.DayNumber;
}
