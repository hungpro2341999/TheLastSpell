namespace TheLastStand.Model.Apocalypse;

public class ApocalypseModifierIdAndStepIndex
{
	public string ModifierId { get; private set; }

	public int StepIndex { get; private set; }

	public ApocalypseModifierIdAndStepIndex(string modifierId, int stepIndex)
	{
		ModifierId = modifierId;
		StepIndex = stepIndex;
	}
}
