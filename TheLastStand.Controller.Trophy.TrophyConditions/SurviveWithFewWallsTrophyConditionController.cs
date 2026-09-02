using System.Linq;
using TPLib;
using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class SurviveWithFewWallsTrophyConditionController : ValueIntTrophyConditionController
{
	public override string Name => "SurviveWithFewWalls";

	public SurviveWithFewWallsTrophyDefinition SurviveWithFewWallsTrophyDefinition => base.TrophyConditionDefinition as SurviveWithFewWallsTrophyDefinition;

	public SurviveWithFewWallsTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
		base.ValueProgression = TPSingleton<BuildingManager>.Instance.Buildings.Count((TheLastStand.Model.Building.Building x) => x.IsWall);
	}

	public override void Clear()
	{
		base.ValueProgression = TPSingleton<BuildingManager>.Instance.Buildings.Count((TheLastStand.Model.Building.Building x) => x.IsWall);
		isCompleted = false;
	}

	public override string ToString()
	{
		return string.Format("    If you win your game, this trophy state will be : {0} you have {1} walls / {2}", (base.ValueProgression <= SurviveWithFewWallsTrophyDefinition.Value) ? "Completed" : "Incompleted", base.ValueProgression, SurviveWithFewWallsTrophyDefinition.Value);
	}

	protected override void CheckCompleteState()
	{
		isCompleted = base.ValueProgression <= SurviveWithFewWallsTrophyDefinition.Value;
	}
}
