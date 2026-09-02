using System.Collections;
using Com.LuisPedroFonseca.ProCamera2D;
using TPLib;
using TPLib.Yield;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.WorldMap;
using TheLastStand.View.Camera;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheLastStand.View.Building;

public class MagicCircleSealView : MonoBehaviour
{
	private static class AnimatorParameters
	{
		public const string VictoryAnticipation = "victory_anticipation";

		public const string VictoryTransition = "victory_transition";

		public const string VictoryAnticipationMultiplier = "anticipation_multiplier";
	}

	public static class Constants
	{
		public const string AnimationBaseMagicCirclePath = "Animation/MagicCircle/";

		public const string AnimationNamePrefix = "MagicCircle_";

		public const string AnimationNameSealPrefix = "MagicCircle_Seal_";

		public const string IdleSuffix = "Idle";

		public const string AnticipationSuffix = "Anticipation";

		public const string PillarIdleSuffix = "Pillar_Idle";
	}

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private Animator pillarAnimator;

	[SerializeField]
	private SortingGroup sortingGroup;

	[SerializeField]
	private SortingGroup[] mageSortingGroups;

	[SerializeField]
	private SpriteRenderer pillar;

	[SerializeField]
	private SpriteRenderer pillarFlash;

	[SerializeField]
	private int cutsceneSealOrder = 100;

	[SerializeField]
	private AudioSource magesCastStartAudioSource;

	[SerializeField]
	private AudioSource pillarAppearAudioSource;

	[SerializeField]
	private AudioSource pillarLoopAudioSource;

	[SerializeField]
	private AudioClip[] magesCastStartSfx;

	[SerializeField]
	private AudioClip[] pillarAppearSfx;

	[SerializeField]
	private AudioClip pillarLoopSfx;

	[SerializeField]
	private float delayBeforeAcceleration = 1f;

	[SerializeField]
	private float anticipationMultiplierMax = 2f;

	[SerializeField]
	private float anticipationMultiplierIncreasePerSec = 0.08f;

	private float anticipationMultiplier;

	private IEnumerator sealAccelerationCoroutine;

	public void InitAnimations()
	{
		AnimationClip animationClip = ResourcePooler.LoadOnce<AnimationClip>("Animation/MagicCircle/MagicCircle_Seal_Idle/MagicCircle_Seal_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "_Idle");
		AnimationClip animationClip2 = ResourcePooler.LoadOnce<AnimationClip>("Animation/MagicCircle/MagicCircle_Seal_Anticipation/MagicCircle_Seal_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "_Anticipation");
		if (!(animationClip == null) || !(animationClip2 == null))
		{
			AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
			if (animationClip != null)
			{
				animatorOverrideController["MagicCircle_Seal_Idle"] = animationClip;
			}
			if (animationClip2 != null)
			{
				animatorOverrideController["MagicCircle_Seal_Anticipation"] = animationClip2;
			}
			animator.runtimeAnimatorController = animatorOverrideController;
			InitPillarAnimations();
		}
	}

	public void InitPillarAnimations()
	{
		string text = ((!TPSingleton<CutsceneManager>.Exist() || string.IsNullOrEmpty(TPSingleton<CutsceneManager>.Instance.VictorySequenceView.debugCityIdOverride)) ? TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id : TPSingleton<CutsceneManager>.Instance.VictorySequenceView.debugCityIdOverride);
		AnimationClip animationClip = ResourcePooler.LoadOnce<AnimationClip>("Animation/MagicCircle/MagicCircle_Pillar_Idle/MagicCircle_" + text + "_Pillar_Idle");
		if (!(animationClip == null))
		{
			AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(pillarAnimator.runtimeAnimatorController);
			animatorOverrideController["MagicCircle_Pillar_Idle"] = animationClip;
			pillarAnimator.runtimeAnimatorController = animatorOverrideController;
		}
	}

	public void OnMagesCastStart()
	{
		BuildingManager.MagicCircle.MagicCircleView.PlayAllMagesCastAnimation();
		for (int i = 0; i < magesCastStartSfx.Length; i++)
		{
			if (magesCastStartSfx[i] != null)
			{
				SoundManager.PlayAudioClip(magesCastStartAudioSource, magesCastStartSfx[i]);
			}
		}
	}

	public void OnPillarAppearFrame()
	{
		ACameraView.Zoom(zoomIn: false);
		TPSingleton<BuildingManager>.Instance.StartCoroutine(BuildingManager.DestroyAll(4, 8f));
		CameraView.CameraVisualEffects.ToggleChromaticAberration(state: true);
		CameraView.CameraLutView.TogglePillarLut(state: true);
		for (int i = 0; i < pillarAppearSfx.Length; i++)
		{
			if (pillarAppearSfx[i] != null)
			{
				SoundManager.PlayAudioClip(pillarAppearAudioSource, pillarAppearSfx[i]);
			}
		}
		if (pillarLoopSfx != null)
		{
			SoundManager.PlayAudioClip(pillarLoopAudioSource, pillarLoopSfx, 0f, doNotInterrupt: true);
		}
		AdjustPillarHeight();
		pillar.enabled = true;
	}

	public void OnPulseFrame()
	{
	}

	public void OnInvertImage()
	{
		CameraView.CameraVisualEffects.InvertImage();
	}

	public void OnResetInvertImage()
	{
		CameraView.CameraVisualEffects.ResetInvertEffect();
	}

	public void OnPillarsCutsceneStart()
	{
		pillarLoopAudioSource.Stop();
	}

	public void SetSortingOrder(int order)
	{
		sortingGroup.sortingOrder = order;
		for (int i = 0; i < mageSortingGroups.Length; i++)
		{
			mageSortingGroups[i].sortingOrder = order;
		}
	}

	public void PlaySealAnticipationAnimation()
	{
		anticipationMultiplier = 1f;
		animator.SetFloat("anticipation_multiplier", anticipationMultiplier);
		sealAccelerationCoroutine = AccelerateSealOverTime();
		StartCoroutine(sealAccelerationCoroutine);
		animator.SetTrigger("victory_anticipation");
	}

	public IEnumerator PlaySealTransitionAnimation()
	{
		StopCoroutine(sealAccelerationCoroutine);
		SetSortingOrder(cutsceneSealOrder);
		animator.SetTrigger("victory_transition");
		yield return new WaitUntil(() => pillar.enabled);
	}

	private IEnumerator AccelerateSealOverTime()
	{
		yield return SharedYields.WaitForSeconds(delayBeforeAcceleration);
		while (true)
		{
			anticipationMultiplier = Mathf.Clamp(anticipationMultiplier + anticipationMultiplierIncreasePerSec * Time.deltaTime, 0f, anticipationMultiplierMax);
			animator.SetFloat("anticipation_multiplier", anticipationMultiplier);
			yield return null;
		}
	}

	private void Awake()
	{
		pillar.enabled = false;
	}

	private void AdjustPillarHeight()
	{
		ProCamera2DNumericBoundaries proCamera2DNumericBoundaries = CameraView.ProCamera2DNumericBoundaries;
		Vector2 size = pillar.size;
		Vector2 size2 = new Vector2(size.x, proCamera2DNumericBoundaries.TopBoundary);
		pillar.size = size2;
		pillarFlash.size = new Vector2(proCamera2DNumericBoundaries.RightBoundary - proCamera2DNumericBoundaries.LeftBoundary, proCamera2DNumericBoundaries.TopBoundary - proCamera2DNumericBoundaries.BottomBoundary);
		float num = size2.y - size.y;
		Vector2 vector = pillar.sprite.pivot / pillar.sprite.rect.size.y;
		pillar.transform.position += new Vector3(0f, num * vector.y);
	}
}
