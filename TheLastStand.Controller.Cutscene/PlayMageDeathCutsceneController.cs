using System.Collections;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager.Building;
using TheLastStand.View.Cutscene;
using UnityEngine;

namespace TheLastStand.Controller.Cutscene;

public class PlayMageDeathCutsceneController : CutsceneController
{
	public PlayMageDeathCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		if (BuildingManager.MagicCircle.MagicCircleView.MageRenderers.Length > 1)
		{
			GameObject gameObject = BuildingManager.MagicCircle.MagicCircleView.MageRenderers[BuildingManager.MagicCircle.MageCount - 1].gameObject;
			yield return BuildingManager.MagicCircle.MagicCircleView.TriggerMageDeathAnimation(gameObject, instant: true);
			BuildingManager.MagicCircle.MageCount--;
		}
	}
}
