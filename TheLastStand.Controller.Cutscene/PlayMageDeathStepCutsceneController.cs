using System.Collections;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager.Building;
using TheLastStand.View.Cutscene;
using UnityEngine;

namespace TheLastStand.Controller.Cutscene;

public class PlayMageDeathStepCutsceneController : CutsceneController
{
	public PlayMageDeathStepCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		if (BuildingManager.MagicCircle.MagicCircleView.MageRenderers.Length > 1)
		{
			Animator component = BuildingManager.MagicCircle.MagicCircleView.MageRenderers[BuildingManager.MagicCircle.MageCount - 1].gameObject.GetComponent<Animator>();
			int integer = component.GetInteger("death_step");
			component.SetInteger("death_step", integer + 1);
			if (integer + 1 == 2)
			{
				BuildingManager.MagicCircle.MageCount--;
			}
		}
		yield break;
	}
}
