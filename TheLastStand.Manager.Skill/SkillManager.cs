using System;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Debugging;
using TPLib.Debugging.Console;
using TPLib.Localization;
using TheLastStand.Controller.Status;
using TheLastStand.Controller.Status.Immunity;
using TheLastStand.Controller.TileMap;
using TheLastStand.Controller.Unit;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Status;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Pathfinding;
using TheLastStand.View;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.HUD.UnitManagement;
using TheLastStand.View.Skill.UI;
using TheLastStand.View.TileMap;
using TheLastStand.View.Tooltip.Tooltip.Compendium;
using TheLastStand.View.Unit;
using UnityEngine;

namespace TheLastStand.Manager.Skill;

public class SkillManager : Manager<SkillManager>
{
	public enum E_InvalidSkillCause
	{
		None,
		FriendlyFireOnly,
		LineOfSight,
		NoTarget,
		OutOfRange,
		[Obsolete]
		TargetingSelf,
		ManeuverInvalid,
		InvincibleTarget
	}

	[SerializeField]
	private SkillTooltip skillInfoPanel;

	[SerializeField]
	private AttackTooltip attackInfoPanel;

	[SerializeField]
	private GenericActionTooltip genericActionInfoPanel;

	[SerializeField]
	private CompendiumPanel compendiumPanel;

	[SerializeField]
	private SkillEffectFeedback skillEffectFeedback;

	[SerializeField]
	private MultiHitTargetHUD multiHitTargetHUDPrefab;

	[SerializeField]
	private SkillTargetingMark skillTargetingMarkUIPrefab;

	[SerializeField]
	private SkillTargetingMark skillTargetingMarkSpritePrefab;

	[SerializeField]
	private Color availableUsesPerTurnColor;

	[SerializeField]
	private Color noUsesPerTurnColor;

	[SerializeField]
	private Color unavailableUsesPerTurnColor;

	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	private AudioClip skillValidTileAudioClip;

	[SerializeField]
	private AudioClip skillInvalidTileAudioClip;

	[SerializeField]
	private AudioClip skillCancelAudioClip;

	[SerializeField]
	private Tuple<int, AudioClip> hitSoftAudioClip = new Tuple<int, AudioClip>(1, null);

	[SerializeField]
	private Tuple<int, AudioClip> hitMediumAudioClip = new Tuple<int, AudioClip>(2, null);

	[SerializeField]
	private Tuple<int, AudioClip> hitHardAudioClip = new Tuple<int, AudioClip>(5, null);

	[SerializeField]
	private TileController.E_SegmentEndsComputationType lineOfSightTargeting;

	[SerializeField]
	[Range(0f, 0.49f)]
	[Tooltip("If ckecked, the line of sight will use the opposite corners of the tiles instead of using the center of the tiles")]
	private float lineOfSightTolerance = 0.4f;

	[SerializeField]
	[Tooltip("Draw line of sight in editor")]
	private bool drawSkillLineOfSight;

	private Dictionary<Tile, MultiHitTargetHUD> multiHitTargetHUDs = new Dictionary<Tile, MultiHitTargetHUD>();

	public static bool DebugSkillsForceCanRotate = false;

	public static bool DebugSkillsForceCanFlip = false;

	public static bool DebugSkillsAllowAllPhases = false;

	private static bool debugToggleSkillCastValidityCheck = true;

	public static AttackTooltip AttackInfoPanel => TPSingleton<SkillManager>.Instance.attackInfoPanel;

	public static AudioSource AudioSource => TPSingleton<SkillManager>.Instance.audioSource;

	public static Color AvailableUsesPerTurnColor => TPSingleton<SkillManager>.Instance.availableUsesPerTurnColor;

	public static GenericActionTooltip GenericActionInfoPanel => TPSingleton<SkillManager>.Instance.genericActionInfoPanel;

	public static Tuple<int, AudioClip> HitHardAudioClip => TPSingleton<SkillManager>.Instance.hitHardAudioClip;

	public static Tuple<int, AudioClip> HitMediumAudioClip => TPSingleton<SkillManager>.Instance.hitMediumAudioClip;

	public static Tuple<int, AudioClip> HitSoftAudioClip => TPSingleton<SkillManager>.Instance.hitSoftAudioClip;

	public static bool IsSelectedSkillFlipped { get; private set; }

	public static bool IsSelectedSkillValid { get; private set; }

	public static E_InvalidSkillCause SelectedSkillInvalidCause { get; private set; }

	public static SkillTargetingMark SkillTargetingMarkUIPrefab => TPSingleton<SkillManager>.Instance.skillTargetingMarkUIPrefab;

	public static SkillTargetingMark SkillTargetingMarkSpritePrefab => TPSingleton<SkillManager>.Instance.skillTargetingMarkSpritePrefab;

	public static float LineOfSightTolerance => TPSingleton<SkillManager>.Instance.lineOfSightTolerance;

	public static TileController.E_SegmentEndsComputationType LineOfSightTargeting => TPSingleton<SkillManager>.Instance.lineOfSightTargeting;

	public static Color NoUsesPerTurnColor => TPSingleton<SkillManager>.Instance.noUsesPerTurnColor;

	public static TheLastStand.Model.Skill.Skill PreviousSkillFlipped { get; set; }

	public static TheLastStand.Model.Skill.Skill SelectedSkill => PlayableUnitManager.SelectedSkill ?? BuildingManager.SelectedSkill;

	public static SkillEffectFeedback SkillEffectFeedback => TPSingleton<SkillManager>.Instance.skillEffectFeedback;

	public static AudioClip SkillCancelAudioClip => TPSingleton<SkillManager>.Instance.skillCancelAudioClip;

	public static SkillTooltip SkillInfoPanel => TPSingleton<SkillManager>.Instance.skillInfoPanel;

	public static CompendiumPanel CompendiumPanel => TPSingleton<SkillManager>.Instance.compendiumPanel;

	public static AudioClip SkillInvalidTileAudioClip => TPSingleton<SkillManager>.Instance.skillInvalidTileAudioClip;

	public static AudioClip SkillValidTileAudioClip => TPSingleton<SkillManager>.Instance.skillValidTileAudioClip;

	public static Color UnavailableUsesPerTurnColor => TPSingleton<SkillManager>.Instance.unavailableUsesPerTurnColor;

	public LineOfSight LineOfSight { get; private set; }

	public static Status AddStatus(TheLastStand.Model.Unit.Unit targetUnit, Status.E_StatusType statusType, StatusCreationInfo statusCreationInfo, bool refreshHUD = true)
	{
		if (targetUnit != statusCreationInfo.Source && !targetUnit.CanBeDamaged())
		{
			return null;
		}
		Status status;
		switch (statusType)
		{
		case Status.E_StatusType.Poison:
			status = new PoisonStatusController(targetUnit, statusCreationInfo).Status;
			break;
		case Status.E_StatusType.Stun:
			status = new StunStatusController(targetUnit, statusCreationInfo).Status;
			break;
		case Status.E_StatusType.Charged:
			status = new ChargedStatusController(targetUnit, statusCreationInfo).Status;
			break;
		case Status.E_StatusType.Contagion:
			status = new ContagionStatusController(targetUnit, statusCreationInfo).Status;
			break;
		case Status.E_StatusType.Invulnerable:
			status = new InvulnerableStatusController(targetUnit, statusCreationInfo).Status;
			break;
		case Status.E_StatusType.DebuffImmunity:
		case Status.E_StatusType.PoisonImmunity:
		case Status.E_StatusType.StunImmunity:
		case Status.E_StatusType.ContagionImmunity:
		case Status.E_StatusType.AllNegativeImmunity:
			status = new ImmunityStatusController(targetUnit, statusCreationInfo, statusType).Status;
			break;
		case Status.E_StatusType.Debuff:
			status = new DebuffStatusController(targetUnit, statusCreationInfo).Status;
			break;
		case Status.E_StatusType.Buff:
			status = new BuffStatusController(targetUnit, statusCreationInfo).Status;
			break;
		default:
			TPSingleton<SkillManager>.Instance.LogError($"Tried to apply status {statusType} it is not implemented in AddStatus static method.");
			return null;
		}
		targetUnit.UnitController.AddStatus(status, refreshHUD);
		if (!(targetUnit is PlayableUnit))
		{
			if (targetUnit is EnemyUnit)
			{
				UnitManagementView<EnemyUnitManagementView>.Refresh();
			}
		}
		else
		{
			UnitManagementView<PlayableUnitManagementView>.Refresh();
			TPSingleton<CharacterSheetPanel>.Instance.Refresh();
		}
		if (statusType == Status.E_StatusType.Poison || statusType == Status.E_StatusType.PoisonImmunity)
		{
			targetUnit.UnitView.RefreshHealth();
		}
		if (refreshHUD)
		{
			targetUnit.UnitView.UnitHUD.RefreshStatuses();
		}
		if (!statusCreationInfo.HideDisplayEffect && GameManager.CurrentStateName != Game.E_State.UnitExecutingSkill && GameManager.CurrentStateName != Game.E_State.BuildingExecutingSkill)
		{
			targetUnit.UnitController.DisplayEffects();
		}
		return status;
	}

	public static void AddMultiHitTargetHUD(Tile tile)
	{
		if (!TPSingleton<SkillManager>.Instance.multiHitTargetHUDs.TryGetValue(tile, out var value))
		{
			value = ObjectPooler.GetPooledComponent("MultiHitHUDs", TPSingleton<SkillManager>.Instance.multiHitTargetHUDPrefab);
			value.transform.position = TileMapView.GetWorldPosition(tile);
			TPSingleton<SkillManager>.Instance.multiHitTargetHUDs.Add(tile, value);
		}
		MultiHitTargetHUD multiHitTargetHUD = value;
		int counter = multiHitTargetHUD.Counter + 1;
		multiHitTargetHUD.Counter = counter;
	}

	public static bool CheckSkillCastValidity(Tile targetTile, out E_InvalidSkillCause cause, TheLastStand.Model.Skill.Skill skillToCheck = null)
	{
		if (debugToggleSkillCastValidityCheck)
		{
			TheLastStand.Model.Skill.Skill skill = skillToCheck ?? SelectedSkill;
			bool flag = skillToCheck == null && SelectedSkill != null && IsSelectedSkillFlipped;
			bool flag2 = skill.SkillAction.HasEffect("Maneuver");
			if (!skill.SkillDefinition.InfiniteRange && !skill.SkillAction.SkillActionExecution.InRangeTiles.Range.ContainsKey(targetTile))
			{
				cause = E_InvalidSkillCause.OutOfRange;
				return false;
			}
			if (flag2 && (!skill.SkillAction.SkillActionExecution.SkillExecutionController.IsManeuverValid(targetTile, skill.CursorDependantOrientation) || !skill.SkillAction.SkillActionExecution.InRangeTiles.Range.ContainsKey(targetTile)))
			{
				cause = E_InvalidSkillCause.ManeuverInvalid;
				return false;
			}
			if (!skill.SkillAction.SkillActionExecution.InRangeTiles.Range.ContainsKey(targetTile) || !skill.SkillAction.SkillActionExecution.InRangeTiles.Range[targetTile].HasLineOfSight)
			{
				cause = E_InvalidSkillCause.LineOfSight;
				return false;
			}
			if (!skill.SkillController.IsValidatingTargetingConstraints(targetTile))
			{
				cause = E_InvalidSkillCause.NoTarget;
				return false;
			}
			if (skill.SkillAction is QuitWatchtowerSkillAction)
			{
				cause = E_InvalidSkillCause.None;
				return true;
			}
			int angleFromOrientation = TileObjectSelectionManager.GetAngleFromOrientation(skill.CursorDependantOrientation);
			AreaOfEffectDefinition areaOfEffectDefinition = skill.SkillDefinition.AreaOfEffectDefinition;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			bool flag6 = false;
			bool flag7 = false;
			bool flag8 = false;
			bool flag9 = false;
			int i = 0;
			for (int count = areaOfEffectDefinition.Pattern.Count; i < count; i++)
			{
				int j = 0;
				for (int count2 = areaOfEffectDefinition.Pattern[i].Count; j < count2; j++)
				{
					bool flag10 = areaOfEffectDefinition.Pattern[i][j] == 'e';
					if (areaOfEffectDefinition.Pattern[i][j] != 'X' && !flag10)
					{
						continue;
					}
					Vector2Int vector2Int = new Vector2Int(j - areaOfEffectDefinition.Origin.x, i - areaOfEffectDefinition.Origin.y);
					if (flag)
					{
						vector2Int = vector2Int.FlipX();
					}
					Vector2Int rotatedTilemapPosition = TileMapController.GetRotatedTilemapPosition(vector2Int, targetTile.Position, angleFromOrientation);
					Tile tile = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(rotatedTilemapPosition.x, rotatedTilemapPosition.y);
					if (tile == null)
					{
						continue;
					}
					if (tile.Unit != null)
					{
						flag3 |= tile.Unit is PlayableUnit && skill.SkillDefinition.CanAffectUnitOfType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.PlayableUnit, flag10) && (skill.SkillAction.SkillActionExecution.Caster != tile.Unit || skill.SkillDefinition.CanAffectUnitOfType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.Caster, flag10));
						if (skill.SkillAction is ResupplySkillAction resupplySkillAction)
						{
							flag9 |= resupplySkillAction.CheckUnitNeedResupply(tile.Unit);
						}
						if (tile.Unit is EnemyUnit)
						{
							if (tile.Unit.CanBeDamaged())
							{
								if (skill.SkillDefinition.CanAffectUnitOfType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.EnemyUnit, flag10))
								{
									flag4 = true;
								}
							}
							else
							{
								flag5 = true;
							}
						}
					}
					else
					{
						TheLastStand.Model.Building.Building building = tile.Building;
						if (building != null)
						{
							DamageableModule damageableModule = building.DamageableModule;
							if (damageableModule != null && !damageableModule.IsDead)
							{
								flag6 |= !tile.Building.BlueprintModule.IsIndestructible;
								flag7 |= tile.Building is MagicCircle;
								if (skill.SkillAction is ResupplySkillAction resupplySkillAction2)
								{
									flag8 |= resupplySkillAction2.CheckBuildingNeedRepair(tile.Building);
								}
							}
						}
					}
					if (flag3 && flag4 && flag6 && flag7)
					{
						goto end_IL_0370;
					}
				}
				continue;
				end_IL_0370:
				break;
			}
			bool flag11 = skill.SkillAction is AttackSkillAction || (skill.SkillAction is GenericSkillAction genericSkillAction && (genericSkillAction.HasEffect("Stun") || genericSkillAction.HasEffect("Poison") || genericSkillAction.HasEffect("Debuff") || genericSkillAction.HasEffect("Contagion")));
			if ((flag3 || flag7) && !flag4 && flag11)
			{
				if (!skill.SkillDefinition.AllowFriendlyFire)
				{
					cause = E_InvalidSkillCause.FriendlyFireOnly;
					return false;
				}
			}
			else
			{
				if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && !flag8 && (skill.SkillAction.TryGetFirstEffect<ResupplyOverallUsesSkillEffectDefinition>("ResupplyOverallUses", out var _) || skill.SkillAction.TryGetFirstEffect<ResupplyChargesSkillEffectDefinition>("ResupplyCharges", out var _)))
				{
					cause = E_InvalidSkillCause.NoTarget;
					return false;
				}
				if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && !flag9 && skill.SkillAction.TryGetFirstEffect<ResupplySkillsSkillEffectDefinition>("ResupplySkills", out var _))
				{
					cause = E_InvalidSkillCause.NoTarget;
					return false;
				}
				if (!flag4 && !flag6 && flag11 && !flag2)
				{
					cause = (flag5 ? E_InvalidSkillCause.InvincibleTarget : E_InvalidSkillCause.NoTarget);
					return false;
				}
			}
		}
		cause = E_InvalidSkillCause.None;
		return true;
	}

	public static void ClearMultiHitTargetHUD()
	{
		foreach (MultiHitTargetHUD value in TPSingleton<SkillManager>.Instance.multiHitTargetHUDs.Values)
		{
			value.gameObject.SetActive(value: false);
		}
		TPSingleton<SkillManager>.Instance.multiHitTargetHUDs.Clear();
	}

	public static float GetSelectedSkillDodgeMultiplierWithDistance()
	{
		float num = 1f;
		if (TPSingleton<GameManager>.Instance.Game.Cursor.Tile != null && SelectedSkill.SkillAction is AttackSkillAction { AttackType: AttackSkillActionDefinition.E_AttackType.Ranged } && SelectedSkill.SkillAction.SkillActionExecution.SkillSourceTile != null && SelectedSkill.SkillAction.SkillActionExecution.InRangeTiles.IsInRange(TPSingleton<GameManager>.Instance.Game.Cursor.Tile) && !SelectedSkill.SkillAction.HasEffect("NoDodge"))
		{
			int num2 = TileMapController.DistanceBetweenTiles(TPSingleton<GameManager>.Instance.Game.Cursor.Tile, SelectedSkill.SkillAction.SkillActionExecution.SkillSourceTile);
			foreach (KeyValuePair<int, float> item in SkillDatabase.DamageTypeModifiersDefinition.DodgeMultiplierByDistance)
			{
				if (num2 < item.Key)
				{
					break;
				}
				num = item.Value;
			}
		}
		if (SelectedSkill.SkillDefinition.SkillActionDefinition.TryGetFirstEffect<InaccurateSkillEffectDefinition>("Inaccurate", out var effect))
		{
			ISkillCaster caster = SelectedSkill.SkillAction.SkillActionExecution.Caster;
			float malus = effect.GetMalus(effect.GetPerkContext(caster));
			num *= 1f + malus;
		}
		return (float)Math.Round(num, 2);
	}

	public static string GetSkillEffectDescription(string id)
	{
		return Localizer.Get("SkillEffectDescription_" + id);
	}

	public static string GetSkillEffectName(string id)
	{
		return Localizer.Get("SkillEffectName_" + id);
	}

	public static SkillDefinition GetSkillDefinition(List<SkillProgression> skillProgressions, string skillGroupId, int modifiedDayNumber)
	{
		SkillProgression skillProgression = null;
		foreach (SkillProgression skillProgression2 in skillProgressions)
		{
			if (skillProgression2.SkillDefinition.GroupId == skillGroupId && skillProgression2.AreConditionsValid(modifiedDayNumber, skillProgression?.UnlockedAtDay ?? (-1)))
			{
				skillProgression = skillProgression2;
			}
		}
		return skillProgression?.SkillDefinition;
	}

	public static bool TryGetSkillDefinition(List<SkillProgression> skillProgressions, string skillGroupId, int modifiedDayNumber, out SkillDefinition skillDefinition)
	{
		skillDefinition = GetSkillDefinition(skillProgressions, skillGroupId, modifiedDayNumber);
		return skillDefinition != null;
	}

	public static bool TryGetSkillDefinitionOrDatabase(List<SkillProgression> skillProgressions, string skillGroupId, int modifiedDayNumber, out SkillDefinition skillDefinition)
	{
		if (!TryGetSkillDefinition(skillProgressions, skillGroupId, modifiedDayNumber, out skillDefinition) && !SkillDatabase.SkillDefinitions.TryGetValue(skillGroupId, out skillDefinition))
		{
			return (skillDefinition = SkillDatabase.SkillDefinitions.Values.FirstOrDefault((SkillDefinition x) => x.GroupId == skillGroupId)) != null;
		}
		return true;
	}

	public static void RefreshSelectedSkillValidityOnTile(Tile tile)
	{
		E_InvalidSkillCause cause = E_InvalidSkillCause.None;
		IsSelectedSkillValid = tile != null && SelectedSkill != null && CheckSkillCastValidity(tile, out cause);
		SelectedSkillInvalidCause = cause;
	}

	public static void RemoveMultiHitTargetHUD(Tile tile)
	{
		if (TPSingleton<SkillManager>.Instance.multiHitTargetHUDs.TryGetValue(tile, out var value) && --value.Counter <= 0)
		{
			value.gameObject.SetActive(value: false);
			TPSingleton<SkillManager>.Instance.multiHitTargetHUDs.Remove(tile);
		}
	}

	public static void ResetIsSelectedSkillFlipped(TheLastStand.Model.Skill.Skill newSelectedSkill)
	{
		IsSelectedSkillFlipped = newSelectedSkill != null && newSelectedSkill == PreviousSkillFlipped;
		PreviousSkillFlipped = null;
	}

	protected override void Awake()
	{
		base.Awake();
		LineOfSight = new LineOfSightController(SkillDatabase.LineOfSightDefinition).LineOfSight;
	}

	private void Update()
	{
		if (SelectedSkill == null || (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.UnitPreparingSkill && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.BuildingPreparingSkill))
		{
			return;
		}
		Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
		bool flag = TPSingleton<GameManager>.Instance.Game.Cursor.TileHasChanged || GameView.TopScreenPanel.UnitPortraitsPanel.TargettedPortraitHasChanged;
		UnitPortraitView portraitIsHovered = GameView.TopScreenPanel.UnitPortraitsPanel.GetPortraitIsHovered();
		if (InputManager.GetButtonDown(61))
		{
			if ((SelectedSkill.SkillDefinition.CanRotate || DebugSkillsForceCanRotate) && TileObjectSelectionManager.CursorOrientationFromSelection.HasFlag(TileObjectSelectionManager.E_Orientation.LIMIT))
			{
				flag = true;
				TileObjectSelectionManager.SwitchPreviousCursorOrientation();
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.UpdateDialsTilesColorsFrom(SelectedSkill.SkillAction.SkillActionExecution.SkillSourceTile, SelectedSkill.SkillAction.SkillActionExecution.InRangeTiles.Range);
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.UpdateDisplayRangeTilesColors(SelectedSkill.SkillAction.SkillActionExecution.InRangeTiles.Range);
				if (TileObjectSelectionManager.HasPlayableUnitSelected)
				{
					TileObjectSelectionManager.SelectedPlayableUnit?.UnitController.LookAtDirection(TileObjectSelectionManager.GetDirectionFromOrientation(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection));
				}
			}
			else if (SelectedSkill.SkillDefinition.CanFlip || DebugSkillsForceCanFlip)
			{
				flag = true;
				IsSelectedSkillFlipped = !IsSelectedSkillFlipped;
			}
		}
		if (flag)
		{
			if (portraitIsHovered != null)
			{
				tile = portraitIsHovered.PlayableUnit.OriginTile;
			}
			RefreshSelectedSkillValidityOnTile(tile);
		}
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.UnitPreparingSkill)
		{
			if (SelectedSkill.SkillAction.HasEffect("Maneuver"))
			{
				if (tile != null && IsSelectedSkillValid)
				{
					if (TPSingleton<GameManager>.Instance.Game.Cursor.TileHasChanged || PlayableUnitManager.MovePath.Path.Count == 0)
					{
						PathfindingData pathfindingData = new PathfindingData
						{
							Unit = TileObjectSelectionManager.SelectedUnit,
							TargetTiles = new Tile[1] { SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.GetManeuverTile(tile, SelectedSkill.CursorDependantOrientation) },
							MoveRange = -1,
							DistanceFromTargetMin = 0,
							DistanceFromTargetMax = 0,
							IgnoreCanStopOnConstraints = true,
							OverrideUnitMoveMethod = UnitTemplateDefinition.E_MoveMethod.Flying,
							PathfindingStyle = PathfindingDefinition.E_PathfindingStyle.Bresenham
						};
						if (DebugManager.DebugMode)
						{
							if (Input.GetKey(KeyCode.M))
							{
								pathfindingData.PathfindingStyle = PathfindingDefinition.E_PathfindingStyle.Manhattan;
							}
							else if (Input.GetKey(KeyCode.H))
							{
								pathfindingData.PathfindingStyle = PathfindingDefinition.E_PathfindingStyle.Hypotenuse;
							}
						}
						PlayableUnitManager.MovePath.MovePathController.ComputePath(pathfindingData);
						PlayableUnitManager.MovePath.MovePathController.UpdateState(tile);
						PlayableUnitManager.MovePath.MovePathView.DisplayMovePath();
					}
				}
				else if (PlayableUnitManager.MovePath.Path.Count > 0)
				{
					PlayableUnitManager.MovePath.MovePathController.Clear();
				}
			}
			if (InputManager.GetButtonDown(24) && tile != null)
			{
				if (IsSelectedSkillValid)
				{
					SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.AddTarget(tile, SelectedSkill.CursorDependantOrientation, IsSelectedSkillFlipped);
					bool flag2 = true;
					if (SelectedSkill.SkillAction.TryGetFirstEffect<MultiHitSkillEffectDefinition>("MultiHit", out var effect))
					{
						int num = ((SelectedSkill.SkillAction.SkillActionExecution.Caster is PlayableUnit playableUnit) ? playableUnit.PlayableUnitController.GetModifiedMultiHitsCount(effect.GetHitsCount(effect.GetPerkContext(playableUnit))) : effect.HitsCount);
						if (SelectedSkill.SkillAction.SkillActionExecution.TargetTiles.Count < num)
						{
							flag2 = false;
						}
					}
					if (flag2)
					{
						TPSingleton<PlayableUnitManager>.Instance.CastSelectedSkill();
					}
					SoundManager.PlayAudioClip(AudioSource, SkillValidTileAudioClip);
				}
				else
				{
					EffectManager.InvalidSkillDisplay.Init(SelectedSkillInvalidCause);
					EffectManager.InvalidSkillDisplay.FollowElement.ChangeTarget(null);
					EffectManager.InvalidSkillDisplay.FollowElement.ChangeTarget(tile.TileView.transform);
					EffectManager.InvalidSkillDisplay.Display();
					SoundManager.PlayAudioClip(AudioSource, SkillInvalidTileAudioClip);
				}
			}
		}
		else if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.BuildingPreparingSkill && InputManager.GetButtonDown(24) && tile != null)
		{
			if (IsSelectedSkillValid)
			{
				SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.AddTarget(tile, SelectedSkill.CursorDependantOrientation, IsSelectedSkillFlipped);
				bool flag3 = true;
				if (SelectedSkill.SkillAction.TryGetFirstEffect<MultiHitSkillEffectDefinition>("MultiHit", out var effect2) && SelectedSkill.SkillAction.SkillActionExecution.TargetTiles.Count < effect2.HitsCount)
				{
					flag3 = false;
				}
				if (flag3)
				{
					TPSingleton<BuildingManager>.Instance.CastSelectedSkill();
				}
			}
			else
			{
				EffectManager.InvalidSkillDisplay.Init(SelectedSkillInvalidCause);
				EffectManager.InvalidSkillDisplay.FollowElement.ChangeTarget(null);
				EffectManager.InvalidSkillDisplay.FollowElement.ChangeTarget(tile.TileView.transform);
				EffectManager.InvalidSkillDisplay.Display();
				SoundManager.PlayAudioClip(AudioSource, SkillInvalidTileAudioClip);
			}
		}
		if (flag)
		{
			if (InputManager.IsPointerOverWorld || InputManager.IsPointerOverAllowingCursorUI || portraitIsHovered != null)
			{
				SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionView.DisplayAreaOfEffect(tile);
				SkillEffectFeedback.Refresh();
			}
			else
			{
				SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionView.Clear(keepRangeTiles: true);
				SkillEffectFeedback.Refresh();
			}
		}
	}

	[DevConsoleCommand("ToggleSkillCastValidityCheck")]
	public static void DebugToggleSkillCastValidityCheck(bool state = false)
	{
		debugToggleSkillCastValidityCheck = state;
	}

	[DevConsoleCommand("SkillsForceCanRotate")]
	public static void DebugSetSkillsForceCanRotate(bool state = true)
	{
		DebugSkillsForceCanRotate = state;
	}

	[DevConsoleCommand("SkillsForceCanFlip")]
	public static void DebugSetSkillsForceCanFlip(bool state = true)
	{
		DebugSkillsForceCanFlip = state;
	}

	[DevConsoleCommand("SkillsAllowAllPhases")]
	public static void DebugSetSkillsAllowAllPhases(bool state = true)
	{
		DebugSkillsAllowAllPhases = state;
		GameView.BottomScreenPanel.BottomLeftPanel.Refresh();
	}
}
