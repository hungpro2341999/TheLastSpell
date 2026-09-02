using TPLib;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Cutscene;

public class PillarsCutsceneIdleBehaviour : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		TPSingleton<CutsceneManager>.Instance.PillarsCutsceneView.animatorIdleStateEnter = true;
	}
}
