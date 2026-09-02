using System.Collections.Generic;
using System.Text;
using TheLastStand.Database;

namespace TheLastStand.Model.Apocalypse;

public class ApocalypseRetroCompatibilityLevelEquivalence
{
	public int CorrespondingLevel { get; set; }

	public List<ApocalypseModifierIdAndStepIndex> CorrespondingModifiersStep { get; private set; }

	public int OldSystemLevel { get; private set; }

	public ApocalypseRetroCompatibilityLevelEquivalence(int oldLevel, List<ApocalypseModifierIdAndStepIndex> correspondingModifiersStep)
	{
		OldSystemLevel = oldLevel;
		CorrespondingModifiersStep = correspondingModifiersStep;
		InitCorrespondModifierStepsLevel();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"ApocalypseRetroCompatibilityLevelEquivalence, OldSystemLevel: {OldSystemLevel}, CorrespondingLevel: {CorrespondingLevel}, CorrespondingModifiersStep nb: {CorrespondingModifiersStep.Count}");
		foreach (ApocalypseModifierIdAndStepIndex item in CorrespondingModifiersStep)
		{
			stringBuilder.AppendLine().Append($" id: {item.ModifierId}, step: {item.StepIndex}");
		}
		return stringBuilder.ToString();
	}

	private void InitCorrespondModifierStepsLevel()
	{
		int num = 0;
		if (CorrespondingModifiersStep != null && CorrespondingModifiersStep.Count > 0)
		{
			foreach (ApocalypseModifierIdAndStepIndex item in CorrespondingModifiersStep)
			{
				if (ApocalypseDatabase.ModifierDefinitions.TryGetValue(item.ModifierId, out var value) && item.StepIndex < value.StepDefinitions.Count)
				{
					num += value.StepDefinitions[item.StepIndex].ApocalypseLevel;
				}
			}
		}
		CorrespondingLevel = num;
	}
}
