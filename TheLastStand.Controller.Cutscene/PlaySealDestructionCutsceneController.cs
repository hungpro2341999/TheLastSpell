using System.Collections;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager.Building;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public class PlaySealDestructionCutsceneController : CutsceneController
{
	public PlaySealDestructionCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		yield return BuildingManager.MagicCircle.MagicCircleView.PlaySealTransitionAnimation();
	}
}
