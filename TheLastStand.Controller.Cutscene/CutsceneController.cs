using System.Collections;
using TheLastStand.Definition.Cutscene;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public abstract class CutsceneController
{
	protected ICutsceneDefinition CutsceneDefinition { get; }

	protected CutsceneController(ICutsceneDefinition cutsceneDefinition)
	{
		CutsceneDefinition = cutsceneDefinition;
	}

	public abstract IEnumerator Play(CutsceneData cutsceneData);
}
