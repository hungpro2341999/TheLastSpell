using System.Collections;
using Coffee.UIExtensions;
using DG.Tweening;
using TPLib;
using TheLastStand.DRM.Achievements;
using TheLastStand.Manager.Achievements;
using UnityEngine;

namespace TheLastStand.View.MetaShops;

public class GoddessView : MonoBehaviour
{
	private static class Constants
	{
		public const string IdleAnimatorParameterName = "Idle";
	}

	[Tooltip("Transform containing all the goddess visuals (sprites and particles).")]
	[SerializeField]
	private RectTransform goddessScaler;

	[SerializeField]
	private GameObject fadeInContainer;

	[SerializeField]
	private GameObject idleContainer;

	[SerializeField]
	private RectTransform goddessContainer;

	[Tooltip("Idle animator to switch between visual evolutions.")]
	[SerializeField]
	private Animator goddessAnimator;

	[Tooltip("Script attached to animator responsible of goddess appearance.")]
	[SerializeField]
	private GoddessAnimatorEventHandler goddessAnimatorEventHandler;

	[Tooltip("Script attached to animator responsible of goddess appearance.")]
	[SerializeField]
	private UIParticle[] particles;

	[Tooltip("Offset applied to goddess when being offset to show replicas (in screen space). Should be negative for one of the goddesses.")]
	[SerializeField]
	private float goddessOffset = 300f;

	[Tooltip("Duration the goddess takes to go from its initial position to the offset position.")]
	[SerializeField]
	private float goddessOffsetDuration = 0.3f;

	[Tooltip("Curve the goddess uses to go from its initial position to the offset position.")]
	[SerializeField]
	private Ease goddessOffsetEase = Ease.OutCirc;

	public int CurrentEvolutionIndex { get; private set; }

	public GameObject IdleContainer => idleContainer;

	public GameObject FadeInContainer => fadeInContainer;

	public float GoddessOffset => goddessOffset;

	public void ChangeEvolution(int index)
	{
		CurrentEvolutionIndex = index;
		goddessAnimator.SetTrigger(string.Format("{0}{1}", "Idle", CurrentEvolutionIndex));
		if (index > 0)
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(TPSingleton<OraculumView>.Instance.IsInLightShop ? AchievementContainer.ACH_FREUDE_REVEAL : AchievementContainer.ACH_SCHADEN_REVEAL);
		}
	}

	public void OffsetAfterGreeting(bool instantly = false)
	{
		if (instantly)
		{
			SetPositionX(GoddessOffset);
			return;
		}
		DOTween.To(() => 0f, SetPositionX, GoddessOffset, goddessOffsetDuration).SetEase(goddessOffsetEase);
	}

	public IEnumerator PlayVisualAnimationAtIndexCoroutine(int index)
	{
		if (goddessAnimator.isActiveAndEnabled)
		{
			goddessAnimatorEventHandler.AppearFrame = false;
			yield return new WaitUntil(() => goddessAnimatorEventHandler.AppearFrame);
			goddessAnimatorEventHandler.AppearFrame = false;
		}
		yield return new WaitUntil(() => goddessAnimator.isActiveAndEnabled);
		ChangeEvolution(index);
	}

	public void Refresh()
	{
		goddessScaler.localScale = Vector3.one * ((Screen.height > 768) ? 2f : 1f);
		for (int num = particles.Length - 1; num >= 0; num--)
		{
			particles[num].scale = (float)Screen.height / 1080f;
		}
	}

	public void SetPositionX(float x)
	{
		goddessScaler.anchoredPosition = new Vector2(x, goddessScaler.anchoredPosition.y);
	}

	private void Awake()
	{
		goddessAnimator.keepAnimatorControllerStateOnDisable = true;
	}

	[ContextMenu("Locate Particles In Children")]
	private void LocateParticlesInChildren()
	{
		particles = goddessContainer.GetComponentsInChildren<UIParticle>();
	}
}
