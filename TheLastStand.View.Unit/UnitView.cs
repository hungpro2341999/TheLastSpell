using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Controller;
using TheLastStand.Definition;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Sequencing;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Enemy.Affix;
using TheLastStand.View.Building;
using TheLastStand.View.Skill.SkillAction;
using TheLastStand.View.Skill.SkillAction.UI;
using TheLastStand.View.TileMap;
using TheLastStand.View.Unit.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheLastStand.View.Unit;

public abstract class UnitView : MonoBehaviour, IDamageableView, ITileObjectView
{
	public enum E_GaugeSize
	{
		Small,
		Large
	}

	public static class Constants
	{
		public static class Animation
		{
			public const string BackLayerName = "Back Layer";

			public const string FrontLayerName = "Front Layer";

			public const string AnimatorEnemyUnitFolderPath = "Animators/Units/EnemyUnits";

			public const string AnimatorPlayableUnitFolderPath = "Animators/Units/PlayableUnits/";

			public const string SkillCastAnimBackSuffix = "Back";

			public const string SkillCastAnimFrontSuffix = "Front";

			public const string SkillCastAnimRootFolderPath = "Animation/Caster Anims/";

			private const string SkillCastDefaultAnimClipBaseName = "Hero_CastSkill_Default_";

			public const string ParamBoolWalk = "Walk";

			public const string ParamTriggerCastSkill = "Cast Skill";

			public const string ParamTriggerDie = "Die";

			public const string ParamTriggerPrepareDie = "PrepareDie";

			public const string ParamTriggerTakeDamage = "Take Damage";

			public static readonly int AnimatorCastSkillStateHash;

			public static readonly int AnimatorDeadStateHash;

			public static readonly int AnimatorDeathStateHash;

			public static readonly int AnimatorIdleStateHash;

			public static readonly int AnimatorLevelUpStateHash;

			public static readonly int AnimatorPrepareDeathStateHash;

			public static readonly int AnimatorTakeDamageStateHash;

			public static readonly string SkillCastDefaultAnimClipBackName;

			public static readonly string SkillCastDefaultAnimClipFrontName;

			static Animation()
			{
				SkillCastDefaultAnimClipBackName = "Hero_CastSkill_Default_Back";
				SkillCastDefaultAnimClipFrontName = "Hero_CastSkill_Default_Front";
				AnimatorCastSkillStateHash = Animator.StringToHash("Cast Skill");
				AnimatorDeathStateHash = Animator.StringToHash("Death");
				AnimatorDeadStateHash = Animator.StringToHash("Dead");
				AnimatorIdleStateHash = Animator.StringToHash("Idle");
				AnimatorLevelUpStateHash = Animator.StringToHash("LevelUp");
				AnimatorPrepareDeathStateHash = Animator.StringToHash("Prepare Death");
				AnimatorTakeDamageStateHash = Animator.StringToHash("Take Damage");
			}
		}

		public static class Effects
		{
			public const string AttackFeedbackPrefabResourcePath = "Prefab/Displayable Effect/Attack Feedback";

			public const string GainArmorFeedbackPrefabResourcePath = "Prefab/Displayable Effect/Gain Armor Feedback";

			public const string HealFeedbackPrefabResourcePath = "Prefab/Displayable Effect/Heal Feedback";
		}
	}

	[SerializeField]
	[FormerlySerializedAs("bodyTransform")]
	protected Transform rootTransform;

	[SerializeField]
	private GameObject bodyFrontContainer;

	[SerializeField]
	private GameObject bodyBackContainer;

	[SerializeField]
	protected SpriteRenderer bodyFrontRenderer;

	[SerializeField]
	protected SpriteRenderer bodyBackRenderer;

	[SerializeField]
	private RendererEventsListener frontRendererEventsListener;

	[SerializeField]
	private RendererEventsListener backRendererEventsListener;

	[SerializeField]
	protected Animator animator;

	[SerializeField]
	private UnitHUD hudPrefab;

	[SerializeField]
	protected Transform hudFollowTarget;

	[SerializeField]
	protected GameObject damagedParticles;

	private float startZ;

	private TheLastStand.Model.Unit.Unit unit;

	private bool orientationIsFront = true;

	private int backLayerIndex = -1;

	private int frontLayerIndex = -1;

	private bool selected;

	protected bool hovered;

	protected AnimatorOverrideController animatorOverrideController;

	protected WaitUntil waitUntilAnimatorStateIsCastSkill;

	protected WaitUntil waitUntilAnimatorStateIsDie;

	protected WaitUntil waitUntilAnimatorStateIsDead;

	protected WaitUntil waitUntilAnimatorStateIsIdle;

	protected WaitUntil waitUntilAnimatorStateIsTakeDamage;

	private Coroutine displaySkillEffectsCoroutine;

	private Coroutine takeHitAnimCoroutine;

	private HealFeedback healFeedback;

	private GainArmorFeedback gainArmorFeedback;

	private List<Tile> repeledTilesForPreviousTile = new List<Tile>();

	private List<Tile> repeledTilesForCurrentTile = new List<Tile>();

	private int visibleSpritesCount;

	public Animator Animator => animator;

	public bool AreAnimationsInitialized { get; protected set; }

	public AttackFeedback AttackFeedback { get; protected set; }

	public SpriteRenderer BodyBackRenderer => bodyBackRenderer;

	public SpriteRenderer BodyFrontRenderer => bodyFrontRenderer;

	public IDamageable Damageable => Unit;

	public IDamageableHUD DamageableHUD => UnitHUD;

	public bool DieAnimationIsFinished { get; private set; }

	public GainArmorFeedback GainArmorFeedback
	{
		get
		{
			if (gainArmorFeedback == null)
			{
				gainArmorFeedback = Object.Instantiate(ResourcePooler.LoadOnce<GainArmorFeedback>("Prefab/Displayable Effect/Gain Armor Feedback"));
				gainArmorFeedback.Init(this);
			}
			return gainArmorFeedback;
		}
	}

	public GameObject GameObject => base.gameObject;

	public Transform HudFollowTarget => hudFollowTarget;

	public HealFeedback HealFeedback
	{
		get
		{
			if (healFeedback == null)
			{
				healFeedback = Object.Instantiate(ResourcePooler.LoadOnce<HealFeedback>("Prefab/Displayable Effect/Heal Feedback"));
				healFeedback.Init(this);
			}
			return healFeedback;
		}
	}

	public virtual bool Hovered
	{
		get
		{
			return hovered;
		}
		set
		{
			if (hovered != value)
			{
				hovered = value;
				RefreshCursorFeedback();
			}
		}
	}

	public bool HoveredOrSelected
	{
		get
		{
			if (!Hovered)
			{
				return Selected;
			}
			return true;
		}
	}

	public bool IsCastingSkill
	{
		get
		{
			if (animator != null)
			{
				return animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Constants.Animation.AnimatorCastSkillStateHash;
			}
			return false;
		}
	}

	public abstract float MoveSpeed { get; }

	public Transform OrientationRootTransform => rootTransform;

	public bool Selected
	{
		get
		{
			return selected;
		}
		set
		{
			if (selected == value)
			{
				return;
			}
			selected = value;
			if (TPSingleton<TileObjectSelectionManager>.Exist())
			{
				if (Selected)
				{
					TileObjectSelectionManager.SelectedUnitFeedback.Unit = Unit;
				}
				TileObjectSelectionManager.SelectedUnitFeedback.Display(Selected);
				RefreshCursorFeedback();
			}
		}
	}

	public List<IDisplayableEffect> SkillEffectDisplays { get; private set; } = new List<IDisplayableEffect>();

	public bool IsTakingDamage { get; set; }

	public virtual TheLastStand.Model.Unit.Unit Unit
	{
		get
		{
			return unit;
		}
		set
		{
			unit = value;
			if (UnitHUD != null)
			{
				UnitHUD.Unit = unit;
			}
		}
	}

	public UnitHUD UnitHUD { get; protected set; }

	public WaitUntil WaitUntilAnimatorStateIsIdle => waitUntilAnimatorStateIsIdle;

	public WaitUntil WaitUntilDeathCanBeFinalized => waitUntilAnimatorStateIsDead;

	public WaitUntil WaitUntilIsDying => waitUntilAnimatorStateIsDie;

	public void AddSkillEffectDisplay(IDisplayableEffect displayableEffect)
	{
		if (displayableEffect is EffectDisplay effectDisplay)
		{
			int num = effectDisplay.gameObject.name.IndexOf('-');
			string text = ((num < 1) ? effectDisplay.gameObject.name.RemoveFirstOccurrence("(Clone)") : effectDisplay.gameObject.name.Remove(num - 1));
			effectDisplay.name = text + " - " + base.name;
			effectDisplay.FollowElement.ChangeTarget(DamageableHUD.Transform);
		}
		SkillEffectDisplays.Add(displayableEffect);
	}

	public virtual void ClearWaitUntils()
	{
		AreAnimationsInitialized = false;
		waitUntilAnimatorStateIsCastSkill = null;
		waitUntilAnimatorStateIsDead = null;
		waitUntilAnimatorStateIsDie = null;
		waitUntilAnimatorStateIsIdle = null;
		waitUntilAnimatorStateIsTakeDamage = null;
	}

	public Task CreateMoveTask(bool followPathOrientation = true, float moveSpeed = -1f, float delay = 0f, bool isMovementInstant = false, bool isCompensate = false)
	{
		List<Tile> list = new List<Tile>();
		int i = 0;
		for (int count = Unit.Path.Count; i < count; i++)
		{
			if (i == 0 || i == count - 1 || !isMovementInstant)
			{
				list.Add(Unit.Path[i]);
			}
		}
		if (isMovementInstant)
		{
			moveSpeed = float.PositiveInfinity;
		}
		return new CoroutineTask(this, MoveUnitCoroutine(list, followPathOrientation, moveSpeed, delay));
	}

	public IEnumerator DisableWhenPossible()
	{
		if (Unit.HasBeenExiled && Unit.ExileForcePlayDieAnim)
		{
			yield return waitUntilAnimatorStateIsDead;
		}
		base.gameObject.SetActive(value: false);
	}

	public Coroutine DisplaySkillEffects(float delay)
	{
		if (displaySkillEffectsCoroutine != null)
		{
			return displaySkillEffectsCoroutine;
		}
		displaySkillEffectsCoroutine = TPSingleton<EffectManager>.Instance.StartCoroutine(DisplaySkillEffectsCoroutine(delay));
		return displaySkillEffectsCoroutine;
	}

	public virtual void ToggleSkillTargeting(bool show)
	{
		UnitHUD.ToggleSkillTargeting(show);
	}

	public virtual void InitVisuals(bool playSpawnAnim)
	{
		SetFrontAndBackActive(active: true);
		InitAndStartAnimations(playSpawnAnim);
		AttackFeedback = Object.Instantiate(ResourcePooler.LoadOnce<AttackFeedback>("Prefab/Displayable Effect/Attack Feedback"));
		AttackFeedback.Init(this);
	}

	public virtual void LookAtDirection(GameDefinition.E_Direction direction, bool updateViewAnimator = false)
	{
		switch (direction)
		{
		case GameDefinition.E_Direction.North:
			rootTransform.localScale = new Vector3(0f - Mathf.Abs(rootTransform.localScale.x), rootTransform.localScale.y, rootTransform.localScale.z);
			SetOrientation(front: false, forceRefresh: false, updateViewAnimator);
			break;
		case GameDefinition.E_Direction.South:
			rootTransform.localScale = new Vector3(Mathf.Abs(rootTransform.localScale.x), rootTransform.localScale.y, rootTransform.localScale.z);
			SetOrientation(front: true, forceRefresh: false, updateViewAnimator);
			break;
		case GameDefinition.E_Direction.East:
			rootTransform.localScale = new Vector3(Mathf.Abs(rootTransform.localScale.x), rootTransform.localScale.y, rootTransform.localScale.z);
			SetOrientation(front: false, forceRefresh: false, updateViewAnimator);
			break;
		case GameDefinition.E_Direction.West:
			rootTransform.localScale = new Vector3(0f - Mathf.Abs(rootTransform.localScale.x), rootTransform.localScale.y, rootTransform.localScale.z);
			SetOrientation(front: true, forceRefresh: false, updateViewAnimator);
			break;
		}
	}

	public void PlayDieAnim()
	{
		StartCoroutine(PlayDieAnimCoroutine());
	}

	public void PlaySkillCastAnim(SkillActionExecution skillExecution)
	{
		StartCoroutine(PlaySkillCastAnimCoroutine(skillExecution));
	}

	public void PlayTakeDamageAnim()
	{
		if (takeHitAnimCoroutine == null)
		{
			if (Unit.IsExecutingSkill)
			{
				IsTakingDamage = false;
			}
			else
			{
				takeHitAnimCoroutine = StartCoroutine(PlayTakeDamageAnimCoroutine());
			}
			if (string.IsNullOrEmpty(Unit.UnitTemplateDefinition.DamagedParticlesId))
			{
				Debug.Log(Unit.Id + " damaged particles has not been set in the definition, using default particles.");
			}
			GameObject pooledGameObject = ObjectPooler.GetPooledGameObject(string.IsNullOrEmpty(Unit.UnitTemplateDefinition.DamagedParticlesId) ? Unit.Id : Unit.UnitTemplateDefinition.DamagedParticlesId, string.IsNullOrEmpty(Unit.UnitTemplateDefinition.DamagedParticlesId) ? damagedParticles : null);
			if (pooledGameObject != null)
			{
				pooledGameObject.transform.position = TileMapView.GetTileCenter(Unit.OriginTile);
			}
			else
			{
				Unit.LogError("Unable to instantiate or get a pooled DamagedParticles for this unit. PoolName is : " + (string.IsNullOrEmpty(Unit.UnitTemplateDefinition.DamagedParticlesId) ? Unit.Id : Unit.UnitTemplateDefinition.DamagedParticlesId));
			}
		}
	}

	public void PlayWalkAnim(bool doWalk)
	{
		animator.enabled = true;
		animator.SetBool("Walk", doWalk);
	}

	[ContextMenu("Refresh Armor")]
	public void RefreshArmor()
	{
		UnitHUD?.RefreshArmor();
	}

	[ContextMenu("Refresh Health")]
	public void RefreshHealth()
	{
		UnitHUD?.RefreshHealth();
	}

	public abstract void RefreshCursorFeedback();

	public virtual void RefreshHud(UnitStatDefinition.E_Stat stat)
	{
		switch (stat)
		{
		case UnitStatDefinition.E_Stat.Health:
		case UnitStatDefinition.E_Stat.HealthTotal:
			RefreshHealth();
			break;
		case UnitStatDefinition.E_Stat.Armor:
		case UnitStatDefinition.E_Stat.ArmorTotal:
			RefreshArmor();
			break;
		}
	}

	public void RefreshHudPositionInstantly()
	{
		UnitHUD.RefreshPositionInstantly();
	}

	[ContextMenu("Refresh Injury Stage")]
	public void RefreshInjuryStage()
	{
		UnitHUD.RefreshInjuryStage();
	}

	[ContextMenu("Refresh Status")]
	public void RefreshStatus()
	{
		UnitHUD.RefreshStatuses();
	}

	public IEnumerator ToggleFollowElementOffWhenIdle()
	{
		yield return WaitUntilAnimatorStateIsIdle;
		UnitHUD.ToggleFollowElement(toggle: false);
	}

	public void OnSkillTargetHover(bool hover)
	{
		UnitHUD.OnSkillTargetHover(hover);
	}

	public virtual void SetFrontAndBackActive(bool active)
	{
		bodyFrontContainer.SetActive(active && orientationIsFront);
		bodyBackContainer.SetActive(active && !orientationIsFront);
	}

	public void UpdatePosition()
	{
		if (Unit.OriginTile.Building != null && Unit.OriginTile.Building.BuildingView is WatchtowerView watchtowerView)
		{
			base.transform.position = TileMapView.BuildingTilemap.CellToWorld(new Vector3Int(Unit.OriginTile.X, Unit.OriginTile.Y, 0)) + watchtowerView.TowerHeight.localPosition;
		}
		else if (Unit.OriginTile.Building != null)
		{
			base.transform.position = TileMapView.BuildingTilemap.CellToWorld(new Vector3Int(Unit.OriginTile.X, Unit.OriginTile.Y, 0)) - new Vector3(0.01f, 0.01f, 0.01f);
		}
		else
		{
			base.transform.position = TileMapView.BuildingTilemap.CellToWorld(new Vector3Int(Unit.OriginTile.X, Unit.OriginTile.Y, 0)) + Vector3.forward * startZ;
		}
	}

	protected virtual void DisableHUD()
	{
		Object.Destroy(UnitHUD.gameObject);
	}

	protected virtual bool InitAnimations()
	{
		AreAnimationsInitialized = false;
		if (animator == null)
		{
			CLoggerManager.Log("InitAnimations(): Need an animator for " + base.transform.name + "! Aborting", LogType.Error);
			return false;
		}
		if (!animator.isActiveAndEnabled)
		{
			return false;
		}
		InitAnimatorController(animator.runtimeAnimatorController);
		backLayerIndex = animator.GetLayerIndex("Back Layer");
		frontLayerIndex = animator.GetLayerIndex("Front Layer");
		SetOrientation(front: true, forceRefresh: true);
		animator.GetComponent<MecanimStartRandomFrame>()?.Execute();
		if (waitUntilAnimatorStateIsCastSkill == null)
		{
			waitUntilAnimatorStateIsCastSkill = new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Constants.Animation.AnimatorCastSkillStateHash);
		}
		if (waitUntilAnimatorStateIsDead == null)
		{
			waitUntilAnimatorStateIsDead = new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Constants.Animation.AnimatorDeadStateHash);
		}
		if (waitUntilAnimatorStateIsDie == null)
		{
			waitUntilAnimatorStateIsDie = new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Constants.Animation.AnimatorDeathStateHash);
		}
		if (waitUntilAnimatorStateIsIdle == null)
		{
			waitUntilAnimatorStateIsIdle = new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Constants.Animation.AnimatorIdleStateHash);
		}
		if (waitUntilAnimatorStateIsTakeDamage == null)
		{
			waitUntilAnimatorStateIsTakeDamage = new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Constants.Animation.AnimatorTakeDamageStateHash);
		}
		return true;
	}

	protected void InitAnimatorController(RuntimeAnimatorController newAnimatorController)
	{
		this.animatorOverrideController = new AnimatorOverrideController(newAnimatorController);
		this.animatorOverrideController.name = newAnimatorController.name;
		if (newAnimatorController is AnimatorOverrideController animatorOverrideController)
		{
			List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(animatorOverrideController.overridesCount);
			animatorOverrideController.GetOverrides(overrides);
			this.animatorOverrideController.ApplyOverrides(overrides);
		}
		animator.runtimeAnimatorController = this.animatorOverrideController;
	}

	protected virtual void InitAndStartAnimations(bool playSpawnAnim)
	{
		AreAnimationsInitialized = InitAnimations();
	}

	protected virtual void InitHud()
	{
		UnitHUD = Object.Instantiate(hudPrefab, PlayableUnitManager.UnitHudsTransform);
	}

	protected virtual void OnEnable()
	{
		startZ = base.transform.position.z;
		InitHud();
		IsTakingDamage = false;
		animator.keepAnimatorControllerStateOnDisable = true;
		if (frontRendererEventsListener != null)
		{
			frontRendererEventsListener.OnSpriteVisibilityToggle += OnSpriteVisibilityToggle;
		}
		if (backRendererEventsListener != null)
		{
			backRendererEventsListener.OnSpriteVisibilityToggle += OnSpriteVisibilityToggle;
		}
	}

	protected virtual void OnDisable()
	{
		HandleHudOnDisable();
		displaySkillEffectsCoroutine = null;
		DieAnimationIsFinished = false;
		animator?.ResetTrigger("Die");
		if (frontRendererEventsListener != null)
		{
			frontRendererEventsListener.OnSpriteVisibilityToggle -= OnSpriteVisibilityToggle;
		}
		if (backRendererEventsListener != null)
		{
			backRendererEventsListener.OnSpriteVisibilityToggle -= OnSpriteVisibilityToggle;
		}
		visibleSpritesCount = 0;
	}

	protected virtual IEnumerator PlayDieAnimCoroutine()
	{
		animator.enabled = true;
		animator.SetTrigger("Die");
		Unit.IsDying = true;
		yield return waitUntilAnimatorStateIsDie;
		yield return waitUntilAnimatorStateIsDead;
		Unit.IsDying = false;
		DieAnimationIsFinished = true;
	}

	protected void SetOrientation(bool front, bool forceRefresh = false, bool updateViewAnimator = false)
	{
		if (orientationIsFront != front || forceRefresh)
		{
			orientationIsFront = front;
			bodyFrontContainer.SetActive(orientationIsFront);
			animator.SetLayerWeight(frontLayerIndex, orientationIsFront ? 1 : 0);
			bodyBackContainer.SetActive(!orientationIsFront);
			animator.SetLayerWeight(backLayerIndex, (!orientationIsFront) ? 1 : 0);
			if (updateViewAnimator && Unit != null && Unit.UnitTemplateDefinition.UpdateAnimatorOnOrientationChange)
			{
				animator.Update(0f);
			}
		}
	}

	private void Awake()
	{
		animator.GetBehaviour<UnitStateMachine>()?.Init(this);
	}

	private IEnumerator DisplaySkillEffectsCoroutine(float delay)
	{
		if (SkillEffectDisplays.Count == 0)
		{
			displaySkillEffectsCoroutine = null;
			yield break;
		}
		int skillEffectDisplaysCount = SkillEffectDisplays.Count;
		if (delay > 0f)
		{
			yield return SharedYields.WaitForSeconds(delay);
		}
		while (skillEffectDisplaysCount > 0)
		{
			skillEffectDisplaysCount--;
			IDisplayableEffect displayableEffect = SkillEffectDisplays[0];
			SkillEffectDisplays.Remove(displayableEffect);
			yield return displayableEffect.Display();
		}
		displaySkillEffectsCoroutine = null;
	}

	private void HandleHudOnDisable()
	{
		if (UnitHUD != null)
		{
			if (SingletonBehaviour<ObjectPooler>.Instance != null)
			{
				UnitHUD.ReleaseSkillTargeting();
			}
			DisableHUD();
			UnitHUD = null;
		}
	}

	private IEnumerator MoveUnitCoroutine(List<Tile> path, bool followPathOrientation = true, float moveSpeed = -1f, float delay = 0f, bool isMovementInstant = false)
	{
		if (path == null || path.Count == 0)
		{
			yield break;
		}
		UnitHUD.ToggleFollowElement(toggle: true);
		bool isMisty = false;
		ILightFogSupplier lightFogSupplier = null;
		if (Unit is EnemyUnit enemyUnit && enemyUnit.HasLightFogSupplier(out lightFogSupplier))
		{
			lightFogSupplier.IsLightFogSupplierMoving = true;
			isMisty = true;
			lightFogSupplier.LightFogSupplierMoveDatas.StartTile = path[0];
			lightFogSupplier.LightFogSupplierMoveDatas.DestinationTile = path[path.Count - 1];
		}
		bool isPlayableUnit = Unit is PlayableUnit;
		if (delay > 0f)
		{
			yield return SharedYields.WaitForSeconds(delay);
		}
		if (moveSpeed == -1f)
		{
			moveSpeed = MoveSpeed;
		}
		bool previousTileHadAnyFog = path[0].HasAnyFog;
		int wayPointIndex = 1;
		while (wayPointIndex < path.Count)
		{
			Tile tile = path[wayPointIndex - 1];
			Tile tile2 = path[wayPointIndex];
			if (followPathOrientation)
			{
				Unit.UnitController.LookAt(tile2, tile, updateViewAnimator: true);
			}
			if (this is EnemyUnitView enemyUnitView && tile2.HasAnyFog != previousTileHadAnyFog)
			{
				enemyUnitView.RefreshMaterial();
				enemyUnitView.RefreshStatus();
				enemyUnitView.RefreshInjuryStage();
			}
			previousTileHadAnyFog = tile2.HasAnyFog;
			if ((tile2.Unit == null || tile2.Unit == Unit) && tile2.Building != null && tile2.Building.IsGate)
			{
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.ForceOpenGate(tile2.Building, tile2.Building.OriginTile);
			}
			if (tile.Unit == null && tile.Building != null && tile.Building.IsGate)
			{
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayBuilding(tile.Building, tile.Building.OriginTile);
			}
			if (isPlayableUnit)
			{
				repeledTilesForPreviousTile = FogManager.GetLightFogRepelTiles(tile);
				repeledTilesForCurrentTile = FogManager.GetLightFogRepelTiles(tile2);
				List<Tile> tiles = repeledTilesForPreviousTile.Except(repeledTilesForCurrentTile).ToList();
				Dictionary<Fog.LightFogTileInfo.E_LightFogMode, List<Tile>> tilesToUpdateByLightFogMode = FogController.ToggleLightFogTiles((from tile3 in repeledTilesForCurrentTile.Except(repeledTilesForPreviousTile).ToList()
					where tile3.HasLightFogOn
					select tile3).ToList());
				Dictionary<Fog.LightFogTileInfo.E_LightFogMode, List<Tile>> tilesToUpdateByLightFogMode2 = FogController.ToggleLightFogTiles(tiles);
				FogController.SetLightFogTilesFromDictionnary(tilesToUpdateByLightFogMode, FogManager.LightFogFadeInEaseAndDuration, FogManager.LightFogFadeOutEaseAndDuration, FogManager.LightFogDisappearEaseAndDuration, instant: false, independently: true);
				FogController.SetLightFogTilesFromDictionnary(tilesToUpdateByLightFogMode2, FogManager.LightFogFadeInEaseAndDuration, FogManager.LightFogFadeOutEaseAndDuration, FogManager.LightFogDisappearEaseAndDuration, instant: false, independently: true);
			}
			unit.Events.GetValueOrDefault(E_EffectTime.OnTileCrossed)?.Invoke(null);
			Vector3 targetPosition = TileMapView.BuildingTilemap.CellToWorld(new Vector3Int(tile2.X, tile2.Y, 0)) + Vector3.forward * startZ;
			base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, targetPosition.z);
			Vector3 positionBuffer = base.transform.position;
			if (isMisty)
			{
				(lightFogSupplier as EnemyMistyAffix)?.EnemyMistyAffixController.TriggerLightFogDuringMovement(tile, tile2);
			}
			do
			{
				positionBuffer = Vector3.MoveTowards(positionBuffer, targetPosition, moveSpeed * Time.deltaTime);
				base.transform.position = positionBuffer;
				yield return SharedYields.WaitForEndOfFrame;
			}
			while (!base.transform.position.IsApproxEqualVect3(targetPosition, 0.01f));
			if (path[path.Count - 1].Building != null)
			{
				base.transform.position = targetPosition - new Vector3(0.01f, 0.01f, 0.01f);
			}
			else
			{
				base.transform.position = targetPosition;
			}
			int num = wayPointIndex + 1;
			wayPointIndex = num;
		}
		if (isMisty)
		{
			lightFogSupplier.IsLightFogSupplierMoving = false;
			lightFogSupplier.LightFogSupplierMoveDatas.CurrentTile = null;
			lightFogSupplier.LightFogSupplierMoveDatas.StartTile = null;
			lightFogSupplier.LightFogSupplierMoveDatas.DestinationTile = null;
		}
		UnitHUD.ToggleFollowElement(toggle: false);
		unit.Events.GetValueOrDefault(E_EffectTime.OnMovementEnd)?.Invoke(null);
	}

	private void OnSpriteVisibilityToggle(bool toggle)
	{
		if (toggle)
		{
			if (++visibleSpritesCount >= 1)
			{
				animator.enabled = true;
			}
		}
		else if (--visibleSpritesCount == 0)
		{
			animator.enabled = Unit.IsExecutingSkill || animator.GetCurrentAnimatorStateInfo(0).shortNameHash != Constants.Animation.AnimatorIdleStateHash;
		}
	}

	private IEnumerator PlaySkillCastAnimCoroutine(SkillActionExecution skillExecution)
	{
		TheLastStand.Model.Skill.Skill skill = skillExecution.Skill;
		if (skill.SkillDefinition.SkillCastFxDefinition.CasterAnimDef == null || string.IsNullOrEmpty(skill.SkillDefinition.SkillCastFxDefinition.CasterAnimDef.Path))
		{
			yield break;
		}
		string text = "Animation/Caster Anims/" + skill.SkillDefinition.SkillCastFxDefinition.CasterAnimDef.Path;
		AnimationClip value = ResourcePooler.LoadOnce<AnimationClip>(text + "Front");
		animatorOverrideController[Constants.Animation.SkillCastDefaultAnimClipFrontName] = value;
		value = ResourcePooler.LoadOnce<AnimationClip>(text + "Back");
		animatorOverrideController[Constants.Animation.SkillCastDefaultAnimClipBackName] = value;
		float num = skill.SkillDefinition.SkillCastFxDefinition.CasterAnimDef.Delay.EvalToFloat(skillExecution.CastFx.CastFXInterpreterContext);
		if (num > 0f)
		{
			yield return SharedYields.WaitForSeconds(num);
		}
		animator.enabled = true;
		animator.SetTrigger("Cast Skill");
		yield return waitUntilAnimatorStateIsCastSkill;
		EnemyUnit enemyUnit = Unit as EnemyUnit;
		if (enemyUnit != null && enemyUnit.Id == "SpawnerCocoon" && skill.SkillAction is SpawnSkillAction)
		{
			int num2 = enemyUnit.CurrentVariantIndex + 1;
			if (((enemyUnit.EnemyUnitTemplateDefinition.VisualEvolutions.Count > num2) ? enemyUnit.EnemyUnitTemplateDefinition.VisualEvolutions[num2] : string.Empty) != string.Empty)
			{
				animator.SetTrigger("LevelUp");
			}
		}
		else
		{
			yield return waitUntilAnimatorStateIsIdle;
		}
		if (unit.OriginTile.Building != null && !unit.OriginTile.Building.IsWatchtower)
		{
			base.transform.position = Unit.OriginTile.TileView.transform.position;
		}
		if (unit.OriginTile.Building != null)
		{
			unit.UnitView.UpdatePosition();
		}
		if (enemyUnit != null)
		{
			ExileCasterEffectDefinition firstEffect = skill.SkillAction.GetFirstEffect<ExileCasterEffectDefinition>("ExileCaster");
			if (firstEffect != null && !firstEffect.ForcePlayDieAnim)
			{
				SetFrontAndBackActive(active: false);
				enemyUnit.OriginTile.TileController.AddDeadBody(enemyUnit);
			}
		}
	}

	private IEnumerator PlayTakeDamageAnimCoroutine()
	{
		animator.enabled = true;
		animator.SetTrigger("Take Damage");
		yield return waitUntilAnimatorStateIsTakeDamage;
		yield return waitUntilAnimatorStateIsIdle;
		takeHitAnimCoroutine = null;
		IsTakingDamage = false;
	}
}
