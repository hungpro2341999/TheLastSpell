using System.Collections;
using TheLastStand.Definition.Cutscene;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public class IncreaseFogCutsceneController : CutsceneController
{
	public IncreaseFogCutsceneDefinition IncreaseFogCutsceneDefinition => base.CutsceneDefinition as IncreaseFogCutsceneDefinition;

	public IncreaseFogCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		FogController.IncreaseDensity(refreshFog: false, IncreaseFogCutsceneDefinition.Value);
		yield break;
	}
}
