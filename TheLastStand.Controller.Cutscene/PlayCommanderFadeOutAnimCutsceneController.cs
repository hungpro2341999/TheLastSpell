using System.Collections;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager.Building;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public class PlayCommanderFadeOutAnimCutsceneController : CutsceneController
{
	public PlayCommanderFadeOutAnimCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		BuildingManager.MagicCircle.MagicCircleView.PlayCommanderFadeOutAnim();
		yield break;
	}
}
