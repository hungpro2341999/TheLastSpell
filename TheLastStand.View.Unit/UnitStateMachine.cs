using UnityEngine;

namespace TheLastStand.View.Unit;

public class UnitStateMachine : StateMachineBehaviour
{
	private UnitView unitView;

	public void Init(UnitView newUnitView)
	{
		unitView = newUnitView;
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (!(unitView == null))
		{
			unitView.Animator.enabled = unitView.BodyFrontRenderer.isVisible || unitView.BodyBackRenderer.isVisible;
		}
	}
}
