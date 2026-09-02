using System;
using TPLib;
using UnityEngine;

namespace TheLastStand.View.WorldMap.Apocalypse;

public class ApocalypseFlameKindleFeedback : MonoBehaviour
{
	private static class Constants
	{
		public static readonly int PlayFeedback = Animator.StringToHash("PlayFeedback");
	}

	[SerializeField]
	private Animator feedbackAnimator;

	[SerializeField]
	private bool isBigFlame = true;

	public event Action OnFlameKindled = delegate
	{
	};

	public event Action OnStartKindlingFlame = delegate
	{
	};

	public void ContinueAnimations()
	{
		feedbackAnimator.speed = 1f;
	}

	public void KindleFlame()
	{
		this.OnFlameKindled?.Invoke();
	}

	public void PauseAnimations()
	{
		feedbackAnimator.speed = 0f;
	}

	public void PlayFeedback()
	{
		feedbackAnimator.SetTrigger(Constants.PlayFeedback);
	}

	public void StartKindlingFlame()
	{
		PlayKindleSound();
		this.OnStartKindlingFlame?.Invoke();
	}

	private void PlayKindleSound()
	{
		if (!(TPSingleton<ApocalypseSelectionPanel>.Instance == null))
		{
			if (isBigFlame)
			{
				TPSingleton<ApocalypseSelectionPanel>.Instance.PlayBigFlameSfx();
			}
			else
			{
				TPSingleton<ApocalypseSelectionPanel>.Instance.PlaySmallFlameSfx();
			}
		}
	}
}
