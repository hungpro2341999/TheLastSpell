using System.Collections;
using TPLib;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public class PlayDeathAnimCutsceneController : CutsceneController
{
	public PlayDeathAnimCutsceneDefinition PlayDeathAnimCutsceneDefinition => base.CutsceneDefinition as PlayDeathAnimCutsceneDefinition;

	public PlayDeathAnimCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		if (cutsceneData.Unit == null)
		{
			TPSingleton<CutsceneManager>.Instance.LogError("Tried to play a PlayDeathAnimCutsceneController with a null unit.");
			yield break;
		}
		cutsceneData.Unit.UnitView.PlayDieAnim();
		if (PlayDeathAnimCutsceneDefinition.WaitDeathAnim)
		{
			yield return cutsceneData.Unit.UnitView.WaitUntilDeathCanBeFinalized;
		}
	}
}
