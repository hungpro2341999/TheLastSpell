using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class NightCompletedXTurnsAfterSpawnEndConditionController : ValueIntTrophyConditionController
{
	public override string Name => "NightCompletedXTurnsAfterSpawn";

	public NightCompletedXTurnsAfterSpawnEndTrophyDefinition NightCompletedXTurnsAfterSpawnEndTrophyDefinition => base.TrophyConditionDefinition as NightCompletedXTurnsAfterSpawnEndTrophyDefinition;

	public NightCompletedXTurnsAfterSpawnEndConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}

	public override string ToString()
	{
		return $"Night End {base.ValueProgression} turns after Spawn End, this number shouldn't exceed {NightCompletedXTurnsAfterSpawnEndTrophyDefinition.Value}" + "\r\n";
	}

	protected override void CheckCompleteState()
	{
		isCompleted = base.ValueProgression <= NightCompletedXTurnsAfterSpawnEndTrophyDefinition.Value && base.ValueProgression >= 0;
	}
}
