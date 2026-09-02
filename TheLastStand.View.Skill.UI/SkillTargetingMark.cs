using UnityEngine;
using UnityEngine.Events;

namespace TheLastStand.View.Skill.UI;

public class SkillTargetingMark : MonoBehaviour
{
	[SerializeField]
	private UnityEvent onHide = new UnityEvent();

	[SerializeField]
	private UnityEvent onShow = new UnityEvent();

	[SerializeField]
	private Animator skillTargetingAnimator;

	public UnityEvent OnHide => onHide;

	public UnityEvent OnShow => onShow;

	public void SetHoverAnimatorState(bool state)
	{
		skillTargetingAnimator.SetBool("hover", state);
	}
}
