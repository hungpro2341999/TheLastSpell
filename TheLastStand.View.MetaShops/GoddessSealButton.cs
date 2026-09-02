using DG.Tweening;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.Manager.Sound;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.MetaShops;

public class GoddessSealButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler
{
	public class Constants
	{
		public const string HoveredAnimatorParameterName = "Hovered";
	}

	private Tween fadeTween;

	private Tween fadeTweenLoop;

	[SerializeField]
	private Animator sealAnimator;

	[SerializeField]
	[Tooltip("If not checked, target shop is light one.")]
	private bool isDarkShop;

	[SerializeField]
	private float fadeInDuration = 0.2f;

	[SerializeField]
	private float fadeOutDuration = 0.2f;

	public bool IsDarkShop => isDarkShop;

	public bool Hovered { get; private set; }

	public void DeactivateSeal()
	{
		Hovered = false;
		sealAnimator.SetBool("Hovered", value: false);
		if (isDarkShop)
		{
			SoundManager.FadeOutAudioSource(MetaShopsManager.AudioSource, ref fadeTween, fadeOutDuration);
			SoundManager.FadeOutAudioSource(MetaShopsManager.AudioSourceLoop, ref fadeTweenLoop, fadeOutDuration);
		}
		else
		{
			SoundManager.FadeOutAudioSource(MetaShopsManager.AudioSourceSecondary, ref fadeTween, fadeOutDuration);
			SoundManager.FadeOutAudioSource(MetaShopsManager.AudioSourceSecondaryLoop, ref fadeTweenLoop, fadeOutDuration);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		TPSingleton<OraculumView>.Instance.StartCoroutine(TPSingleton<OraculumView>.Instance.TransitionToShop(isDarkShop));
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Hovered = true;
		sealAnimator.SetBool("Hovered", value: true);
		if (isDarkShop)
		{
			SoundManager.PlayFadeInAudioClip(MetaShopsManager.AudioSource, ref fadeTween, MetaShopsManager.HoverAudioClip, fadeInDuration);
			SoundManager.PlayFadeInAudioClip(MetaShopsManager.AudioSourceLoop, ref fadeTweenLoop, MetaShopsManager.DarkHoverLoopAudioClip, fadeInDuration);
		}
		else
		{
			SoundManager.PlayFadeInAudioClip(MetaShopsManager.AudioSourceSecondary, ref fadeTween, MetaShopsManager.HoverAudioClip, fadeInDuration);
			SoundManager.PlayFadeInAudioClip(MetaShopsManager.AudioSourceSecondaryLoop, ref fadeTweenLoop, MetaShopsManager.LightHoverLoopAudioClip, fadeInDuration);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		DeactivateSeal();
	}

	private void OnDisable()
	{
		fadeTween?.Kill();
		fadeTweenLoop?.Kill();
		if (TPSingleton<MetaShopsManager>.Exist())
		{
			MetaShopsManager.AudioSource?.Stop();
			MetaShopsManager.AudioSourceLoop?.Stop();
		}
	}
}
