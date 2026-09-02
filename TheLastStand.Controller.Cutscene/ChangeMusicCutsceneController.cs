using System.Collections;
using TPLib;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager.Sound;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public class ChangeMusicCutsceneController : CutsceneController
{
	public ChangeMusicCutsceneDefinition ChangeMusicCutsceneDefinition => base.CutsceneDefinition as ChangeMusicCutsceneDefinition;

	public ChangeMusicCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		TPSingleton<SoundManager>.Instance.ChangeMusic(ChangeMusicCutsceneDefinition.Instant);
		yield break;
	}
}
