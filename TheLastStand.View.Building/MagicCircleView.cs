using System;
using System.Collections;
using Com.LuisPedroFonseca.ProCamera2D;
using TPLib;
using TPLib.Yield;
using TheLastStand.Controller;
using TheLastStand.Controller.Settings;
using TheLastStand.Database.WorldMap;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.SDK;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.View.Building.UI;
using TheLastStand.View.Camera;
using TheLastStand.View.Cursor;
using TheLastStand.View.Seer;
using TheLastStand.View.Sound;
using TheLastStand.View.ToDoList;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheLastStand.View.Building;

public class MagicCircleView : BuildingView
{
	public static class AnimatorParameters
	{
		public const string MageDeathSteps = "death_step";

		public const string MageDeath = "is_dead";

		public const string MageRevive = "revive";

		public const string MagicCircleDestroyed = "is_destroyed";

		public const string MagicCircleIdle = "idle";

		public const string MagicCircleTakeDamage = "take_damage";
	}

	public new static class Constants
	{
		public const string AnimationBaseMagicCirclePath = "Animation/MagicCircle/";

		public const string AnimationNamePrefix = "MagicCircle_";

		public const string IdleSuffix = "Idle";

		public const string HitSuffix = "Hit";

		public const string DestructionSuffix = "Destruction";

		public const string IdleDestroyedSuffix = "IdleDestroyed";

		public const string SpriteBaseMagicCirclePath = "View/Tiles/Buildings/Diffuse/MagicCircle/";

		public const string SpriteBaseNamePrefix = "MagicCircle_Base_";

		public const int MaxMages = 4;
	}

	[SerializeField]
	private float fadeDuration;

	[SerializeField]
	private AnimationCurve fadeCurve;

	[SerializeField]
	private float gameOverShakeDuration;

	[SerializeField]
	private AnimationCurve gameOverShakeCurve;

	[SerializeField]
	private int gameOverShakeIntensityMultiplier = 1;

	[SerializeField]
	private float victoryShakeDuration;

	[SerializeField]
	private AnimationCurve victoryShakeCurve;

	[SerializeField]
	private float victoryShakeIntensityMultiplier = 1f;

	[SerializeField]
	private float victoryExplosionShake = 3.3f;

	[SerializeField]
	private float pillarShakeIntensity = 0.25f;

	[SerializeField]
	private int pillarShakeVibrato = 50;

	[SerializeField]
	private RandomIdleDuration[] magesAnimators;

	[SerializeField]
	private ShakePreset onHitShake;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private MagicCircleBaseView baseView;

	[SerializeField]
	private MagicCircleSealView sealView;

	[SerializeField]
	private SpriteRenderer[] idleRenderers;

	[SerializeField]
	private SpriteRenderer[] hitRenderers;

	[SerializeField]
	[FormerlySerializedAs("mages")]
	private SpriteRenderer[] mageRenderers;

	[SerializeField]
	private float timeIntervalBetweenIdleAnimations = 5f;

	[SerializeField]
	private OneShotSound sfxPrefab;

	[SerializeField]
	private Animator firstMageAnimator;

	[SerializeField]
	private RuntimeAnimatorController commanderAnimator;

	[SerializeField]
	private RuntimeAnimatorController commanderEyeOfTheLawAnimator;

	[SerializeField]
	private SpriteRenderer firstMageSpriteRenderer;

	[SerializeField]
	private Material commanderLutMaterial;

	private int destructionStage;

	private Action[] onDestructionStages = new Action[3];

	private IEnumerator idleCoroutine;

	private IEnumerator shakeCoroutine;

	public bool Dirty { get; set; }

	public SpriteRenderer[] MageRenderers => mageRenderers;

	public MagicCircleHUD MagicCircleHUD => base.BuildingHUD as MagicCircleHUD;

	public MagicCircleSealView SealView => sealView;

	public OneShotSound SFXPrefab => sfxPrefab;

	public override void InitVisuals()
	{
		base.InitVisuals();
		for (int i = 0; i < mageRenderers.Length; i++)
		{
			mageRenderers[i].gameObject.SetActive(value: false);
		}
		if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.UseCommanderAsMage)
		{
			firstMageAnimator.runtimeAnimatorController = commanderAnimator;
			firstMageSpriteRenderer.material = commanderLutMaterial;
		}
		InitAnimations();
		sealView.InitAnimations();
		baseView.InitAnimations();
		RefreshSlotsAndMagesQuantity();
		idleCoroutine = DisplayIdleAnimation();
		TPSingleton<GameManager>.Instance.StartCoroutine(idleCoroutine);
		MagicCircleHUD.ProductionPanelMagicCircle?.UnitsGauge.SetUnitsCount(CityDatabase.CityDefinitions[TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id].VictoryDaysCount);
		Dirty = true;
	}

	public void InitAnimations()
	{
		AnimationClip animationClip = ResourcePooler.LoadOnce<AnimationClip>("Animation/MagicCircle/MagicCircle_Idle/MagicCircle_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "_Idle");
		AnimationClip animationClip2 = ResourcePooler.LoadOnce<AnimationClip>("Animation/MagicCircle/MagicCircle_Hit/MagicCircle_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "_Hit");
		AnimationClip animationClip3 = ResourcePooler.LoadOnce<AnimationClip>("Animation/MagicCircle/MagicCircle_Destruction/MagicCircle_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "_Destruction", failSilently: true);
		AnimationClip animationClip4 = ResourcePooler.LoadOnce<AnimationClip>("Animation/MagicCircle/MagicCircle_IdleDestroyed/MagicCircle_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "_IdleDestroyed", failSilently: true);
		AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
		if (animationClip != null)
		{
			animatorOverrideController["MagicCircle_Idle"] = animationClip;
		}
		if (animationClip2 != null)
		{
			animatorOverrideController["MagicCircle_Hit"] = animationClip2;
		}
		if (animationClip3 != null)
		{
			animatorOverrideController["MagicCircle_Destruction"] = animationClip3;
		}
		if (animationClip4 != null)
		{
			animatorOverrideController["MagicCircle_IdleDestroyed"] = animationClip4;
		}
		animator.runtimeAnimatorController = animatorOverrideController;
	}

	public void NextDestructionStage()
	{
		if (destructionStage < onDestructionStages.Length)
		{
			onDestructionStages[destructionStage]();
		}
		destructionStage++;
	}

	public override void PlayDieAnim()
	{
		SettingsController.ToggleGameSpeed(isOn: false);
		TPSingleton<SoundManager>.Instance.StopMusic();
		TPSingleton<LightningSDKManager>.Instance.TransitionToColor(Color.black);
		NightTurnsManager.ForceStopTurnExecution();
		TileObjectSelectionManager.DeselectAll();
		CursorView.ClearTiles();
		TPSingleton<SeerPreviewDisplay>.Instance.Displayed = false;
		TPSingleton<ToDoListView>.Instance.Hide();
		GameView.TopScreenPanel.Display(show: false);
		PanicManager.Panic.PanicView.Hide();
		StartCoroutine(TPSingleton<UIManager>.Instance.ToggleUICoroutine());
		SpawnWaveManager.SpawnWaveView.Refresh();
		for (int num = TPSingleton<BuildingManager>.Instance.Buildings.Count - 1; num >= 0; num--)
		{
			TPSingleton<BuildingManager>.Instance.Buildings[num].BuildingView.BuildingHUD.DisplayHealthIfNeeded();
		}
		ObjectPooler.GetPooledComponent("HitsSFX", TPSingleton<PlayableUnitManager>.Instance.HitSFXPrefab).Play(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.IsTutorialMap ? GameManager.TutorialDefeatAudioClip : GameManager.DefeatAudioClip);
		StartCoroutine(StartDestructionAnimation());
	}

	public override void PlayTakeDamageAnim()
	{
		base.PlayTakeDamageAnim();
		ToggleHitRenderers(isEnabled: true);
		ACameraView.Shake(onHitShake.name);
		animator.SetTrigger("take_damage");
		RefreshSlotsAndMagesQuantity();
	}

	public void PlayAllMagesCastAnimation()
	{
		for (int num = magesAnimators.Length - 1; num >= 0; num--)
		{
			magesAnimators[num].PlayAnimation();
		}
	}

	public void PlayCommanderFadeOutAnim()
	{
		firstMageAnimator.Play("Commander_EyeOfLaw_FadeOut");
	}

	public void ReplaceCommanderView()
	{
		firstMageAnimator.runtimeAnimatorController = commanderEyeOfTheLawAnimator;
	}

	public void StopIdleAnimation()
	{
		TPSingleton<GameManager>.Instance.StopCoroutine(idleCoroutine);
		for (int num = magesAnimators.Length - 1; num >= 0; num--)
		{
			magesAnimators[num].Stop();
		}
	}

	protected override void InitHud()
	{
		base.BuildingHUD = UnityEngine.Object.Instantiate(hudPrefab, BuildingManager.BuildingsHudsTransform);
	}

	private IEnumerator DisplayIdleAnimation()
	{
		while (true)
		{
			yield return SharedYields.WaitForSeconds(timeIntervalBetweenIdleAnimations);
			if ((ApplicationManager.Application.State.GetName() == "LevelEditor" && BuildingManager.MagicCircle != base.BuildingController.Building) || base.BuildingController.Building.DamageableModule.IsDead)
			{
				break;
			}
			ToggleIdleRenderers(isEnabled: true);
			animator.SetTrigger("idle");
		}
	}

	private void RefreshSlotsAndMagesQuantity()
	{
		for (int i = 0; i < mageRenderers.Length; i++)
		{
			if (i >= BuildingManager.MagicCircle.MageCount && mageRenderers[i].enabled)
			{
				StartCoroutine(TriggerMageDeathAnimation(mageRenderers[i].gameObject));
			}
			else if (i < BuildingManager.MagicCircle.MageCount && (!mageRenderers[i].gameObject.activeSelf || !mageRenderers[i].enabled))
			{
				mageRenderers[i].gameObject.SetActive(value: true);
				mageRenderers[i].GetComponent<Animator>().SetTrigger("revive");
			}
		}
		baseView.RefreshAnimationBaseWithMagesQuantity(BuildingManager.MagicCircle.MageSlots);
	}

	private void Start()
	{
		ref Action reference = ref onDestructionStages[0];
		reference = (Action)Delegate.Combine(reference, (Action)delegate
		{
			StartCoroutine(GameOverShakeCoroutine());
		});
		ref Action reference2 = ref onDestructionStages[1];
		reference2 = (Action)Delegate.Combine(reference2, (Action)delegate
		{
			StartCoroutine(FadeCoroutine());
		});
		ref Action reference3 = ref onDestructionStages[1];
		reference3 = (Action)Delegate.Combine(reference3, (Action)delegate
		{
			StartCoroutine(BuildingManager.DestroyAll(BuildingManager.MagicCircleExplosionForce));
		});
		ref Action reference4 = ref onDestructionStages[2];
		reference4 = (Action)Delegate.Combine(reference4, (Action)delegate
		{
			StartCoroutine(DisplayGameOverScreen());
		});
	}

	private void ToggleHitRenderers(bool isEnabled = false, bool reciprocate = true)
	{
		for (int num = hitRenderers.Length - 1; num >= 0; num--)
		{
			hitRenderers[num].enabled = isEnabled;
		}
		if (reciprocate)
		{
			ToggleIdleRenderers(!isEnabled, reciprocate: false);
		}
	}

	private void ToggleIdleRenderers(bool isEnabled = false, bool reciprocate = true)
	{
		for (int num = idleRenderers.Length - 1; num >= 0; num--)
		{
			idleRenderers[num].enabled = isEnabled;
		}
		if (reciprocate)
		{
			ToggleHitRenderers(!isEnabled, reciprocate: false);
		}
	}

	public IEnumerator TriggerMageDeathAnimation(GameObject mage, bool instant = false)
	{
		if (!instant)
		{
			yield return SharedYields.WaitForSeconds(UnityEngine.Random.Range(0.6f, 1.6f));
		}
		mage.GetComponent<Animator>().SetTrigger("is_dead");
	}

	private void Update()
	{
		if (Dirty && BuildingManager.MagicCircle != null)
		{
			RefreshSlotsAndMagesQuantity();
			Dirty = false;
		}
	}

	private IEnumerator StartDestructionAnimation()
	{
		yield return SharedYields.WaitForSeconds(TPSingleton<CutsceneManager>.Instance.TutorialSequenceView.IsPlaying ? 0.5f : 5f);
		animator.SetTrigger("is_destroyed");
		baseView.DisableAnimator();
	}

	private IEnumerator FadeCoroutine()
	{
		float timeSpent = 0f;
		while (timeSpent <= fadeDuration)
		{
			float num = fadeCurve.Evaluate(timeSpent / fadeDuration);
			TPSingleton<LightningSDKManager>.Instance.SetAllLightsToColor(Color.white * num);
			UIManager.WhiteScreen.alpha = num;
			timeSpent += Time.deltaTime;
			yield return SharedYields.WaitForEndOfFrame;
		}
	}

	private IEnumerator DisplayGameOverScreen()
	{
		ACameraView.StopShaking();
		yield return SharedYields.WaitForSeconds(2f);
		GameController.TriggerGameOver(Game.E_GameOverCause.MagicCircleDestroyed);
	}

	private IEnumerator GameOverShakeCoroutine()
	{
		float timeSpent = 0f;
		while (timeSpent <= gameOverShakeDuration)
		{
			ACameraView.Shake(0.4f, Vector2.one * gameOverShakeIntensityMultiplier * gameOverShakeCurve.Evaluate(timeSpent / gameOverShakeDuration), 10, 0.5f, 0f, Vector3.zero, 0.05f);
			timeSpent += 0.2f;
			yield return SharedYields.WaitForSeconds(0.2f);
		}
	}

	public void PlaySealAnticipationAnimation()
	{
		sealView.PlaySealAnticipationAnimation();
		shakeCoroutine = SealAnticipationShakeCoroutine();
		StartCoroutine(shakeCoroutine);
	}

	public IEnumerator PlaySealTransitionAnimation()
	{
		yield return sealView.PlaySealTransitionAnimation();
		StopShakeCoroutine();
		CameraView.RippleEffect.RippleAtWorldPosition(base.transform.position + new Vector3(0f, 2.5f));
		ACameraView.Shake(0.5f, Vector2.one * victoryExplosionShake, 10, 0.5f, 0f, Vector3.zero, 0.05f);
		shakeCoroutine = PillarShakeCoroutine();
		StartCoroutine(shakeCoroutine);
	}

	public void StopShakeCoroutine()
	{
		if (shakeCoroutine != null)
		{
			StopCoroutine(shakeCoroutine);
			ACameraView.StopShaking();
		}
	}

	private IEnumerator SealAnticipationShakeCoroutine()
	{
		float timeSpent = 0f;
		while (timeSpent <= victoryShakeDuration)
		{
			ACameraView.Shake(0.4f, Vector2.one * victoryShakeIntensityMultiplier * victoryShakeCurve.Evaluate(timeSpent / victoryShakeDuration), 10, 0.5f, 0f, Vector3.zero, 0.05f);
			timeSpent += 0.2f;
			yield return SharedYields.WaitForSeconds(0.2f);
		}
	}

	private IEnumerator PillarShakeCoroutine()
	{
		while (true)
		{
			ACameraView.Shake(1f, Vector2.one * pillarShakeIntensity, pillarShakeVibrato, 0.2f, 0f, Vector3.zero, 0.05f);
			yield return null;
		}
	}
}
