using System.Collections;
using TheLastStand.Definition.Cutscene;
using TheLastStand.View.Camera;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public class ZoomCutsceneController : CutsceneController
{
	public ZoomCutsceneDefinition ZoomCutsceneDefinition => base.CutsceneDefinition as ZoomCutsceneDefinition;

	public ZoomCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		yield return ACameraView.Zoom(ZoomCutsceneDefinition.In, ZoomCutsceneDefinition.Instant);
	}
}
