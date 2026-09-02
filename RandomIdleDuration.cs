using System.Collections;
using TPLib.Yield;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RandomIdleDuration : MonoBehaviour
{
	[SerializeField]
	private Vector2 randomWait = new Vector2(5f, 25f);

	[SerializeField]
	private Animator idleAnimator;

	[SerializeField]
	private string transitionTriggerName = string.Empty;

	public void Stop()
	{
		StopAllCoroutines();
	}

	public void PlayAnimation()
	{
		idleAnimator.SetTrigger(transitionTriggerName);
	}

	private void OnEnable()
	{
		StartCoroutine(StartRandomIdle());
	}

	private IEnumerator StartRandomIdle()
	{
		while (true)
		{
			yield return SharedYields.WaitForSeconds(Random.Range(randomWait.x, randomWait.y));
			PlayAnimation();
		}
	}
}
