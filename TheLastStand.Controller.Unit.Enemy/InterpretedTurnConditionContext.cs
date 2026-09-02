using TheLastStand.Model;

namespace TheLastStand.Controller.Unit.Enemy;

public class InterpretedTurnConditionContext : FormulaInterpreterContext
{
	public int SpawnedHour { get; private set; }

	public InterpretedTurnConditionContext(int spawnedHour)
	{
		SpawnedHour = spawnedHour;
	}
}
