using System.Collections;
using TPLib.Yield;
using TheLastStand.Definition.Cutscene;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public class WaitCutsceneController : CutsceneController
{
	public WaitCutsceneDefinition WaitCutsceneDefinition => base.CutsceneDefinition as WaitCutsceneDefinition;

	public WaitCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		yield return SharedYields.WaitForSeconds(WaitCutsceneDefinition.Duration);
	}
}
