using System.Collections;
using System.Collections.Generic;
using TPLib;
using TheLastStand.Definition;
using TheLastStand.Definition.Skill;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.TileMap;
using TheLastStand.View.Unit.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

namespace TheLastStand.View.Unit;

public class EnemyUnitView : UnitView
{
	public enum E_EnemyHudType
	{
		Small,
		Large,
		BossSmall,
		BossLarge
	}

	public new static class Constants
	{
		public static class Sprites
		{
			public const string SpritesFolderPath = "View/Sprites/Units/";

			public const string BossSpritesFolderName = "BossUnits";

			public const string SpritesFolderName = "EnemyUnits";

			public const string DeadBodiesFolderName = "DeadBodies";

			public const string DefaultSpritesFolderName = "DefaultSprites";

			public const string PortraitPathPrefix = "View/Sprites/UI/Units/Portaits/Enemy Unit/EnemiesPortrait_";
		}

		public static readonly int AnimatorSpawnStateHash = Animator.StringToHash("Spawn");

		public const int DefaultSortingOrder = 30;

		public const string IdleSpeedParameterName = "IdleSpeed";

		public const string IdleStateName = "Idle";

		public const string SpawnAnimationClipName = "Ennemy_Spawn";

		public const string SpawnAnimatorStateName = "Spawn";

		public const string EnemyMaterialPath = "View/Materials/EnemyUnit";

		public const string BossMaterialPath = "View/Materials/BossUnit";
	}

	[SerializeField]
	private EnemyAttackFeedback enemyAttackFeedbackPrefab;

	[SerializeField]
	private SpriteRenderer frontSpriteRenderer;

	[SerializeField]
	private SpriteRenderer backSpriteRenderer;

	[SerializeField]
	private Transform front;

	[SerializeField]
	private Transform back;

	[SerializeField]
	private SortingGroup sortingGroup;

	private static readonly int IdleSpeedParameter = Animator.StringToHash("IdleSpeed");

	private static readonly int MaskingColorId = Shader.PropertyToID("_MaskingColor");

	private Material material;

	private HashSet<PlayableUnit> playableUnitsInRange = new HashSet<PlayableUnit>();

	protected WaitUntil waitUntilAnimatorStateIsSpawn;

	public EnemyAttackFeedback EnemyAttackFeedbackPrefab => enemyAttackFeedbackPrefab;

	public EnemyUnit EnemyUnit { get; protected set; }

	public bool IsVisible
	{
		get
		{
			if (!frontSpriteRenderer.isVisible)
			{
				return backSpriteRenderer.isVisible;
			}
			return true;
		}
	}

	public bool HasSpawnAnim { get; private set; }

	public override bool Hovered
	{
		get
		{
			return hovered;
		}
		set
		{
			bool flag = value || (EnemyUnit?.LinkedBuilding != null && EnemyUnit.LinkedBuilding == TileObjectSelectionManager.SelectedBuilding);
			if (hovered != flag)
			{
				hovered = flag;
				RefreshCursorFeedback();
			}
		}
	}

	public override float MoveSpeed
	{
		get
		{
			if (!EnemyUnitManager.TurboMode)
			{
				return EnemyUnit.EnemyUnitTemplateDefinition.MoveSpeed * GameManager.MoveSpeedMultiplier;
			}
			return EnemyUnit.EnemyUnitTemplateDefinition.MoveSpeed * GameManager.MoveSpeedMultiplier * 10f;
		}
	}

	public EnemyUnitHUD EnemyUnitHUD => base.UnitHUD as EnemyUnitHUD;

	public override TheLastStand.Model.Unit.Unit Unit
	{
		get
		{
			return base.Unit;
		}
		set
		{
			base.Unit = value;
			EnemyUnit = Unit as EnemyUnit;
			InitZoneControlSkill();
			InstantiateHudIfNeeded();
			base.UnitHUD.Unit = Unit;
		}
	}

	public WaitUntil WaitUntilAnimatorStateIsSpawn => waitUntilAnimatorStateIsSpawn;

	public static Sprite GetHiddenEnemySprite()
	{
		return ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Units/Portaits/Enemy Unit/EnemiesPortrait_Hidden");
	}

	public static Sprite GetUiSprite(string enemyUnitDefinitionId)
	{
		if (string.IsNullOrEmpty(enemyUnitDefinitionId))
		{
			return null;
		}
		return ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Units/Portaits/Enemy Unit/EnemiesPortrait_" + enemyUnitDefinitionId);
	}

	public override void ClearWaitUntils()
	{
		base.ClearWaitUntils();
		waitUntilAnimatorStateIsSpawn = null;
	}

	public virtual string GetDefaultSpritesFolder()
	{
		if (!(EnemyUnit is BossUnit))
		{
			return "EnemyUnits";
		}
		return "BossUnits";
	}

	public override void InitVisuals(bool playSpawnAnim)
	{
		if (!(animator == null))
		{
			string specificAssetsId = EnemyUnit.SpecificAssetsId;
			InitDefaultSprites(specificAssetsId);
			Unit.UiSprite = GetUiSprite(specificAssetsId);
			front.localPosition = EnemyUnit.EnemyUnitTemplateDefinition.VisualOffset;
			back.localPosition = EnemyUnit.EnemyUnitTemplateDefinition.VisualOffset;
			if (EnemyUnit.EnemyUnitTemplateDefinition.SortingOrderOverride.HasValue)
			{
				sortingGroup.sortingOrder = EnemyUnit.EnemyUnitTemplateDefinition.SortingOrderOverride.Value;
			}
			if (Unit.UiSprite == null)
			{
				Unit.UiSprite = EnemyUnit.DefaultSpriteFront;
			}
			animator.runtimeAnimatorController = null;
			frontSpriteRenderer.sprite = EnemyUnit.DefaultSpriteFront;
			backSpriteRenderer.sprite = EnemyUnit.DefaultSpriteBack;
			InitMaterial();
			base.InitVisuals(playSpawnAnim);
		}
	}

	public override void LookAtDirection(GameDefinition.E_Direction direction, bool updateViewAnimator = false)
	{
		if (EnemyUnit != null && EnemyUnit.EnemyUnitTemplateDefinition.LockedOrientation != GameDefinition.E_Direction.None)
		{
			direction = EnemyUnit.EnemyUnitTemplateDefinition.LockedOrientation;
		}
		base.LookAtDirection(direction, updateViewAnimator);
	}

	public override void RefreshCursorFeedback()
	{
		DisplayHover(!base.Selected && Hovered);
		RefreshZoneControl();
		if (EnemyUnit.LinkedBuilding != null)
		{
			EnemyUnit.LinkedBuilding.BuildingView.Hovered = base.HoveredOrSelected;
		}
	}

	protected void CheckDefaultSpritesInit(string unitId)
	{
		if (EnemyUnit.EnemyUnitTemplateDefinition.MovePointsTotal.Min != 0f)
		{
			if (EnemyUnit.DefaultSpriteFront == null)
			{
				EnemyUnit.LogError("Missing default sprite 'Front' for unit: '" + unitId + "' with variant: '" + EnemyUnit.VariantId + "'");
			}
			if (EnemyUnit.DefaultSpriteBack == null)
			{
				EnemyUnit.LogError("Missing default sprite 'Back' for unit: '" + unitId + "' with variant: '" + EnemyUnit.VariantId + "'");
			}
		}
	}

	protected virtual void InitMaterial()
	{
		material = ResourcePooler.LoadOnce<Material>(((EnemyUnit is BossUnit) ? "View/Materials/BossUnit" : "View/Materials/EnemyUnit") + "_" + EnemyUnit.SpecificAssetsId, failSilently: true) ?? EnemyUnitManager.DefaultEnemyMaterial;
		frontSpriteRenderer.material = material;
		backSpriteRenderer.material = material;
		RefreshMaterial();
	}

	public void RefreshMaterial()
	{
		if (Unit.OriginTile.HasAnyFog)
		{
			frontSpriteRenderer.material = TPSingleton<FogManager>.Instance.FogView.EnemiesInFogMaterial;
			backSpriteRenderer.material = TPSingleton<FogManager>.Instance.FogView.EnemiesInFogMaterial;
		}
		else
		{
			frontSpriteRenderer.material = material;
			backSpriteRenderer.material = material;
		}
	}

	protected override void DisableHUD()
	{
		base.UnitHUD.gameObject.SetActive(value: false);
	}

	protected override bool InitAnimations()
	{
		animator.enabled = true;
		animator.runtimeAnimatorController = ResourcePooler.LoadOnce<RuntimeAnimatorController>("Animators/Units/EnemyUnits/" + EnemyUnit.SpecificAssetsId + "/" + EnemyUnit.SpecificAssetsId + "_" + EnemyUnit.VariantId);
		animator.SetFloat(IdleSpeedParameter, (Unit is BossUnit) ? 1f : RandomManager.GetRandomRange(this, EnemyUnitManager.IdleAnimSpeedMultRange.x, EnemyUnitManager.IdleAnimSpeedMultRange.y));
		if (!base.InitAnimations())
		{
			return false;
		}
		if (waitUntilAnimatorStateIsSpawn == null)
		{
			waitUntilAnimatorStateIsSpawn = new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Constants.AnimatorSpawnStateHash);
		}
		List<KeyValuePair<AnimationClip, AnimationClip>> list = new List<KeyValuePair<AnimationClip, AnimationClip>>();
		animatorOverrideController.GetOverrides(list);
		HasSpawnAnim = false;
		for (int num = list.Count - 1; num >= 0; num--)
		{
			KeyValuePair<AnimationClip, AnimationClip> keyValuePair = list[num];
			if (keyValuePair.Key.name == "Ennemy_Spawn" && keyValuePair.Value != null)
			{
				HasSpawnAnim = true;
				break;
			}
		}
		return true;
	}

	protected override void InitAndStartAnimations(bool playSpawnAnim)
	{
		base.InitAndStartAnimations(playSpawnAnim);
		animator.Play("Idle", -1, 0f);
		if (playSpawnAnim && HasSpawnAnim)
		{
			animator.Play("Spawn");
		}
	}

	protected virtual void InitDefaultSprites(string unitId)
	{
		EnemyUnit.DefaultSpriteFront = ResourcePooler.LoadOnce<Sprite>("View/Sprites/Units/" + GetDefaultSpritesFolder() + "/DefaultSprites/" + unitId + "/" + EnemyUnit.VariantId + "/" + unitId + "_Lvl1_" + EnemyUnit.VariantId + "_Idle_Front_00");
		Sprite sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/Units/" + GetDefaultSpritesFolder() + "/DefaultSprites/" + unitId + "/" + EnemyUnit.VariantId + "/" + unitId + "_Lvl1_" + EnemyUnit.VariantId + "_Idle_Back_00");
		EnemyUnit.DefaultSpriteBack = sprite ?? EnemyUnit.DefaultSpriteFront;
		CheckDefaultSpritesInit(unitId);
	}

	protected override void InitHud()
	{
	}

	protected virtual E_EnemyHudType GetBestFittingHUD()
	{
		if (EnemyUnit is BossUnit bossUnit)
		{
			return bossUnit.BossUnitTemplateDefinition.HealthGaugeSize switch
			{
				E_GaugeSize.Small => E_EnemyHudType.BossSmall, 
				E_GaugeSize.Large => E_EnemyHudType.BossLarge, 
				_ => E_EnemyHudType.BossLarge, 
			};
		}
		return EnemyUnit.EnemyUnitTemplateDefinition.HealthGaugeSize switch
		{
			E_GaugeSize.Small => E_EnemyHudType.Small, 
			E_GaugeSize.Large => E_EnemyHudType.Large, 
			_ => E_EnemyHudType.Large, 
		};
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Hovered = false;
		base.Selected = false;
		PlayableUnitManager.OnPlayableUnitMoved -= HandleOnPlayableUnitMoved;
		PlayableUnitManager.OnPlayableUnitDied -= HandleOnPlayableUnitDied;
	}

	protected override IEnumerator PlayDieAnimCoroutine()
	{
		if (!EnemyUnit.IgnoreFromEnemyUnitsCount && !(EnemyUnit is BossUnit))
		{
			TPSingleton<EnemyUnitManager>.Instance.EnemiesDying.Add(EnemyUnit);
		}
		yield return base.PlayDieAnimCoroutine();
	}

	private void DisplayHover(bool show)
	{
		if (!TPSingleton<TileMapManager>.Exist())
		{
			return;
		}
		TileBase tile = (show ? TileMapView.EnemiesHoverTileBase : null);
		foreach (TheLastStand.Model.TileMap.Tile occupiedTile in EnemyUnit.OccupiedTiles)
		{
			TileMapView.EnemiesHoverTilemap.SetTile((Vector3Int)occupiedTile.Position, tile);
		}
	}

	private void InstantiateHudIfNeeded()
	{
		if (base.UnitHUD != null)
		{
			base.UnitHUD.gameObject.SetActive(value: true);
		}
		else if (Unit != null)
		{
			E_EnemyHudType bestFittingHUD = GetBestFittingHUD();
			base.UnitHUD = ObjectPooler.GetPooledComponent(bestFittingHUD.GetPoolId(), bestFittingHUD.GetPrefab(), PlayableUnitManager.UnitHudsTransform);
		}
	}

	public void DisplayZoneControlSkill()
	{
		TPSingleton<TileMapView>.Instance.DisplaySkillAoE(EnemyUnit.EnemyUnitTemplateDefinition.ZoneControlSkill, EnemyUnit.OriginTile, TileMapView.EnemiesHoverAreaOfEffectTilemap);
	}

	public void RefreshZoneControl()
	{
		if (EnemyUnit.EnemyUnitTemplateDefinition.ZoneControlSkill != null)
		{
			TPSingleton<EnemyUnitManager>.Instance.HookZoneControlEnemy(this, ShouldDisplayZoneControlSkill());
		}
	}

	private bool ShouldDisplayZoneControlSkill()
	{
		if (!EnemyUnit.IsDeadOrDeathRattling)
		{
			if (!base.Selected && !Hovered)
			{
				return playableUnitsInRange.Count > 0;
			}
			return true;
		}
		return false;
	}

	private void InitZoneControlSkill()
	{
		HookZoneControlSkill();
		if (EnemyUnit.EnemyUnitTemplateDefinition.ZoneControlSkill == null)
		{
			return;
		}
		AreaOfEffectDefinition areaOfEffectDefinition = EnemyUnit.EnemyUnitTemplateDefinition.ZoneControlSkill.AreaOfEffectDefinition;
		for (int i = 0; i < areaOfEffectDefinition.Pattern.Count; i++)
		{
			for (int j = 0; j < areaOfEffectDefinition.Pattern[i].Count; j++)
			{
				if (areaOfEffectDefinition.Pattern[i][j] == 'X' && TileMapManager.GetTile(EnemyUnit.OriginTile.Position.x - areaOfEffectDefinition.Origin.x + j, EnemyUnit.OriginTile.Position.y - areaOfEffectDefinition.Origin.y + i)?.Unit is PlayableUnit item)
				{
					playableUnitsInRange.Add(item);
				}
			}
		}
		RefreshZoneControl();
	}

	private void HookZoneControlSkill()
	{
		PlayableUnitManager.OnPlayableUnitMoved -= HandleOnPlayableUnitMoved;
		PlayableUnitManager.OnPlayableUnitDied -= HandleOnPlayableUnitDied;
		if (EnemyUnit.EnemyUnitTemplateDefinition.ZoneControlSkill != null)
		{
			PlayableUnitManager.OnPlayableUnitMoved += HandleOnPlayableUnitMoved;
			PlayableUnitManager.OnPlayableUnitDied += HandleOnPlayableUnitDied;
		}
	}

	private void HandleOnPlayableUnitMoved(PlayableUnit playableUnit, TheLastStand.Model.TileMap.Tile tile)
	{
		AreaOfEffectDefinition areaOfEffectDefinition = EnemyUnit.EnemyUnitTemplateDefinition.ZoneControlSkill.AreaOfEffectDefinition;
		Vector2Int vector2Int = playableUnit.OriginTile.Position - EnemyUnit.OriginTile.Position + areaOfEffectDefinition.Origin;
		if (vector2Int.y >= 0 && vector2Int.y < areaOfEffectDefinition.Pattern.Count && vector2Int.x >= 0 && vector2Int.x < areaOfEffectDefinition.Pattern[vector2Int.y].Count && areaOfEffectDefinition.Pattern[vector2Int.y][vector2Int.x] == 'X')
		{
			playableUnitsInRange.Add(playableUnit);
		}
		else
		{
			playableUnitsInRange.Remove(playableUnit);
		}
		RefreshZoneControl();
	}

	private void HandleOnPlayableUnitDied(PlayableUnit playableUnit)
	{
		playableUnitsInRange.Remove(playableUnit);
		RefreshZoneControl();
	}
}
