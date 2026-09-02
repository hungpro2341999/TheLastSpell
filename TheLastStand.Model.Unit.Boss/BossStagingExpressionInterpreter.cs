namespace TheLastStand.Model.Unit.Boss;

public class BossStagingExpressionInterpreter
{
	public float MovementDuration { get; private set; }

	public int NumberOfSectors { get; private set; }

	public float PauseDuration { get; private set; }

	public BossStagingExpressionInterpreter(int numberOfSectors, float pauseDuration, float movementDuration)
	{
		NumberOfSectors = numberOfSectors;
		PauseDuration = pauseDuration;
		MovementDuration = movementDuration;
	}
}
