using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TPLib;
using TPLib.Debugging;
using TPLib.Debugging.Console;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Controller;
using TheLastStand.Controller.ApplicationState;
using TheLastStand.Controller.Item;
using TheLastStand.Controller.TileMap;
using TheLastStand.Controller.Trophy.TrophyConditions;
using TheLastStand.Controller.Unit;
using TheLastStand.Controller.Unit.Pathfinding;
using TheLastStand.Controller.Unit.Stat;
using TheLastStand.DRM.Achievements;
using TheLastStand.Database;
using TheLastStand.Database.Unit;
using TheLastStand.Definition;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Meta.Glyphs.GlyphEffects;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Definition.Unit.Race;
using TheLastStand.Definition.Unit.Trait;
using TheLastStand.Framework.Command.Conversation;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Sequencing;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Item;
using TheLastStand.Model.Meta;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Model.Status;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Movement;
using TheLastStand.Model.Unit.Pathfinding;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkModule;
using TheLastStand.Model.Unit.Stat;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Unit;
using TheLastStand.View;
using TheLastStand.View.Camera;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.Cursor;
using TheLastStand.View.HUD.UnitManagement;
using TheLastStand.View.ProductionReport;
using TheLastStand.View.Seer;
using TheLastStand.View.Sound;
using TheLastStand.View.TileMap;
using TheLastStand.View.ToDoList;
using TheLastStand.View.Unit;
using TheLastStand.View.Unit.Pathfinding;
using TheLastStand.View.Unit.Perk;
using TheLastStand.View.Unit.Race;
using TheLastStand.View.Unit.Stat;
using TheLastStand.View.Unit.Trait;
using TheLastStand.View.Unit.UI;
using TheLastStand.View.UnitManagement.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheLastStand.Manager.Unit;

[StringConverter(typeof(StringToTPSingletonConverter<PlayableUnitManager>))]
public sealed class PlayableUnitManager : Manager<PlayableUnitManager>, ISerializable, IDeserializable
{
	public static class Consts
	{
		public const string PlayableUnits = "PlayableUnits";

		public const string HitsAudioSourcePoolId = "HitsSFX";

		public const string HitsSpatializedAudioSourcePoolId = "HitsSFX Spatialized";

		public const string PlayableUnitHitsSoundAssetPrefix = "Sounds/SFX/PlayableUnitHits/";
	}

	public class StringToPerkIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries
		{
			get
			{
				List<string> list = new List<string>();
				if (TileObjectSelectionManager.SelectedPlayableUnit != null)
				{
					foreach (UnitPerkTier unitPerkTier in TileObjectSelectionManager.SelectedPlayableUnit.PerkTree.UnitPerkTiers)
					{
						foreach (Perk perk in unitPerkTier.Perks)
						{
							if (perk != null && !perk.Unlocked)
							{
								list.Add(perk.PerkDefinition.Id);
							}
						}
					}
				}
				return list;
			}
		}
	}

	public class StringToActivePerkIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries
		{
			get
			{
				List<string> list = new List<string>();
				if (TileObjectSelectionManager.SelectedPlayableUnit != null)
				{
					foreach (Perk value in TileObjectSelectionManager.SelectedPlayableUnit.Perks.Values)
					{
						if (value != null && value.Unlocked)
						{
							list.Add(value.PerkDefinition.Id);
						}
					}
				}
				return list;
			}
		}
	}

	public class StringToUnlockedPerkIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries
		{
			get
			{
				List<string> list = new List<string>();
				if (TileObjectSelectionManager.SelectedPlayableUnit != null)
				{
					foreach (UnitPerkTier unitPerkTier in TileObjectSelectionManager.SelectedPlayableUnit.PerkTree.UnitPerkTiers)
					{
						foreach (Perk perk in unitPerkTier.Perks)
						{
							if (perk != null && perk.Unlocked)
							{
								list.Add(perk.PerkDefinition.Id);
							}
						}
					}
				}
				return list;
			}
		}
	}

	[SerializeField]
	private Transform unitsTransform;

	[SerializeField]
	private Transform unitHudsTransform;

	[SerializeField]
	private PlayableUnitView playableUnitViewPrefab;

	[SerializeField]
	private PlayableUnitGhostView playableUnitGhostViewPrefab;

	[SerializeField]
	private Transform playableUnitGhostParent;

	[FormerlySerializedAs("statTooltipPanel")]
	[SerializeField]
	private StatTooltip statTooltip;

	[FormerlySerializedAs("traitTooltipPanel")]
	[SerializeField]
	private TraitTooltip traitTooltip;

	[FormerlySerializedAs("perkTooltipPanel")]
	[SerializeField]
	private PerkTooltip perkTooltip;

	[SerializeField]
	private PlayableUnitTooltip playableUnitTooltip;

	[SerializeField]
	private RaceTooltip raceTooltip;

	[SerializeField]
	private MovePathView movePathView;

	[SerializeField]
	private OneShotSound hitSFXPrefab;

	[SerializeField]
	private OneShotSound hitSFXSpatializedPrefab;

	[SerializeField]
	private Vector2 perkReplacementRandomDelay = new Vector2(0f, 0.01f);

	private TheLastStand.Model.Skill.Skill selectedSkill;

	private SkillActionExecution hoverSkillExecution;

	private readonly Dictionary<int, TheLastStand.Model.Skill.Skill> skillHotkeys = new Dictionary<int, TheLastStand.Model.Skill.Skill>();

	private MovePath movePath;

	private readonly CompensationConversation unitsConversation = new CompensationConversation(isRedoable: false);

	private readonly List<PlayableUnitGhostView> playableUnitGhostView = new List<PlayableUnitGhostView>();

	private OneShotSound hitSFX;

	[SerializeField]
	private bool debugForceSkipNightReport;

	[SerializeField]
	private List<RuntimeAnimatorController> debugPlayableUnitsAnimatorControllers;

	private bool debugDisableHealthDisplay;

	private bool debugToggleDismissHeroValidityChecks = true;

	public static MovePath MovePath => TPSingleton<PlayableUnitManager>.Instance.movePath;

	public static CompensationConversation UnitsConversation => TPSingleton<PlayableUnitManager>.Instance.unitsConversation;

	public static Vector2 PerkReplacementRandomDelay => TPSingleton<PlayableUnitManager>.Instance.perkReplacementRandomDelay;

	public static PerkTooltip PerkTooltip => TPSingleton<PlayableUnitManager>.Instance.perkTooltip;

	public static PlayableUnitTooltip PlayableUnitTooltip => TPSingleton<PlayableUnitManager>.Instance.playableUnitTooltip;

	public static RaceTooltip RaceTooltip => TPSingleton<PlayableUnitManager>.Instance.raceTooltip;

	public static TheLastStand.Model.Skill.Skill SelectedSkill
	{
		get
		{
			return TPSingleton<PlayableUnitManager>.Instance.selectedSkill;
		}
		set
		{
			TheLastStand.Model.Skill.Skill skill = TPSingleton<PlayableUnitManager>.Instance.selectedSkill;
			TPSingleton<PlayableUnitManager>.Instance.selectedSkill = value;
			if (TPSingleton<PlayableUnitManager>.Instance.selectedSkill == skill)
			{
				return;
			}
			SkillManager.ResetIsSelectedSkillFlipped(TPSingleton<PlayableUnitManager>.Instance.selectedSkill);
			TPSingleton<TileMapView>.Instance.ClearRangedSkillsModifiers();
			TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution?.SkillExecutionController.Reset();
			if (skill != null)
			{
				skill.SkillAction.SkillActionExecution.SkillExecutionController.Reset();
				if ((skill.SkillDefinition.CanRotate || SkillManager.DebugSkillsForceCanRotate) && (TPSingleton<PlayableUnitManager>.Instance.selectedSkill == null || (!TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.CanRotate && !SkillManager.DebugSkillsForceCanRotate)) && TileObjectSelectionManager.CursorOrientationFromSelection.HasFlag(TileObjectSelectionManager.E_Orientation.LIMIT))
				{
					TileMapView.SetTile(TileMapView.SkillRotationFeedbackTileMap, TPSingleton<GameManager>.Instance.Game.Cursor.Tile);
				}
				else if ((skill.SkillDefinition.CanFlip || SkillManager.DebugSkillsForceCanFlip) && (TPSingleton<PlayableUnitManager>.Instance.selectedSkill == null || (!TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.CanFlip && !SkillManager.DebugSkillsForceCanFlip)) && TPSingleton<GameManager>.Instance.Game.Cursor.Tile != null)
				{
					TileMapView.SetTile(TileMapView.SkillRotationFeedbackTileMap, TPSingleton<GameManager>.Instance.Game.Cursor.Tile);
				}
			}
			if (TPSingleton<PlayableUnitManager>.Instance.selectedSkill != null)
			{
				TPSingleton<GameManager>.Instance.Game.Cursor.Tile?.Building?.BuildingView.HideSkillRangeIfNeeded();
				GenericSkillActionDefinition genericSkillActionDefinition = TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.SkillActionDefinition as GenericSkillActionDefinition;
				if (TPSingleton<SettingsManager>.Instance.Settings.SmartCast && !InputManager.IsLastControllerJoystick && genericSkillActionDefinition != null && genericSkillActionDefinition.CasterEffectOnly)
				{
					TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.PrepareSkill(TPSingleton<PlayableUnitManager>.Instance.selectedSkill.Owner, TPSingleton<PlayableUnitManager>.Instance.selectedSkill.Owner.OriginTile);
					SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.AddTarget(TPSingleton<PlayableUnitManager>.Instance.selectedSkill.Owner.OriginTile, SelectedSkill.CursorDependantOrientation, SkillManager.IsSelectedSkillFlipped);
					TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution = null;
					TPSingleton<PlayableUnitManager>.Instance.CastSelectedSkill();
					return;
				}
				GameController.SetState(Game.E_State.UnitPreparingSkill);
				TPSingleton<PlayableUnitManager>.Instance.HasToRecomputeReachableTiles = true;
				TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.PrepareSkill(TileObjectSelectionManager.SelectedUnit, TileObjectSelectionManager.SelectedUnit.OriginTile);
				if (TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillAction is AttackSkillAction)
				{
					SkillManager.AttackInfoPanel.SetSkill(TPSingleton<PlayableUnitManager>.Instance.selectedSkill, TileObjectSelectionManager.SelectedUnit);
					SkillManager.GenericActionInfoPanel.Hide();
					TheLastStand.Model.Unit.Unit unit = TPSingleton<GameManager>.Instance.Game.Cursor.Tile?.Unit;
					if (unit != TileObjectSelectionManager.SelectedUnit || SelectedSkill.SkillDefinition.AreaOfEffectDefinition.Pattern.Count <= TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin.x || SelectedSkill.SkillDefinition.AreaOfEffectDefinition.Pattern[TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin.x].Count <= TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin.y || SelectedSkill.SkillDefinition.AreaOfEffectDefinition.Pattern[TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin.x][TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin.y] == 'X')
					{
						SkillManager.AttackInfoPanel.TargetTile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
						SkillManager.AttackInfoPanel.TargetUnit = unit;
						SkillManager.AttackInfoPanel.Display();
						if (EnemyUnitManager.IsAnyEnemyTooltipDisplayed())
						{
							EnemyUnitManager.GetDisplayedEnemyTootlip().Refresh();
						}
						else if (PlayableUnitTooltip.Displayed)
						{
							PlayableUnitTooltip.Refresh();
						}
					}
				}
				else if (genericSkillActionDefinition != null)
				{
					SkillManager.GenericActionInfoPanel.SetSkill(TPSingleton<PlayableUnitManager>.Instance.selectedSkill, TileObjectSelectionManager.SelectedUnit);
					SkillManager.AttackInfoPanel.Hide();
					SkillManager.GenericActionInfoPanel.Display();
					if (EnemyUnitManager.IsAnyEnemyTooltipDisplayed())
					{
						EnemyUnitManager.GetDisplayedEnemyTootlip().Refresh();
					}
					else if (PlayableUnitTooltip.Displayed)
					{
						PlayableUnitTooltip.Refresh();
					}
				}
				else
				{
					SkillManager.AttackInfoPanel.Hide();
					SkillManager.GenericActionInfoPanel.Hide();
				}
			}
			if (TPSingleton<PlayableUnitManager>.Instance.selectedSkill == null)
			{
				SkillManager.AttackInfoPanel.Hide();
				SkillManager.GenericActionInfoPanel.Hide();
				if (EnemyUnitManager.IsAnyEnemyTooltipDisplayed())
				{
					EnemyUnitManager.GetDisplayedEnemyTootlip().Refresh();
				}
				else if (PlayableUnitTooltip.Displayed)
				{
					PlayableUnitTooltip.Refresh();
				}
				if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.UnitPreparingSkill)
				{
					GameController.SetState(Game.E_State.Management);
				}
			}
			TPSingleton<PlayableUnitManagementView>.Instance.PlayableSkillBar.ChangeSelectedSkill();
		}
	}

	public static StatTooltip StatTooltip => TPSingleton<PlayableUnitManager>.Instance.statTooltip;

	public static TraitTooltip TraitTooltip => TPSingleton<PlayableUnitManager>.Instance.traitTooltip;

	public static Transform UnitHudsTransform => TPSingleton<PlayableUnitManager>.Instance.unitHudsTransform;

	public static bool HasUnitInFog => TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Any((PlayableUnit x) => x.OriginTile.HasFog);

	public bool HasToRecomputeReachableTiles { get; set; }

	public OneShotSound HitSFXPrefab => hitSFXPrefab;

	public OneShotSound HitSFXSpatializedPrefab => hitSFXSpatializedPrefab;

	public TaskGroup MoveUnitsTaskGroup { get; set; }

	public NightReport NightReport { get; } = new NightReportController().NightReport;

	public List<PlayableUnit> PlayableUnits { get; private set; }

	public Dictionary<int, List<PlayableUnit>> DeadPlayableUnits { get; private set; } = new Dictionary<int, List<PlayableUnit>>();

	public List<PlayableUnit> PlayableUnitsToRespawn { get; private set; }

	public SkillActionExecution PreviewSkillExecution { get; set; }

	public Recruitment Recruitment { get; private set; } = new Recruitment();

	public PlayableUnitGhostView SelectedPlayableUnitGhost { get; set; }

	public bool ShouldClearUndoStack { get; set; }

	public bool ShouldWaitUntilDeathSequences => PlayableUnits.Where((PlayableUnit o) => o.IsDead).Any((PlayableUnit o) => !o.PlayableUnitView.DeathSequenceOver);

	public bool ShouldTriggerPlayableUnitsDeathSequence => PlayableUnits.Where((PlayableUnit u) => u.IsDead).Any((PlayableUnit u) => !u.IsDying);

	public WaitUntil WaitUntilDeathSequences => new WaitUntil(() => !ShouldWaitUntilDeathSequences);

	public WaitUntil WaitUntilTakeDamageSequences
	{
		get
		{
			IEnumerable<PlayableUnit> damagedPlayableUnits = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Where((PlayableUnit o) => o.UnitView.IsTakingDamage);
			return new WaitUntil(() => damagedPlayableUnits.Where((PlayableUnit o) => o.UnitView.IsTakingDamage).Count() == 0);
		}
	}

	public static bool DebugDisableHealthDisplay => TPSingleton<PlayableUnitManager>.Instance.debugDisableHealthDisplay;

	public static bool DebugForceSkipNightReport => TPSingleton<PlayableUnitManager>.Instance.debugForceSkipNightReport;

	public static bool DebugToggleDismissHeroValidityChecks => TPSingleton<PlayableUnitManager>.Instance.debugToggleDismissHeroValidityChecks;

	public static event Action<PlayableUnit, Tile> OnPlayableUnitMoved;

	public static event Action<PlayableUnit> OnPlayableUnitDied;

	public static bool CanUndoLastCommand()
	{
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management && TPSingleton<PlayableUnitManager>.Instance.unitsConversation.UndoStack.Count > 0)
		{
			if (TPSingleton<PlayableUnitManager>.Instance.unitsConversation.UndoStack.Peek() is MoveUnitCommand moveUnitCommand)
			{
				return moveUnitCommand.PlayableUnit.CanStopOn(moveUnitCommand.StartTile);
			}
			return true;
		}
		return false;
	}

	public static void UndoLastCommand()
	{
		if (CanUndoLastCommand())
		{
			UnitCommand unitCommand = TPSingleton<PlayableUnitManager>.Instance.unitsConversation.Undo() as UnitCommand;
			TileObjectSelectionManager.SetSelectedPlayableUnit(unitCommand.PlayableUnit, CameraView.CameraUIMasksHandler.IsPointOffscreenOrHiddenByUI(unitCommand.PlayableUnit.OriginTile));
			GameView.BottomScreenPanel.BottomLeftPanel.CancelMovementPanel.Refresh();
		}
	}

	public static void CreateStartUnits()
	{
		Vector2Int origin = new Vector2Int(TPSingleton<TileMapManager>.Instance.TileMap.Width / 2, TPSingleton<TileMapManager>.Instance.TileMap.Height / 2);
		Tile tile = null;
		int num = PlayableUnitDatabase.UnitTraitGenerationDefinition.StartTraitTotalPointsWithModifiers;
		TPSingleton<PlayableUnitManager>.Instance.Log("Generating start units using Id " + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.UnitGenerationDefinitionId, CLogLevel.DETAILED);
		List<UnitGenerationDefinition> enumerable = PlayableUnitDatabase.UnitsGenerationStartDefinitions[TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.UnitGenerationDefinitionId];
		enumerable = RandomManager.Shuffle(TPSingleton<PlayableUnitManager>.Instance, enumerable).ToList();
		List<string> availableRacesIds = GetAvailableRacesIds();
		List<string> availableRacesIds2 = GetAvailableRacesIds(removeHumans: true);
		List<string> list = new List<string>();
		if (!string.IsNullOrEmpty(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.UnitGenerationGuaranteedRaceId))
		{
			string unitGenerationGuaranteedRaceId = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.UnitGenerationGuaranteedRaceId;
			if (availableRacesIds.Contains(unitGenerationGuaranteedRaceId))
			{
				list.Add(unitGenerationGuaranteedRaceId);
			}
		}
		int removeStartingPlayableUnitAmount = ApocalypseManager.CurrentApocalypse.RemoveStartingPlayableUnitAmount;
		int num2 = Mathf.Max(1, enumerable.Count - removeStartingPlayableUnitAmount);
		int num3 = num2 - list.Count;
		if (num3 > 0)
		{
			for (int i = 0; i < num3; i++)
			{
				list.Add(GetStartingRosterRandomRaceId(availableRacesIds2));
			}
		}
		for (int j = 0; j < num2; j++)
		{
			tile = TileMapController.GetRandomUnoccupiedTile(origin, PlayableUnitDatabase.StartingUnitsSpawnAreaSize / 2);
			if (tile != null)
			{
				int randomRange = RandomManager.GetRandomRange(TPSingleton<PlayableUnitManager>.Instance, 0, enumerable[j].PlayableUnitGenerationDefinitionArchetypeIds.Count);
				string archetypeId = enumerable[j].PlayableUnitGenerationDefinitionArchetypeIds[randomRange];
				int num4 = ((j >= num2 - 1) ? num : RandomManager.GetRandomRange(TPSingleton<PlayableUnitManager>.Instance, PlayableUnitDatabase.UnitTraitGenerationDefinition.UnitTraitPointBoundariesWithModifiers.x, Mathf.Min(PlayableUnitDatabase.UnitTraitGenerationDefinition.UnitTraitPointBoundariesWithModifiers.y + 1, num - PlayableUnitDatabase.UnitTraitGenerationDefinition.UnitTraitPointBoundariesWithModifiers.x)));
				InstantiateUnit(GenerateUnit(1, archetypeId, num4, 0, list[j], isStartingUnit: true), tile, -1, onLoad: true);
				num -= num4;
			}
		}
		if (removeStartingPlayableUnitAmount > 0 && TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count > 0)
		{
			PlayableUnit playableUnit = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[0];
			foreach (UnitGenerationDefinition item in enumerable)
			{
				string text = item.PlayableUnitGenerationDefinitionArchetypeIds[0];
				if (!(text == playableUnit.ArchetypeId) && PlayableUnitDatabase.PlayableUnitGenerationDefinitions.TryGetValue(text, out var value))
				{
					PlayableUnitController.GenerateEquipment(value, playableUnit, ItemSlotDefinition.E_ItemSlotId.WeaponSlot, generateItemsToInventory: true);
				}
			}
		}
		List<UnitGenerationDefinition> list2 = new List<UnitGenerationDefinition>();
		if (!GlyphManager.TryGetGlyphEffects(out List<GlyphBonusUnitsEffectDefinition> glyphEffects))
		{
			return;
		}
		for (int num5 = glyphEffects.Count - 1; num5 >= 0; num5--)
		{
			list2.AddRange(glyphEffects[num5].UnitGenerationDefinitions);
		}
		for (int num6 = list2.Count - 1; num6 >= 0; num6--)
		{
			string startingRosterRandomRaceId = GetStartingRosterRandomRaceId(availableRacesIds2);
			tile = TileMapController.GetRandomUnoccupiedTile(origin, PlayableUnitDatabase.StartingUnitsSpawnAreaSize / 2);
			if (tile != null)
			{
				string archetypeId = list2[num6].PlayableUnitGenerationDefinitionArchetypeIds[RandomManager.GetRandomRange(TPSingleton<PlayableUnitManager>.Instance, 0, list2[num6].PlayableUnitGenerationDefinitionArchetypeIds.Count)];
				int num4 = RandomManager.GetRandomRange(TPSingleton<PlayableUnitManager>.Instance, PlayableUnitDatabase.UnitTraitGenerationDefinition.UnitTraitPointBoundariesWithModifiers.x, PlayableUnitDatabase.UnitTraitGenerationDefinition.UnitTraitPointBoundariesWithModifiers.y + 1);
				InstantiateUnit(GenerateUnit(1, archetypeId, num4, 0, startingRosterRandomRaceId, isStartingUnit: true), tile, -1, onLoad: true);
			}
		}
		TPSingleton<GlyphManager>.Instance.Log($"Added {list2.Count} units.");
	}

	public static void DestroyUnit(PlayableUnit playableUnit)
	{
		if (playableUnit.OriginTile.Unit == playableUnit)
		{
			playableUnit.OriginTile.TileController.SetUnit(null);
		}
		GameView.TopScreenPanel.UnitPortraitsPanel.RemovePortrait(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.IndexOf(playableUnit));
		TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Remove(playableUnit);
		if (TileObjectSelectionManager.SelectedUnit == playableUnit)
		{
			TileObjectSelectionManager.DeselectUnit();
		}
		else if (TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.RefreshUnitMovePath(TileObjectSelectionManager.SelectedPlayableUnit);
		}
		if (TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count == 0)
		{
			NightTurnsManager.ForceStopTurnExecution();
			TileObjectSelectionManager.DeselectAll();
			CursorView.ClearTiles();
			TPSingleton<SeerPreviewDisplay>.Instance.Displayed = false;
			TPSingleton<ToDoListView>.Instance.Hide();
			GameView.TopScreenPanel.Display(show: false);
			PanicManager.Panic.PanicView.DisplayOrHide();
			GameController.TriggerGameOver(Game.E_GameOverCause.HeroesDeath);
		}
	}

	public static void DestroyDeadUnit(PlayableUnit playableUnit)
	{
		playableUnit.OriginTile.TileController.SetUnit(null);
		playableUnit.PlayableUnitView.gameObject.SetActive(value: false);
		playableUnit.PlayableUnitView.UnitHUD?.gameObject.SetActive(value: false);
	}

	public static void DismissPlayableUnit(PlayableUnit playableUnit = null)
	{
		if (playableUnit == null)
		{
			playableUnit = TileObjectSelectionManager.SelectedPlayableUnit;
		}
		if (playableUnit != null)
		{
			playableUnit.PlayableUnitController.PrepareForExile();
			playableUnit.PlayableUnitController.ExecuteExile();
			UnityEngine.Object.Destroy(playableUnit.PlayableUnitView.UnitHUD.gameObject);
			UnityEngine.Object.Destroy(playableUnit.PlayableUnitView.gameObject);
			CharacterSheetManager.CloseCharacterSheetPanel();
			TPSingleton<ToDoListView>.Instance.RefreshUnitLevelUpNotification();
		}
	}

	public static void EndTurn()
	{
		SelectedSkill?.SkillAction.SkillActionExecution?.SkillExecutionController.Reset();
		TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution?.SkillExecutionController.Reset();
		int num = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Max((PlayableUnit o) => o.ActionPointsSpentThisTurn);
		if (num > 0)
		{
			TPSingleton<MetaConditionManager>.Instance.RefreshMaxDoubleValue(MetaConditionSpecificContext.E_ValueCategory.MaxActionPointsOnHeroSingleTurn, num);
		}
		int num2 = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Max((PlayableUnit o) => o.TilesCrossedThisTurn);
		if (num2 > 0)
		{
			TPSingleton<MetaConditionManager>.Instance.RefreshMaxDoubleValue(MetaConditionSpecificContext.E_ValueCategory.MaxTilesCrossedSingleTurn, num2);
		}
		for (int num3 = 0; num3 < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; num3++)
		{
			TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[num3].UnitController.EndTurn();
		}
		switch (TPSingleton<GameManager>.Instance.Game.Cycle)
		{
		case Game.E_Cycle.Day:
			if (TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Deployment)
			{
				TPSingleton<MetaConditionManager>.Instance.RefreshEquippedUsables();
			}
			break;
		case Game.E_Cycle.Night:
			if (TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits)
			{
				TPSingleton<PlayableUnitManager>.Instance.ClearIconFeedback();
			}
			break;
		}
	}

	[DevConsoleCommand("GatherUnitsForVictorySequence")]
	public static void GatherUnitsForVictorySequence()
	{
		Vector2Int[] array = new Vector2Int[2]
		{
			new Vector2Int(TPSingleton<TileMapManager>.Instance.TileMap.Width / 2 - 3, TPSingleton<TileMapManager>.Instance.TileMap.Height / 2 + 3),
			new Vector2Int(TPSingleton<TileMapManager>.Instance.TileMap.Width / 2 + 3, TPSingleton<TileMapManager>.Instance.TileMap.Height / 2 - 3)
		};
		int num = UnityEngine.Random.Range(0, array.Length);
		for (int num2 = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count - 1; num2 >= 0; num2--)
		{
			Tile tile = TileMapController.GetRandomUnoccupiedOrBarricadeTile(array[num], PlayableUnitDatabase.VictoryUnitsGatherAreaSize / 2) ?? TileMapController.GetRandomUnoccupiedOrBarricadeTile(array[++num % array.Length], PlayableUnitDatabase.VictoryUnitsGatherAreaSize / 2);
			if (tile == null)
			{
				TPSingleton<PlayableUnitManager>.Instance.Log($"Could not find tiles to gather all playable units around the map center in a radius of {PlayableUnitDatabase.VictoryUnitsGatherAreaSize / 2}.");
				break;
			}
			if (tile.Building != null)
			{
				BuildingManager.DestroyBuilding(tile, updateView: true, addDeadBuilding: false, triggerEvent: false);
			}
			TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[num2].UnitController.SetTile(tile);
			TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[num2].UnitView.UpdatePosition();
			num = ++num % array.Length;
		}
	}

	public static PlayableUnit GenerateUnit(int unitLevel, string archetypeId, int traitPoints, int ghostUnitIndex = 0, string raceDefinitionId = null, bool isStartingUnit = false)
	{
		PlayableUnitController playableUnitController = new PlayableUnitController(archetypeId, traitPoints, null, null, unitLevel, raceDefinitionId, isStartingUnit);
		playableUnitController.Unit.UnitStatsController.SnapBaseStatTo(UnitStatDefinition.E_Stat.ActionPoints, UnitStatDefinition.E_Stat.ActionPointsTotal);
		playableUnitController.Unit.UnitStatsController.SnapBaseStatTo(UnitStatDefinition.E_Stat.MovePoints, UnitStatDefinition.E_Stat.MovePointsTotal);
		SetPlayableUnitGhost(playableUnitController.PlayableUnit, ghostUnitIndex, snapshotOnly: true);
		return playableUnitController.PlayableUnit;
	}

	public static PlayableUnit GetFirstLivingUnit()
	{
		foreach (PlayableUnit playableUnit in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
		{
			if (playableUnit.Health > 0f)
			{
				return playableUnit;
			}
		}
		return null;
	}

	public static void InstantiateUnit(PlayableUnit generatedUnit, Tile tile, int saveVersion = -1, bool onLoad = false)
	{
		PlayableUnitView playableUnitView = UnityEngine.Object.Instantiate(TPSingleton<PlayableUnitManager>.Instance.playableUnitViewPrefab, TPSingleton<PlayableUnitManager>.Instance.unitsTransform);
		generatedUnit.UnitController.SetTile(tile);
		generatedUnit.UnitView = playableUnitView;
		generatedUnit.UnitController.LookAtDirection(GameDefinition.E_Direction.South);
		tile.TileController.SetUnit(generatedUnit);
		playableUnitView.Init(generatedUnit);
		TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Add(generatedUnit);
		if (tile.Building != null)
		{
			TPSingleton<TileMapView>.Instance.DisplayBuildingInstantly(tile.Building, tile);
		}
		FogController.SetLightFogTilesFromDictionnary(FogController.ToggleLightFogTiles((from tile2 in FogManager.GetLightFogRepelTiles(tile)
			where tile2.HasLightFogOn
			select tile2).ToList()), FogManager.LightFogFadeInEaseAndDuration, FogManager.LightFogFadeOutEaseAndDuration, FogManager.LightFogDisappearEaseAndDuration, instant: false, independently: true);
		if (!onLoad)
		{
			TPSingleton<MetaConditionManager>.Instance.RefreshMaxPlayableUnitStatReached(generatedUnit);
			TileObjectSelectionManager.SetSelectedPlayableUnit(generatedUnit);
		}
		if (TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count >= 6)
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_HAVE_6_HEROES);
		}
		generatedUnit.Events.GetValueOrDefault(E_EffectTime.OnUnitSpawned)?.Invoke(new PerkDataContainer());
	}

	public static void InstantiateDeadUnit(PlayableUnit generatedUnit, Tile tile)
	{
		PlayableUnitView playableUnitView = UnityEngine.Object.Instantiate(TPSingleton<PlayableUnitManager>.Instance.playableUnitViewPrefab, TPSingleton<PlayableUnitManager>.Instance.unitsTransform);
		generatedUnit.UnitController.SetTile(tile);
		generatedUnit.UnitView = playableUnitView;
		tile.TileController.SetUnit(generatedUnit);
		playableUnitView.InitDeadUnit(generatedUnit);
		DestroyDeadUnit(generatedUnit);
	}

	public static void OnCursorTileBecomeNull()
	{
		switch (TPSingleton<GameManager>.Instance.Game.State)
		{
		case Game.E_State.Management:
			TPSingleton<PlayableUnitManager>.Instance.movePath.MovePathController.Clear();
			break;
		case Game.E_State.PlaceUnit:
			TPSingleton<PlayableUnitManager>.Instance.SelectedPlayableUnitGhost.Display(displayed: false);
			break;
		}
	}

	public static void OnGameStateChange(Game.E_State state, Game.E_State previousState)
	{
		TPSingleton<PlayableUnitManager>.Instance.skillHotkeys.Clear();
		TPSingleton<PlayableUnitManager>.Instance.movePath.MovePathController.Clear();
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.NightTurn != Game.E_NightTurn.PlayableUnits)
		{
			return;
		}
		if (previousState == Game.E_State.UnitPreparingSkill && state != Game.E_State.UnitExecutingSkill && state != Game.E_State.UnitPreparingSkill)
		{
			SelectedSkill = null;
		}
		switch (state)
		{
		case Game.E_State.Construction:
		case Game.E_State.PlaceUnit:
			TileObjectSelectionManager.SelectedUnitFeedback.Display(display: false);
			if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Day)
			{
				PathfindingManager.Pathfinding.PathfindingController.ClearReachableTiles();
			}
			break;
		case Game.E_State.Management:
			if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Day)
			{
				PathfindingManager.Pathfinding.PathfindingController.ClearReachableTiles();
			}
			if (!TileObjectSelectionManager.HasUnitSelected)
			{
				break;
			}
			TPSingleton<PlayableUnitManager>.Instance.SetSkillsHotkeys();
			if (previousState == Game.E_State.Construction && !TileObjectSelectionManager.ClickedOnBuilding)
			{
				if (TileObjectSelectionManager.SelectedBuilding != null)
				{
					ACameraView.MoveTo(TileObjectSelectionManager.SelectedBuilding.BuildingView.transform);
				}
				else
				{
					ACameraView.MoveTo(TileObjectSelectionManager.SelectedUnit.UnitView.transform);
				}
			}
			else
			{
				UnitManagementView<PlayableUnitManagementView>.Refresh();
			}
			if (TPSingleton<PlayableUnitManager>.Instance.HasToRecomputeReachableTiles && TileObjectSelectionManager.HasPlayableUnitSelected)
			{
				TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.ComputeReachableTiles();
			}
			if (TileObjectSelectionManager.HasUnitSelected)
			{
				TileObjectSelectionManager.SelectedUnitFeedback.Display(display: true);
			}
			break;
		case Game.E_State.CharacterSheet:
		case Game.E_State.Shopping:
		case Game.E_State.ProductionReport:
			UnitManagementView<PlayableUnitManagementView>.Refresh();
			break;
		case Game.E_State.UnitPreparingSkill:
			TPSingleton<PlayableUnitManager>.Instance.SetSkillsHotkeys();
			if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Day)
			{
				PathfindingManager.Pathfinding.PathfindingController.ClearReachableTiles();
			}
			break;
		case Game.E_State.UnitExecutingSkill:
		case Game.E_State.Wait:
			TileObjectSelectionManager.SelectedUnitFeedback.Display(display: false);
			break;
		default:
			PlayableUnitTooltip.Hide();
			break;
		}
		PlayableUnitManagementView.OnGameStateChange(state);
		GameView.TopScreenPanel.TurnPanel.Refresh();
	}

	public static void OnTurnStart()
	{
		TPSingleton<PlayableUnitManager>.Instance.movePath.MovePathController.Clear();
	}

	public static void RegisterUnitToRespawn(PlayableUnit unit)
	{
		if (TPSingleton<PlayableUnitManager>.Instance.PlayableUnitsToRespawn == null)
		{
			TPSingleton<PlayableUnitManager>.Instance.PlayableUnitsToRespawn = new List<PlayableUnit>();
		}
		TPSingleton<PlayableUnitManager>.Instance.PlayableUnitsToRespawn.Add(unit);
	}

	public static void RespawnUnits()
	{
		if (TPSingleton<PlayableUnitManager>.Instance.PlayableUnitsToRespawn == null || TPSingleton<PlayableUnitManager>.Instance.PlayableUnitsToRespawn.Count == 0)
		{
			return;
		}
		TPSingleton<PlayableUnitManager>.Instance.Log($"Respawning {TPSingleton<PlayableUnitManager>.Instance.PlayableUnitsToRespawn.Count} playable units.");
		Vector2Int origin = new Vector2Int(TPSingleton<TileMapManager>.Instance.TileMap.Width / 2, TPSingleton<TileMapManager>.Instance.TileMap.Height / 2);
		for (int num = TPSingleton<PlayableUnitManager>.Instance.PlayableUnitsToRespawn.Count - 1; num >= 0; num--)
		{
			if (TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits.TryGetValue(TPSingleton<GameManager>.Instance.DayNumber, out var value) && value.Contains(TPSingleton<PlayableUnitManager>.Instance.PlayableUnitsToRespawn[num]))
			{
				value.Remove(TPSingleton<PlayableUnitManager>.Instance.PlayableUnitsToRespawn[num]);
			}
			TPSingleton<PlayableUnitManager>.Instance.PlayableUnitsToRespawn[num].State = TheLastStand.Model.Unit.Unit.E_State.Ready;
			InstantiateUnit(TPSingleton<PlayableUnitManager>.Instance.PlayableUnitsToRespawn[num], TileMapController.GetRandomUnoccupiedTile(origin, PlayableUnitDatabase.StartingUnitsSpawnAreaSize / 2));
		}
		TPSingleton<PlayableUnitManager>.Instance.PlayableUnitsToRespawn.Clear();
	}

	public static void SelectNextUnit()
	{
		SelectNewUnit(next: true);
	}

	public static void SelectNewUnit(bool next)
	{
		int num = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.IndexOf(TileObjectSelectionManager.SelectedPlayableUnit);
		int index = ((num != -1) ? (num + (next ? 1 : (-1))).Mod(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count) : (next ? (-1) : 0).Mod(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count));
		SelectUnitAtIndex(index);
	}

	public static void SelectPreviousUnit()
	{
		SelectNewUnit(next: false);
	}

	public static void SelectUnitAtIndex(int index)
	{
		TPSingleton<PlayableUnitManager>.Instance.movePath?.MovePathController.Clear();
		SelectedSkill?.SkillAction.SkillActionExecution.SkillExecutionController.Reset();
		if (EnemyUnitManager.PreviewedSkill != null)
		{
			EnemyUnitManager.PreviewedSkill = null;
		}
		if (BuildingManager.PreviewedSkill != null)
		{
			BuildingManager.PreviewedSkill = null;
		}
		TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution?.SkillExecutionController.Reset();
		if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Day)
		{
			PathfindingManager.Pathfinding.PathfindingController.ClearReachableTiles();
		}
		PlayableUnit playableUnit = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[index];
		TileObjectSelectionManager.SetSelectedPlayableUnit(playableUnit, CameraView.CameraUIMasksHandler.IsPointOffscreenOrHiddenByUI(playableUnit.OriginTile));
		if (TPSingleton<PlayableUnitManager>.Instance.HasToRecomputeReachableTiles && TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.ComputeReachableTiles();
		}
		if (StatTooltip.Displayed)
		{
			StatTooltip.Refresh();
		}
	}

	public static void SetPlayableUnitGhost(PlayableUnit playableUnit, int unitIndex, bool snapshotOnly = false)
	{
		if (unitIndex >= TPSingleton<PlayableUnitManager>.Instance.playableUnitGhostView.Count)
		{
			PlayableUnitGhostView playableUnitGhostView = UnityEngine.Object.Instantiate(TPSingleton<PlayableUnitManager>.Instance.playableUnitGhostViewPrefab, TPSingleton<PlayableUnitManager>.Instance.playableUnitGhostParent);
			TPSingleton<PlayableUnitManager>.Instance.playableUnitGhostView.Add(playableUnitGhostView);
			playableUnitGhostView.Display(displayed: false);
		}
		playableUnit.UnitView = TPSingleton<PlayableUnitManager>.Instance.playableUnitGhostView[unitIndex];
		TPSingleton<PlayableUnitManager>.Instance.playableUnitGhostView[unitIndex].Unit = playableUnit;
		playableUnit.PlayableUnitView.InitVisuals(playSpawnAnim: false);
		if (!snapshotOnly)
		{
			TPSingleton<PlayableUnitManager>.Instance.playableUnitGhostView[unitIndex].Display(displayed: true);
		}
	}

	public void SetSkillsHotkeys()
	{
		if (TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			skillHotkeys.Clear();
			SetWeaponSkillsHotkeys();
			SetEquipmentSkillsHotkeys();
			SetContextualSkillsHotkeys();
		}
	}

	public static void StartTurn()
	{
		TPSingleton<PlayableUnitManager>.Instance.unitsConversation.Clear();
		if (TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count == 0)
		{
			return;
		}
		for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i++)
		{
			TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].PlayableUnitController.StartTurn();
		}
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day || TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits)
		{
			TileObjectSelectionManager.EnsureUnitSelection();
			if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night)
			{
				ACameraView.MoveTo(TileObjectSelectionManager.SelectedUnit.UnitView.transform);
			}
			TPSingleton<PlayableUnitManager>.Instance.HasToRecomputeReachableTiles = TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night;
			if (EnemyUnitManager.DisableHuman)
			{
				GameController.EndTurn();
			}
		}
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits)
		{
			TPSingleton<PlayableUnitManager>.Instance.DisplayIconAndTileFeedback();
		}
	}

	public static List<string> GetAvailableRacesIds(bool removeHumans = false)
	{
		HashSet<string> lockedRacesIds = TPSingleton<MetaUpgradesManager>.Instance.GetLockedRacesIds();
		List<string> list = new List<string>();
		foreach (RaceDefinition value in PlayableUnitDatabase.RaceDefinitions.Values)
		{
			if (!lockedRacesIds.Contains(value.Id))
			{
				list.Add(value.Id);
			}
		}
		if (removeHumans && list.Contains("Human"))
		{
			list.Remove("Human");
		}
		return list;
	}

	private static string GetStartingRosterRandomRaceId(List<string> nonHumanAvailableRacesIds)
	{
		if (nonHumanAvailableRacesIds == null || nonHumanAvailableRacesIds.Count == 0)
		{
			return "Human";
		}
		int count = nonHumanAvailableRacesIds.Count;
		if (!PlayableUnitDatabase.UnitStartingRosterRacesDistributionDefinitionsByUnlockedRacesNb.TryGetValue(count, out var value))
		{
			if (PlayableUnitDatabase.UnitStartingRosterRacesDistributionDefinitionsByUnlockedRacesNb.Count <= 0)
			{
				return "Human";
			}
			int num = 0;
			foreach (int key in PlayableUnitDatabase.UnitStartingRosterRacesDistributionDefinitionsByUnlockedRacesNb.Keys)
			{
				if (num < key)
				{
					num = key;
				}
			}
			value = PlayableUnitDatabase.UnitStartingRosterRacesDistributionDefinitionsByUnlockedRacesNb[num];
		}
		int max = value.HumanWeight + value.NonHumanWeight;
		if (RandomManager.GetRandomRange(TPSingleton<PlayableUnitManager>.Instance, 0, max) - value.HumanWeight < 0 || value.NonHumanWeight <= 0)
		{
			return "Human";
		}
		int randomRange = RandomManager.GetRandomRange(TPSingleton<PlayableUnitManager>.Instance, 0, count);
		return nonHumanAvailableRacesIds[randomRange];
	}

	public void CastSelectedSkill()
	{
		GameController.SetState(Game.E_State.UnitExecutingSkill);
		ShouldClearUndoStack = false;
		TrophyManager.SetValueToTrophiesConditions<EnemiesKilledSingleAttackTrophyConditionController>(new object[2]
		{
			TileObjectSelectionManager.SelectedUnit.RandomId,
			0
		});
		SkillCommand command = new SkillCommand(TileObjectSelectionManager.SelectedPlayableUnit, SelectedSkill);
		UnitsConversation.Execute(command);
		SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.ExecuteSkill();
		StartCoroutine(WaitForSkillExecution(TileObjectSelectionManager.SelectedUnit));
	}

	public void ChangeEquipment()
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			return;
		}
		TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.SwitchWeaponSet();
		TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.ChangeEquipmentAudioClip);
		TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.RefreshStats();
		TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitView?.RefreshBodyParts();
		TPSingleton<PlayableUnitManagementView>.Instance.PlayableSkillBar.SetEquippedSkills(TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.GetWeaponSkills());
		TPSingleton<PlayableUnitManagementView>.Instance.RefreshEquipmentBoxSelectedSet();
		if (PlayableUnitTooltip.Displayed && PlayableUnitTooltip.PlayableUnit == TileObjectSelectionManager.SelectedPlayableUnit)
		{
			PlayableUnitTooltip.RefreshEquipmentSlots();
		}
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CharacterSheet || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.GameOver)
		{
			TPSingleton<CharacterSheetPanel>.Instance.RefreshAvatar();
			TPSingleton<CharacterSheetPanel>.Instance.RefreshSkills(TileObjectSelectionManager.SelectedPlayableUnit);
		}
		if (InventoryManager.InventoryView.FocusedInventorySlotView != null)
		{
			TheLastStand.Model.Item.Item item = InventoryManager.InventoryView.FocusedInventorySlotView.InventorySlot.Item;
			if (item != null && item.ItemDefinition.IsHandItem)
			{
				InventoryManager.InventoryView.FocusedInventorySlotView.Refresh();
			}
		}
		SetSkillsHotkeys();
	}

	public void DistributeDailyExperience()
	{
		float num = 0f;
		foreach (KillReportData item in NightReport.KillsThisNight)
		{
			num += item.TotalExperienceToShare;
		}
		for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i++)
		{
			TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].PlayableUnitController.ReceiveDailyExperience(num / (float)TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count);
		}
		EffectManager.DisplayEffects();
		NightReport.KillsThisNight.Clear();
		TPSingleton<EffectTimeEventManager>.Instance.InvokeEvent(E_EffectTime.OnExperienceDistributionEnded);
	}

	public void FocusCamOnSelectedPlayableUnit()
	{
		if (TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			FocusCamOnUnit(TileObjectSelectionManager.SelectedPlayableUnit);
		}
	}

	public void FocusCamOnUnit(TheLastStand.Model.Unit.Unit unit)
	{
		ACameraView.MoveTo(unit.UnitView.transform);
	}

	public int GetUnitIndexHotkeyPressed()
	{
		if (DebugManager.DebugMode)
		{
			return -1;
		}
		for (int i = 0; i < PlayableUnits.Count; i++)
		{
			int unitHotkey = GetUnitHotkey(i);
			if (unitHotkey != -1 && InputManager.GetButtonDown(unitHotkey))
			{
				return i;
			}
		}
		return -1;
	}

	public void InvokeDiedPlayableUnit(PlayableUnit playableUnit)
	{
		PlayableUnitManager.OnPlayableUnitDied?.Invoke(playableUnit);
	}

	public void InvokeMovedPlayableUnit(PlayableUnit playableUnit, Tile tile)
	{
		PlayableUnitManager.OnPlayableUnitMoved?.Invoke(playableUnit, tile);
	}

	public bool IsSelectedPlayableUnitInRange(Tile tile)
	{
		if (TileObjectSelectionManager.SelectedPlayableUnit != null && PathfindingManager.Pathfinding.ReachableTiles != null)
		{
			return PathfindingManager.Pathfinding.ReachableTiles.ContainsKey(tile);
		}
		return false;
	}

	public void MoveUnit(PlayableUnit playableUnit)
	{
		playableUnit.Path = MovePath.Path;
		Task task = playableUnit.PlayableUnitController.PrepareForMovement((TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night) ? (playableUnit.Path.Count - 1) : 0);
		if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Day)
		{
			PathfindingManager.Pathfinding.PathfindingController.ClearReachableTiles();
		}
		else if (playableUnit != null)
		{
			playableUnit.MovedThisDay = true;
			TPSingleton<ToDoListView>.Instance.RefreshPositionNotification();
		}
		MovePath.MovePathController.Clear();
		GameController.SetState(Game.E_State.Wait);
		playableUnit.PlayableUnitView.PlayWalkAnim(doWalk: true);
		TPSingleton<PlayableUnitManager>.Instance.MoveUnitsTaskGroup = new TaskGroup(delegate
		{
			GameController.SetState(Game.E_State.Management);
		});
		TPSingleton<PlayableUnitManager>.Instance.MoveUnitsTaskGroup.AddTask(task);
		TPSingleton<PlayableUnitManager>.Instance.MoveUnitsTaskGroup.Run();
	}

	public void RefreshUnitMovePath(PlayableUnit unit)
	{
		Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
		if (tile == null)
		{
			return;
		}
		if (PathfindingManager.Pathfinding.ReachableTiles.ContainsKey(tile) && unit.CanStopOn(tile))
		{
			int moveRange = (int)unit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.MovePoints).FinalClamped;
			if (unit.OriginTile.Building != null && unit.OriginTile.Building.IsWatchtower)
			{
				moveRange = 0;
			}
			else if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day)
			{
				if (tile.Unit == null)
				{
					movePath.MovePathController.SetPath(new Tile[2] { unit.OriginTile, tile });
					movePath.MovePathController.UpdateState(tile);
					movePath.MovePathView.DisplayMovePath();
				}
				else
				{
					movePath.MovePathView.Clear();
				}
				return;
			}
			PathfindingData pathfindingData = new PathfindingData
			{
				Unit = unit,
				TargetTiles = new Tile[1] { tile },
				MoveRange = moveRange,
				DistanceFromTargetMin = 0,
				DistanceFromTargetMax = 0,
				IgnoreCanStopOnConstraints = false,
				PathfindingStyle = PathfindingDefinition.E_PathfindingStyle.Bresenham
			};
			if (Input.GetKey(KeyCode.M))
			{
				pathfindingData.PathfindingStyle = PathfindingDefinition.E_PathfindingStyle.Manhattan;
			}
			else if (Input.GetKey(KeyCode.H))
			{
				pathfindingData.PathfindingStyle = PathfindingDefinition.E_PathfindingStyle.Hypotenuse;
			}
			movePath.MovePathController.ComputePath(pathfindingData);
			movePath.MovePathController.UpdateState(tile);
			movePath.MovePathView.DisplayMovePath();
		}
		else
		{
			movePath.MovePathController.Clear();
		}
	}

	public void UpdateSkillHotkeysInput()
	{
		if (TileObjectSelectionManager.SelectedBuilding != null || !(TileObjectSelectionManager.SelectedUnit is PlayableUnit selectedUnit))
		{
			return;
		}
		foreach (KeyValuePair<int, TheLastStand.Model.Skill.Skill> skillHotkey in skillHotkeys)
		{
			if (InputManager.GetButtonDown(skillHotkey.Key) && SelectSkill(skillHotkey.Value, selectedUnit))
			{
				break;
			}
		}
	}

	public int GetSkillHotkey(TheLastStand.Model.Skill.Skill skill)
	{
		foreach (KeyValuePair<int, TheLastStand.Model.Skill.Skill> skillHotkey in skillHotkeys)
		{
			if (skillHotkey.Value == skill)
			{
				return skillHotkey.Key;
			}
		}
		return -1;
	}

	public bool SelectSkill(TheLastStand.Model.Skill.Skill skill, PlayableUnit selectedUnit)
	{
		if (!selectedUnit.PreventedSkillsIds.Contains(skill.SkillDefinition.Id) && CanExecuteSkill(skill, selectedUnit) && (!skill.SkillDefinition.IsContextual || skill.SkillController.ComputeTargetsAndValidity(selectedUnit)))
		{
			SelectedSkill = skill;
			if (SelectedSkill != null)
			{
				Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
				SkillManager.RefreshSelectedSkillValidityOnTile(tile);
				if (GameView.TopScreenPanel.UnitPortraitsPanel.GetPortraitIsHovered() != null)
				{
					tile = GameView.TopScreenPanel.UnitPortraitsPanel.GetPortraitIsHovered().PlayableUnit.OriginTile;
				}
				SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionView.DisplayAreaOfEffect(tile);
			}
			return true;
		}
		return false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		TileObjectSelectionManager.OnUnitSelectionChange -= OnNewUnitSelected;
	}

	private bool CanExecuteSkill(TheLastStand.Model.Skill.Skill skill, PlayableUnit selectedUnit)
	{
		PlayableUnitStatsController playableUnitStatsController = selectedUnit.PlayableUnitStatsController;
		return skill.SkillController.CanExecuteSkill(playableUnitStatsController.GetStat(UnitStatDefinition.E_Stat.ActionPoints).FinalClamped, playableUnitStatsController.GetStat(UnitStatDefinition.E_Stat.MovePoints).FinalClamped, playableUnitStatsController.GetStat(UnitStatDefinition.E_Stat.Mana).FinalClamped, playableUnitStatsController.GetStat(UnitStatDefinition.E_Stat.Health).FinalClamped, selectedUnit.IsStunned);
	}

	private void ClearIconFeedback()
	{
		for (int i = 0; i < PlayableUnits.Count; i++)
		{
			PlayableUnits[i].UnitView.UnitHUD.DisplayIconFeedback(show: false);
		}
	}

	private void DisplayIconAndTileFeedback()
	{
		List<Tile> list = new List<Tile>();
		for (int i = 0; i < PlayableUnits.Count; i++)
		{
			if (PlayableUnits[i].WillDieByPoison && !PlayableUnits[i].IsDead)
			{
				PlayableUnits[i].UnitView.UnitHUD.DisplayIconFeedback();
				list.AddRange(PlayableUnits[i].OccupiedTiles);
			}
		}
		TileMapView.SetTiles(TileMapView.UnitFeedbackTilemap, list, "View/Tiles/Feedbacks/PoisonDeath");
	}

	private int GetUnitHotkey(int index)
	{
		return index switch
		{
			0 => 68, 
			1 => 69, 
			2 => 70, 
			3 => 71, 
			4 => 72, 
			5 => 73, 
			6 => 74, 
			_ => -1, 
		};
	}

	private void SelectNextSkill(bool nextSkill)
	{
		if (TileObjectSelectionManager.SelectedUnit is PlayableUnit)
		{
			TPSingleton<PlayableUnitManagementView>.Instance.SkillBar.SelectNextSkill(nextSkill);
		}
	}

	private void OnNewUnitSelected()
	{
		if (!TPSingleton<HUDJoystickNavigationManager>.Instance.HUDNavigationOn && InputManager.IsLastControllerJoystick && TileObjectSelectionManager.SelectedUnit is PlayableUnit)
		{
			TPSingleton<PlayableUnitManagementView>.Instance.SkillBar.JoystickSkillBar.ResetSkillIndex(1);
		}
	}

	private void SetWeaponSkillsHotkeys()
	{
		List<TheLastStand.Model.Skill.Skill> weaponSkills = TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.GetWeaponSkills();
		int num = 0;
		foreach (TheLastStand.Model.Skill.Skill item in weaponSkills)
		{
			if (item.IsPunch)
			{
				skillHotkeys.Add(120, item);
				continue;
			}
			int num2 = num switch
			{
				0 => 116, 
				1 => 117, 
				2 => 118, 
				3 => 119, 
				_ => -1, 
			};
			if (num2 == -1)
			{
				break;
			}
			skillHotkeys.Add(num2, item);
			num++;
		}
	}

	private void SetEquipmentSkillsHotkeys()
	{
		List<TheLastStand.Model.Skill.Skill> equipmentSkills = TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.GetEquipmentSkills();
		int num = 0;
		bool flag = false;
		foreach (TheLastStand.Model.Skill.Skill item2 in equipmentSkills)
		{
			if (item2.SkillContainer is TheLastStand.Model.Item.Item item && ItemDefinition.E_Category.BodyArmor.HasFlag(item.ItemDefinition.Category))
			{
				if (flag)
				{
					LogWarning($"Trying to set the hotkey of a {ItemDefinition.E_Category.BodyArmor} skill but one has already been set.");
					continue;
				}
				skillHotkeys.Add(121, item2);
				flag = true;
				continue;
			}
			int num2 = num switch
			{
				0 => 122, 
				1 => 123, 
				2 => 124, 
				3 => 125, 
				4 => 126, 
				5 => 127, 
				_ => -1, 
			};
			if (num2 == -1)
			{
				break;
			}
			skillHotkeys.Add(num2, item2);
			num++;
		}
	}

	private void SetContextualSkillsHotkeys()
	{
		List<TheLastStand.Model.Skill.Skill> contextualSkills = TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.GetContextualSkills();
		for (int i = 0; i < contextualSkills.Count; i++)
		{
			TheLastStand.Model.Skill.Skill value = contextualSkills[i];
			int num = i switch
			{
				0 => 128, 
				1 => 129, 
				2 => 130, 
				3 => 131, 
				4 => 132, 
				5 => 133, 
				6 => 134, 
				7 => 135, 
				_ => -1, 
			};
			if (num != -1)
			{
				skillHotkeys.Add(num, value);
				continue;
			}
			break;
		}
	}

	private void Start()
	{
		Recruitment?.ListenToBuildingDestroyEvent();
		TileObjectSelectionManager.OnUnitSelectionChange += OnNewUnitSelected;
	}

	private void Update()
	{
		if (!(ApplicationManager.Application.State is GameState) || (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.NightTurn != Game.E_NightTurn.PlayableUnits))
		{
			return;
		}
		foreach (PlayableUnit playableUnit2 in PlayableUnits)
		{
			if (playableUnit2.IsDead && !playableUnit2.PlayableUnitView.DeathSequenceOver)
			{
				return;
			}
		}
		Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
		Tile previousTile = TPSingleton<GameManager>.Instance.Game.Cursor.PreviousTile;
		switch (TPSingleton<GameManager>.Instance.Game.State)
		{
		case Game.E_State.UnitPreparingSkill:
			if (TPSingleton<GameManager>.Instance.Game.Cursor.TileHasChanged)
			{
				SkillManager.SkillInfoPanel.RefreshOnTileChanged();
				TheLastStand.Model.Unit.Unit unit = tile?.Unit;
				if (SelectedSkill.SkillDefinition.SkillActionDefinition is AttackSkillActionDefinition)
				{
					if (unit == TileObjectSelectionManager.SelectedUnit && SelectedSkill.SkillDefinition.AreaOfEffectDefinition.Pattern.Count > TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin.x && SelectedSkill.SkillDefinition.AreaOfEffectDefinition.Pattern[TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin.x].Count > TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin.y && SelectedSkill.SkillDefinition.AreaOfEffectDefinition.Pattern[TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin.x][TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin.y] != 'X')
					{
						SkillManager.AttackInfoPanel.TargetTile = tile;
						SkillManager.AttackInfoPanel.TargetUnit = null;
						if (SkillManager.AttackInfoPanel.Displayed)
						{
							SkillManager.AttackInfoPanel.Hide();
						}
					}
					else
					{
						if (!SkillManager.AttackInfoPanel.Displayed)
						{
							SkillManager.AttackInfoPanel.Display();
						}
						SkillManager.AttackInfoPanel.TargetTile = tile;
						SkillManager.AttackInfoPanel.TargetUnit = unit;
						SkillManager.AttackInfoPanel.Refresh();
					}
				}
				else if (SelectedSkill.SkillDefinition.SkillActionDefinition is GenericSkillActionDefinition)
				{
					SkillManager.GenericActionInfoPanel.TargetTile = tile;
					SkillManager.GenericActionInfoPanel.TargetUnit = null;
					SkillManager.GenericActionInfoPanel.Refresh();
					if (!SkillManager.GenericActionInfoPanel.Displayed)
					{
						SkillManager.GenericActionInfoPanel.Display();
					}
				}
				if (SelectedSkill?.SkillDefinition.ValidTargets != null)
				{
					if (SelectedSkill.SkillDefinition.ValidTargets.AnyUnits)
					{
						if (tile != null && unit != null)
						{
							TheLastStand.Model.Unit.Unit unit2 = unit;
							unit2.UnitView.OnSkillTargetHover(hover: true);
						}
						if (previousTile != null)
						{
							TheLastStand.Model.Unit.Unit unit3 = previousTile.Unit;
							if (unit3 != null && unit3 != unit)
							{
								unit3.UnitView.OnSkillTargetHover(hover: false);
							}
						}
					}
					if (tile != null && tile.Building != null && SelectedSkill.SkillDefinition.ValidTargets.Buildings.ContainsKey(tile.Building.Id))
					{
						tile.Building.BuildingView.OnSkillTargetHover(hover: true);
					}
					if (previousTile != null && previousTile.Building != null)
					{
						previousTile.Building.BuildingView.OnSkillTargetHover(hover: false);
					}
					if (tile != null && SelectedSkill.Targets.Contains(tile))
					{
						tile.TileView.OnSkillTargetHover(hover: true);
					}
					if (previousTile != null && SelectedSkill.Targets.Contains(previousTile))
					{
						previousTile.TileView.OnSkillTargetHover(hover: false);
					}
				}
			}
			if (GameView.TopScreenPanel.UnitPortraitsPanel.TargettedPortraitHasChanged)
			{
				UnitPortraitView portraitIsHovered = GameView.TopScreenPanel.UnitPortraitsPanel.GetPortraitIsHovered();
				if (GameView.TopScreenPanel.UnitPortraitsPanel.CursorIsHoverPortrait)
				{
					portraitIsHovered.PlayableUnit.PlayableUnitView.OnSkillTargetHover(hover: true);
				}
				if (GameView.TopScreenPanel.UnitPortraitsPanel.GetPreviousPortraitWasHovered() != null)
				{
					GameView.TopScreenPanel.UnitPortraitsPanel.GetPreviousPortraitWasHovered().PlayableUnit.PlayableUnitView.OnSkillTargetHover(hover: false);
				}
			}
			if (InputManager.GetButtonDown(22) && GameView.TopScreenPanel.UnitPortraitsPanel.CursorIsHoverPortrait)
			{
				PlayableUnit playableUnit = GameView.TopScreenPanel.UnitPortraitsPanel.GetPortraitIsHovered().PlayableUnit;
				SkillManager.RefreshSelectedSkillValidityOnTile(playableUnit.OriginTile);
				if (SkillManager.IsSelectedSkillValid)
				{
					SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.AddTarget(playableUnit.OriginTile, SelectedSkill.CursorDependantOrientation, SkillManager.IsSelectedSkillFlipped);
					CastSelectedSkill();
					SoundManager.PlayAudioClip(SkillManager.AudioSource, SkillManager.SkillValidTileAudioClip);
				}
				else
				{
					SoundManager.PlayAudioClip(SkillManager.AudioSource, SkillManager.SkillInvalidTileAudioClip);
				}
			}
			else if (InputManager.GetButtonDown(0))
			{
				SelectedSkill = null;
				SelectNextUnit();
			}
			else if (InputManager.GetButtonDown(11))
			{
				SelectedSkill = null;
				SelectPreviousUnit();
			}
			else if (InputManager.GetButtonDown(23) || InputManager.GetButtonDown(137))
			{
				TPSingleton<TileObjectSelectionManager>.Instance.HasToWaitForNextFrame = true;
				if (SelectedSkill.SkillAction.HasEffect("MultiHit") && SelectedSkill.SkillAction.SkillActionExecution.TargetTiles.Count > 0)
				{
					SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.RemoveLastTarget();
				}
				else
				{
					SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.Reset();
					if (SelectedSkill.SkillAction is ResupplySkillAction resupplySkillAction && (resupplySkillAction.TryGetFirstEffect<ResupplyChargesSkillEffectDefinition>("ResupplyCharges", out var _) || resupplySkillAction.TryGetFirstEffect<ResupplyOverallUsesSkillEffectDefinition>("ResupplyOverallUses", out var _)))
					{
						resupplySkillAction.ResupplySkillActionExecution.ResupplySkillActionExecutionView.HideDisplayedHUD();
					}
					SelectedSkill = null;
					if (tile != null && !tile.HasFog && tile.Unit is EnemyUnit { State: not TheLastStand.Model.Unit.Unit.E_State.Dead } enemyUnit)
					{
						TPSingleton<EnemyUnitManager>.Instance.DisplayOneEnemyReachableTiles(enemyUnit);
					}
					else if (TileObjectSelectionManager.HasPlayableUnitSelected && TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.Unit.UnitController.MoveTask == null)
					{
						TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.ComputeReachableTiles();
						TPSingleton<TileObjectSelectionManager>.Instance.HasToWaitForNextFrame = true;
					}
				}
				TPSingleton<PlayableUnitManagementView>.Instance.SkillBar.JoystickSkillBar.DeselectCurrentSkill();
				SoundManager.PlayAudioClip(SkillManager.AudioSource, SkillManager.SkillCancelAudioClip);
			}
			else if (InputManager.GetButtonDown(83))
			{
				SelectNextSkill(nextSkill: false);
			}
			else if (InputManager.GetButtonDown(82))
			{
				SelectNextSkill(nextSkill: true);
			}
			else if (InputManager.GetButtonDown(136))
			{
				FocusCamOnSelectedPlayableUnit();
			}
			else
			{
				int unitIndexHotkeyPressed2 = TPSingleton<PlayableUnitManager>.Instance.GetUnitIndexHotkeyPressed();
				if (unitIndexHotkeyPressed2 != -1)
				{
					SelectedSkill = null;
					TileObjectSelectionManager.SetSelectedPlayableUnit(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[unitIndexHotkeyPressed2], CameraView.CameraUIMasksHandler.IsPointOffscreenOrHiddenByUI(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[unitIndexHotkeyPressed2].UnitView.transform.position));
				}
			}
			UpdateSkillHotkeysInput();
			break;
		case Game.E_State.Management:
			if (InputManager.GetButtonDown(83))
			{
				SelectNextSkill(nextSkill: false);
			}
			else if (InputManager.GetButtonDown(82))
			{
				SelectNextSkill(nextSkill: true);
			}
			else if (InputManager.GetButtonDown(23) || InputManager.GetButtonDown(137))
			{
				TPSingleton<PlayableUnitManagementView>.Instance.SkillBar.JoystickSkillBar.DeselectCurrentSkill();
			}
			else if (InputManager.GetButtonDown(136))
			{
				FocusCamOnSelectedPlayableUnit();
			}
			break;
		case Game.E_State.CharacterSheet:
			if (InputManager.GetButtonDown(60) && !InventoryManager.InventoryView.DraggableItem.Displayed)
			{
				ChangeEquipment();
			}
			break;
		case Game.E_State.NightReport:
		case Game.E_State.ProductionReport:
		{
			if (InputManager.GetButtonDown(0))
			{
				SelectNextUnit();
				int newUnitIndex = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.IndexOf(TileObjectSelectionManager.SelectedPlayableUnit);
				TPSingleton<ChooseRewardPanel>.Instance.ChangeUnitToCompareAndResetDropdown(newUnitIndex);
				break;
			}
			if (InputManager.GetButtonDown(11))
			{
				SelectPreviousUnit();
				int newUnitIndex2 = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.IndexOf(TileObjectSelectionManager.SelectedPlayableUnit);
				TPSingleton<ChooseRewardPanel>.Instance.ChangeUnitToCompareAndResetDropdown(newUnitIndex2);
				break;
			}
			if (InputManager.GetButtonDown(60))
			{
				ChangeEquipment();
				TPSingleton<PlayableUnitManagementView>.Instance.PlayableSkillBar.CheckSkillButtonsFocus();
				int newUnitIndex3 = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.IndexOf(TileObjectSelectionManager.SelectedPlayableUnit);
				TPSingleton<ChooseRewardPanel>.Instance.ChangeUnitToCompareAndResetDropdown(newUnitIndex3);
				break;
			}
			int unitIndexHotkeyPressed = TPSingleton<PlayableUnitManager>.Instance.GetUnitIndexHotkeyPressed();
			if (unitIndexHotkeyPressed != -1)
			{
				TileObjectSelectionManager.SetSelectedPlayableUnit(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[unitIndexHotkeyPressed], CameraView.CameraUIMasksHandler.IsPointOffscreenOrHiddenByUI(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[unitIndexHotkeyPressed].UnitView.transform.position));
				TPSingleton<ChooseRewardPanel>.Instance.ChangeUnitToCompareAndResetDropdown(unitIndexHotkeyPressed);
			}
			break;
		}
		}
		if (!DebugManager.DebugMode)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.F5))
		{
			DebugReplenishEverything();
		}
		else if (Input.GetKeyDown(KeyCode.F6))
		{
			ResourceManager.Debug_GainResources();
		}
		else if (Input.GetKeyDown(KeyCode.F7) && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.NightReport && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.CutscenePlaying && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.ProductionReport && !TPSingleton<NightTurnsManager>.Instance.IsEndingNight)
		{
			if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night)
			{
				GameManager.ExileAllEnemies(countAsKills: true, resetSpawnWave: true);
				TPSingleton<NightTurnsManager>.Instance.StartCoroutine(TPSingleton<NightTurnsManager>.Instance.EndNightCoroutine());
			}
			else
			{
				GameController.EndTurn();
			}
		}
	}

	private IEnumerator WaitForSkillExecution(TheLastStand.Model.Unit.Unit unit)
	{
		while (unit.IsExecutingSkill)
		{
			yield return SharedYields.WaitForEndOfFrame;
		}
		TheLastStand.Model.Skill.Skill previousSkillFlipped = null;
		if (SkillManager.IsSelectedSkillFlipped && SelectedSkill != null)
		{
			previousSkillFlipped = SelectedSkill;
		}
		SelectedSkill?.SkillAction.SkillActionExecution.SkillExecutionController.Reset();
		SelectedSkill = null;
		if (ShouldClearUndoStack)
		{
			unitsConversation.Clear();
		}
		GameView.BottomScreenPanel.BottomLeftPanel.CancelMovementPanel.Refresh();
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.UnitExecutingSkill && !TPSingleton<NightTurnsManager>.Instance.IsEndingNight)
		{
			GameController.SetState(Game.E_State.Management);
			if (InputManager.IsLastControllerJoystick)
			{
				SkillManager.PreviousSkillFlipped = previousSkillFlipped;
				movePath.MovePathController.Clear();
				TPSingleton<PlayableUnitManagementView>.Instance.SkillBar.JoystickSkillBar.SelectCurrentOrPreviousSkill();
				SkillManager.RefreshSelectedSkillValidityOnTile(TPSingleton<GameManager>.Instance.Game.Cursor.Tile);
			}
		}
	}

	[DevConsoleCommand("AddTrait")]
	public static void DebugAddTrait([StringConverter(typeof(PlayableUnit.StringToTraitIdConverter))] string traitId, bool forceAdd = false)
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select a unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.AddTrait(traitId, forceAdd);
		TPSingleton<CharacterSheetPanel>.Instance.Refresh();
		UnitManagementView<PlayableUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("AddStatus")]
	public static void DebugAddStatus([StringConverter(typeof(Status.StringToStatusForDefaultCommand))] string statusId, int turnsCount, int value = 1)
	{
		if (!TileObjectSelectionManager.HasUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select an unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		if (!Enum.TryParse<Status.E_StatusType>(statusId, out var result))
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Incorrect Status (unable to parse)", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		StatusCreationInfo statusCreationInfo = new StatusCreationInfo
		{
			Source = null,
			TurnsCount = turnsCount,
			Value = value
		};
		SkillManager.AddStatus(TileObjectSelectionManager.SelectedUnit, result, statusCreationInfo);
	}

	[DevConsoleCommand("AddStatusWithStat")]
	public static void DebugAddStatusWithStat([StringConverter(typeof(Status.StringToStatusWithStatCommand))] string statusId, [StringConverter(typeof(PlayableUnit.StringToStatIdConverter))] string statId, int turnsCount, int value = 1)
	{
		if (!TileObjectSelectionManager.HasUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select an unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		if (!Enum.TryParse<Status.E_StatusType>(statusId, out var result))
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Incorrect Status (unable to parse)", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		if (!Enum.TryParse<UnitStatDefinition.E_Stat>(statId, out var result2))
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Incorrect E_Stat (unable to parse)", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		StatusCreationInfo statusCreationInfo = new StatusCreationInfo
		{
			Source = null,
			Stat = result2,
			TurnsCount = turnsCount,
			Value = value
		};
		SkillManager.AddStatus(TileObjectSelectionManager.SelectedUnit, result, statusCreationInfo);
	}

	[DevConsoleCommand("AddAllImmunity")]
	public static void DebugAddAllImmunity()
	{
		DebugAddStatus("PoisonImmunity", 2, 25);
		DebugAddStatus("StunImmunity", 5, 50);
		DebugAddStatus("DebuffImmunity", 5, 50);
		DebugAddStatus("AllNegativeImmunity", 5, 50);
	}

	[DevConsoleCommand("ChangeRace")]
	public static void DebugChangeRace([StringConverter(typeof(PlayableUnit.StringToRaceIdConverter))] string raceId)
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select a unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		PlayableUnit selectedPlayableUnit = TileObjectSelectionManager.SelectedPlayableUnit;
		RaceDefinition raceDefinition = selectedPlayableUnit.RaceDefinition;
		if (raceDefinition.Id == raceId)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please another race to change to, current race is: " + raceDefinition.Id, TPSingleton<PlayableUnitManager>.Instance);
		}
		else
		{
			if (!PlayableUnitDatabase.RaceDefinitions.TryGetValue(raceId, out var value))
			{
				return;
			}
			selectedPlayableUnit.RaceDefinition = value;
			selectedPlayableUnit.PlayableUnitStatsController.OnRaceRemoved(raceDefinition);
			selectedPlayableUnit.PlayableUnitStatsController.OnRaceGenerated(value);
			foreach (string perksId in raceDefinition.PerksIds)
			{
				if (selectedPlayableUnit.Perks.ContainsKey(perksId))
				{
					selectedPlayableUnit.Perks[perksId].PerkController.LockAndClearUnlockers();
					selectedPlayableUnit.Perks.Remove(perksId);
				}
			}
			selectedPlayableUnit.PlayableUnitController.DebugGenerateRacePerks();
			Debug_RerollPerks();
			TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitView.InitVisuals(playSpawnAnim: false);
			TPSingleton<CharacterSheetPanel>.Instance.Refresh();
			UnitManagementView<PlayableUnitManagementView>.Refresh();
		}
	}

	[DevConsoleCommand("DisableHealthDisplay")]
	public static void Debug_DisableHealthDisplay()
	{
		TPSingleton<PlayableUnitManager>.Instance.debugDisableHealthDisplay = true;
		RefreshHealthDisplays();
	}

	[DevConsoleCommand("EnableHealthDisplay")]
	public static void Debug_EnableHealthDisplay()
	{
		TPSingleton<PlayableUnitManager>.Instance.debugDisableHealthDisplay = false;
		RefreshHealthDisplays();
	}

	[DevConsoleCommand("GainActionPoints")]
	public static void DebugGainActionPoints(int amount = 100)
	{
		if (!TileObjectSelectionManager.HasUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select a unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		TileObjectSelectionManager.SelectedUnit.UnitStatsController.IncreaseBaseStat(UnitStatDefinition.E_Stat.ActionPoints, amount, includeChildStat: false);
		TPSingleton<CharacterSheetPanel>.Instance.Refresh();
		UnitManagementView<PlayableUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("GainArmor")]
	public static void DebugGainArmor(int amount = 1000)
	{
		if (!TileObjectSelectionManager.HasUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select a unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		TileObjectSelectionManager.SelectedUnit.UnitStatsController.IncreaseBaseStat(UnitStatDefinition.E_Stat.Armor, amount, includeChildStat: true);
		if (TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<CharacterSheetPanel>.Instance.Refresh();
		}
		UnitManagementView<PlayableUnitManagementView>.Refresh();
		UnitManagementView<EnemyUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("GainExperience")]
	public static void DebugGainExperience(int amount = 1000000)
	{
		if (!TileObjectSelectionManager.HasUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select a unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.GainExperience(amount);
		TPSingleton<CharacterSheetPanel>.Instance.Refresh();
		UnitManagementView<PlayableUnitManagementView>.Refresh();
		TPSingleton<ToDoListView>.Instance.RefreshUnitLevelUpNotification();
	}

	[DevConsoleCommand("GainLevel")]
	public static void DebugGainLevel()
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select a unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.LevelUp();
		TileObjectSelectionManager.SelectedPlayableUnit.ExperienceInCurrentLevel = 0f;
		TPSingleton<CharacterSheetPanel>.Instance.Refresh();
		UnitManagementView<PlayableUnitManagementView>.Refresh();
		TPSingleton<ToDoListView>.Instance.RefreshUnitLevelUpNotification();
	}

	[DevConsoleCommand("GainHealth")]
	public static void DebugGainHealth(int amount = 1000)
	{
		if (!TileObjectSelectionManager.HasUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select a unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		TileObjectSelectionManager.SelectedUnit.UnitController.GainHealth(amount);
		if (TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<CharacterSheetPanel>.Instance.Refresh();
		}
		UnitManagementView<PlayableUnitManagementView>.Refresh();
		UnitManagementView<EnemyUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("GainMana")]
	public static void DebugGainMana(int amount = 100)
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select a unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.GainMana(amount);
		TPSingleton<CharacterSheetPanel>.Instance.Refresh();
		UnitManagementView<PlayableUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("GainMovePoints")]
	public static void DebugGainMovePoints(int amount = 100)
	{
		if (!TileObjectSelectionManager.HasUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select a unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		TileObjectSelectionManager.SelectedUnit.UnitStatsController.IncreaseBaseStat(UnitStatDefinition.E_Stat.MovePoints, amount, includeChildStat: false);
		if (TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<CharacterSheetPanel>.Instance.Refresh();
			if (TPSingleton<PlayableUnitManager>.Instance.HasToRecomputeReachableTiles)
			{
				TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.ComputeReachableTiles();
			}
		}
		UnitManagementView<PlayableUnitManagementView>.Refresh();
		UnitManagementView<EnemyUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("GetStat")]
	public static void DebugGetStat([StringConverter(typeof(PlayableUnit.StringToStatIdConverter))] string statId, bool isFinalValue = false)
	{
		if (!TileObjectSelectionManager.HasUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select a unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		UnitStatDefinition.E_Stat stat = (UnitStatDefinition.E_Stat)Enum.Parse(typeof(UnitStatDefinition.E_Stat), statId);
		float num = (isFinalValue ? TileObjectSelectionManager.SelectedUnit.UnitStatsController.GetStat(stat).FinalClamped : TileObjectSelectionManager.SelectedUnit.UnitStatsController.GetStat(stat).Base);
		TPSingleton<DebugManager>.Instance.LogDevConsole(num);
	}

	[DevConsoleCommand("GainPerkPoints")]
	public static void GainPerkPoints(int perksPoints = 1)
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select a unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
		}
		else
		{
			TileObjectSelectionManager.SelectedPlayableUnit.PerksPoints += perksPoints;
		}
	}

	[DevConsoleCommand("RemoveStatus")]
	public static void DebugRemoveStatus([StringConverter(typeof(Status.StringToStatusForDefaultCommand))] string statusId)
	{
		if (!TileObjectSelectionManager.HasUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select an unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		if (!Enum.TryParse<Status.E_StatusType>(statusId, out var result))
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Incorrect Status (unable to parse)", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		TileObjectSelectionManager.SelectedUnit.UnitController.RemoveStatus(result);
		TileObjectSelectionManager.SelectedUnit.UnitView.RefreshArmor();
		TileObjectSelectionManager.SelectedUnit.UnitView.RefreshHealth();
		TileObjectSelectionManager.SelectedUnit.UnitView.RefreshInjuryStage();
		TileObjectSelectionManager.SelectedUnit.UnitView.RefreshStatus();
		UnitManagementView<PlayableUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("ReplenishEquippedSkills")]
	public static void DebugReplenishEquippedSkills()
	{
		for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i++)
		{
			TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].PlayableUnitController.StartEquipmentTurn();
			TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].PlayableUnitController.ResetContextualSkillsTurnUses();
			TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].PlayableUnitController.RefillContextualSkillsOverallUses();
			foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].EquipmentSlots)
			{
				for (int num = equipmentSlot.Value.Count - 1; num >= 0; num--)
				{
					equipmentSlot.Value[num].Item?.ItemController.RefillOverallUses();
				}
			}
		}
		UnitManagementView<PlayableUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("ReplenishEverything")]
	public static void DebugReplenishEverything()
	{
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management && TileObjectSelectionManager.SelectedBuilding == null && TileObjectSelectionManager.SelectedUnit != null)
		{
			if (TileObjectSelectionManager.SelectedPlayableUnit != null)
			{
				DebugGainActionPoints();
				DebugGainMana();
				DebugReplenishEquippedSkills();
			}
			DebugGainArmor();
			DebugGainHealth();
			DebugGainMovePoints();
			TileObjectSelectionManager.SelectedUnit.UnitView.RefreshInjuryStage();
		}
	}

	[DevConsoleCommand("SetFaceId")]
	public static void DebugSetFaceId([StringConverter(typeof(PlayableUnit.StringToFaceIdConverter))] string faceId)
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select a unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		if (PlayableUnitDatabase.GetFaceIdsForGender(TileObjectSelectionManager.SelectedPlayableUnit.Gender).Contains(faceId))
		{
			GenerateNewPortrait(TileObjectSelectionManager.SelectedPlayableUnit, faceId, changeBackgroundColor: false, changeHairSkinEyesColor: false);
			return;
		}
		TPSingleton<PlayableUnitManager>.Instance.LogError("faceId '" + faceId + "' doesn't exist in the " + ((TileObjectSelectionManager.SelectedPlayableUnit.Gender == "Male") ? "PlayableMaleUnitFaceIds" : "PlayableFemaleUnitFaceIds") + " list.", TPSingleton<PlayableUnitManager>.Instance);
	}

	private static void GenerateNewPortrait(PlayableUnit playableUnit, string faceId, bool changeBackgroundColor, bool changeHairSkinEyesColor)
	{
		if (changeBackgroundColor)
		{
			PlayableUnitView.GetRandomPortraitBGColor(playableUnit);
		}
		playableUnit.PlayableUnitController.DebugSetFaceId(faceId);
		playableUnit.PlayableUnitView.DebugRefreshColorSwapping();
		TileObjectSelectionManager.SetSelectedPlayableUnit(playableUnit);
	}

	[DevConsoleCommand("GenerateRandomPortrait")]
	public static void DebugGenerateRandomPortrait(bool changeFaceId = true, bool changeBackgroundColor = true, bool changeHairSkinEyesColor = true)
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select a unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		string faceId = TileObjectSelectionManager.SelectedPlayableUnit.FaceId;
		if (changeFaceId)
		{
			faceId = PlayableUnitDatabase.GetFaceIdsForGender(TileObjectSelectionManager.SelectedPlayableUnit.Gender).RandomElement();
		}
		GenerateNewPortrait(TileObjectSelectionManager.SelectedPlayableUnit, faceId, changeBackgroundColor, changeHairSkinEyesColor);
	}

	[DevConsoleCommand("RemoveTrait")]
	public static void DebugRemoveTrait([StringConverter(typeof(PlayableUnit.StringToCurrentTraitsConverter))] string traitId)
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select a unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		UnitTraitDefinition unitTraitDefinition = TileObjectSelectionManager.SelectedPlayableUnit.UnitTraitDefinitions.Find((UnitTraitDefinition trait) => trait.Id == traitId);
		if (unitTraitDefinition != null)
		{
			TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.RemoveTrait(unitTraitDefinition);
			TPSingleton<CharacterSheetPanel>.Instance.Refresh();
			UnitManagementView<PlayableUnitManagementView>.Refresh();
			return;
		}
		TPSingleton<PlayableUnitManager>.Instance.LogError("traitId '" + traitId + "' doesn't exist in " + TileObjectSelectionManager.SelectedUnit.Id + " traits.", TPSingleton<PlayableUnitManager>.Instance);
	}

	[DevConsoleCommand("RemoveAllTraits")]
	public static void DebugRemoveAllTraits()
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select a unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		for (int num = TileObjectSelectionManager.SelectedPlayableUnit.UnitTraitDefinitions.Count - 1; num >= 0; num--)
		{
			TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.RemoveTrait(TileObjectSelectionManager.SelectedPlayableUnit.UnitTraitDefinitions[num]);
		}
		TPSingleton<CharacterSheetPanel>.Instance.Refresh();
		UnitManagementView<PlayableUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("SetHairPalette")]
	public static void DebugSetHairPalette([StringConverter(typeof(PlayableUnit.StringToHairPaletteIdConverter))] string hairPaletteId)
	{
		ColorSwapPaletteDefinition value;
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select a unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
		}
		else if (PlayableUnitDatabase.PlayableUnitHairColorDefinitions.TryGetValue(hairPaletteId, out value))
		{
			TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.DebugSetColorPalette(value, isHairPalette: true);
		}
	}

	[DevConsoleCommand("SetRandomPortraitBGColor")]
	public static void DebugSetRandomPortraitBGColor()
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select a unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		PlayableUnitView.RemoveUsedPortraitColor(TileObjectSelectionManager.SelectedPlayableUnit.PortraitColor);
		PlayableUnitView.GetRandomPortraitBGColor(TileObjectSelectionManager.SelectedPlayableUnit);
		PlayableUnitManagementView.UnitPortraitView.RefreshPortrait();
		GameView.TopScreenPanel.UnitPortraitsPanel.RefreshPortraits();
	}

	[DevConsoleCommand("SetSkinPalette")]
	public static void DebugSetSkinPalette([StringConverter(typeof(PlayableUnit.StringToSkinPaletteIdConverter))] string skinPaletteId)
	{
		ColorSwapPaletteDefinition value;
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select an unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
		}
		else if (PlayableUnitDatabase.PlayableUnitSkinColorDefinitions.TryGetValue(skinPaletteId, out value))
		{
			TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.DebugSetColorPalette(value, isHairPalette: false);
		}
	}

	[DevConsoleCommand("SetStat")]
	public static void DebugSetStat([StringConverter(typeof(PlayableUnit.StringToStatIdConverter))] string statId, float newStatValue)
	{
		if (!TileObjectSelectionManager.HasUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select an unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		TheLastStand.Model.Unit.Unit selectedUnit = TileObjectSelectionManager.SelectedUnit;
		UnitStatDefinition.E_Stat e_Stat = (UnitStatDefinition.E_Stat)Enum.Parse(typeof(UnitStatDefinition.E_Stat), statId);
		if (selectedUnit is PlayableUnit playableUnit)
		{
			PlayableUnitStat stat = playableUnit.PlayableUnitStatsController.GetStat(e_Stat);
			float num = newStatValue - stat.FinalClamped;
			if (num < 0f)
			{
				playableUnit.PlayableUnitStatsController.DecreaseBaseStat(e_Stat, Mathf.Abs(num), includeChildStat: true);
			}
			else if (num > 0f)
			{
				playableUnit.PlayableUnitStatsController.IncreaseBaseStat(e_Stat, num, includeChildStat: true);
			}
		}
		else
		{
			selectedUnit.UnitStatsController.SetBaseStat(e_Stat, newStatValue);
		}
		selectedUnit.UnitController.UpdateInjuryStage();
		if (TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			UnitManagementView<PlayableUnitManagementView>.Refresh();
			TPSingleton<CharacterSheetPanel>.Instance.Refresh();
		}
		else if (TileObjectSelectionManager.HasEnemyUnitSelected)
		{
			UnitManagementView<EnemyUnitManagementView>.Refresh();
			EnemyUnitManager.EnemyUnitInfoPanel.Refresh();
		}
		selectedUnit.UnitView.RefreshInjuryStage();
		selectedUnit.UnitController.DisplayEffects();
	}

	[DevConsoleCommand("PlayableUnitToggleAnimator")]
	public static void DebugPlayableUnitToggleAnimator()
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select a playable unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		PlayableUnit selectedPlayableUnit = TileObjectSelectionManager.SelectedPlayableUnit;
		int count = TPSingleton<PlayableUnitManager>.Instance.debugPlayableUnitsAnimatorControllers.Count;
		if (selectedPlayableUnit != null && count > 0)
		{
			string currentAnimatorControllerName = selectedPlayableUnit.PlayableUnitView.DebugGetCurrentAnimatorControllerName();
			int num = TPSingleton<PlayableUnitManager>.Instance.debugPlayableUnitsAnimatorControllers.FindIndex((RuntimeAnimatorController animatorController) => animatorController.name == currentAnimatorControllerName);
			int index = ((num + 1 < count) ? (num + 1) : 0);
			selectedPlayableUnit.PlayableUnitView.DebugChangeAnimatorController(TPSingleton<PlayableUnitManager>.Instance.debugPlayableUnitsAnimatorControllers[index]);
			TPSingleton<DebugManager>.Instance.LogDevConsole("Changed " + selectedPlayableUnit.Id + " animator controller from " + currentAnimatorControllerName + " to " + TPSingleton<PlayableUnitManager>.Instance.debugPlayableUnitsAnimatorControllers[index].name);
		}
	}

	[DevConsoleCommand(Name = "ForceSkipNightReport")]
	private static void DebugSkipNightReport(bool forceSkipNightReport = true)
	{
		TPSingleton<PlayableUnitManager>.Instance.debugForceSkipNightReport = forceSkipNightReport;
	}

	[DevConsoleCommand("ToggleHealthDisplay")]
	public static void Debug_ToggleHealthDisplay()
	{
		TPSingleton<PlayableUnitManager>.Instance.debugDisableHealthDisplay = !TPSingleton<PlayableUnitManager>.Instance.debugDisableHealthDisplay;
		RefreshHealthDisplays();
	}

	[DevConsoleCommand("DismissHeroToggleValidityChecks")]
	private static void DebugToggleDismissHeroChecks(bool check = false)
	{
		TPSingleton<PlayableUnitManager>.Instance.debugToggleDismissHeroValidityChecks = check;
		TPSingleton<CharacterSheetPanel>.Instance.Refresh();
	}

	[DevConsoleCommand("EquipmentSlotAdd")]
	private static void Debug_EquipmentSlotAdd(ItemSlotDefinition.E_ItemSlotId slotType = ItemSlotDefinition.E_ItemSlotId.Usables, int amount = 1)
	{
		PlayableUnit reliablePlayableUnit = TileObjectSelectionManager.ReliablePlayableUnit;
		UnitTraitDefinition.SlotModifier slotModifier = new UnitTraitDefinition.SlotModifier(slotType, amount, addSlot: true, string.Empty);
		for (int i = 0; i < slotModifier.Amount; i++)
		{
			List<EquipmentSlotView> list = CharacterSheetPanel.EquipmentSlots[slotModifier.Name];
			int num = 0;
			if (reliablePlayableUnit.EquipmentSlots.TryGetValue(slotModifier.Name, out var value))
			{
				num = value.Count;
			}
			if (reliablePlayableUnit.EquipmentSlots.ContainsKey(slotModifier.Name))
			{
				if (list.Count - 1 >= num)
				{
					EquipmentSlot equipmentSlot = new EquipmentSlotController(ItemDatabase.ItemSlotDefinitions[slotModifier.Name], list[num], reliablePlayableUnit).EquipmentSlot;
					reliablePlayableUnit.EquipmentSlots[slotModifier.Name].Add(equipmentSlot);
					list[num].ItemSlot = equipmentSlot;
					list[num].Refresh();
				}
			}
			else
			{
				EquipmentSlot equipmentSlot2 = new EquipmentSlotController(ItemDatabase.ItemSlotDefinitions[slotModifier.Name], list[num], reliablePlayableUnit).EquipmentSlot;
				reliablePlayableUnit.EquipmentSlots.Add(slotModifier.Name, new List<EquipmentSlot>());
				reliablePlayableUnit.EquipmentSlots[slotModifier.Name].Add(equipmentSlot2);
				list[num].ItemSlot = equipmentSlot2;
				list[num].Refresh();
			}
		}
	}

	[DevConsoleCommand("EquipmentSlotRemove")]
	private static void Debug_EquipmentSlotRemove(ItemSlotDefinition.E_ItemSlotId slotId = ItemSlotDefinition.E_ItemSlotId.Usables, int amount = 1)
	{
		PlayableUnit reliablePlayableUnit = TileObjectSelectionManager.ReliablePlayableUnit;
		UnitTraitDefinition.SlotModifier slotModifier = new UnitTraitDefinition.SlotModifier(slotId, amount, addSlot: false, string.Empty);
		for (int i = 0; i < slotModifier.Amount; i++)
		{
			List<EquipmentSlotView> list = CharacterSheetPanel.EquipmentSlots[slotModifier.Name];
			if (!reliablePlayableUnit.EquipmentSlots.ContainsKey(slotId))
			{
				continue;
			}
			int index = reliablePlayableUnit.EquipmentSlots[slotModifier.Name].Count - 1;
			reliablePlayableUnit.EquipmentSlots[slotId].RemoveAt(reliablePlayableUnit.EquipmentSlots[slotId].Count - 1);
			list[index].ItemSlot = null;
			list[index].Refresh();
			if (reliablePlayableUnit.EquipmentSlots[slotId].Count == 0)
			{
				reliablePlayableUnit.EquipmentSlots.Remove(slotId);
				if (slotId == ItemSlotDefinition.E_ItemSlotId.LeftHand)
				{
					reliablePlayableUnit.BodyParts["Arm_L"].ChangeAdditionalConstraint("Hide", add: true);
				}
			}
		}
	}

	[DevConsoleCommand("MomentumAddValue")]
	private static void Debug_MomentumAddValue(int valueToAdd)
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select a playable unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		TileObjectSelectionManager.SelectedPlayableUnit.MomentumTilesActive += valueToAdd;
		UnitManagementView<PlayableUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("PerkBufferReset")]
	private static void Debug_ResetPerkBuffer([StringConverter(typeof(StringToActivePerkIdConverter))] string perkId)
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Please select a playable unit before firing this command", TPSingleton<PlayableUnitManager>.Instance);
			return;
		}
		if (TileObjectSelectionManager.SelectedPlayableUnit.Perks.TryGetValue(perkId, out var value))
		{
			foreach (APerkModule perkModule in value.PerkModules)
			{
				perkModule.ResetDynamicData();
			}
		}
		UnitManagementView<PlayableUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("PerksUnlockAllTiers")]
	private static void Debug_UnlockAllPerksTiers()
	{
		TileObjectSelectionManager.SelectedPlayableUnit.PerkTree.UnitPerkTreeController.DebugUpdateAllTiersAvailability();
	}

	[DevConsoleCommand("PerkUnlock")]
	private static void Debug_UnlockPerk([StringConverter(typeof(StringToPerkIdConverter))] string perkId)
	{
		TileObjectSelectionManager.SelectedPlayableUnit.PerkTree.UnitPerkTreeController.UnlockPerk(perkId);
		UnitManagementView<PlayableUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("PerkLock")]
	private static void Debug_LockPerk([StringConverter(typeof(StringToUnlockedPerkIdConverter))] string perkId)
	{
		TileObjectSelectionManager.SelectedPlayableUnit.PerkTree.UnitPerkTreeController.LockPerk(perkId);
		UnitManagementView<PlayableUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("PerkUnlockAll")]
	private static void Debug_UnlockAllPerks()
	{
		PlayableUnit selectedPlayableUnit = TileObjectSelectionManager.SelectedPlayableUnit;
		foreach (Perk item in selectedPlayableUnit.PerkTree.UnitPerkTiers.SelectMany((UnitPerkTier perkTier) => perkTier.Perks))
		{
			if (item != null && !selectedPlayableUnit.Perks.ContainsKey(item.PerkDefinition.Id))
			{
				selectedPlayableUnit.PerkTree.UnitPerkTreeController.UnlockPerk(item.PerkDefinition.Id);
			}
		}
		UnitManagementView<PlayableUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("PerkReroll")]
	private static void Debug_RerollPerks()
	{
		PlayableUnit selectedPlayableUnit = TileObjectSelectionManager.SelectedPlayableUnit;
		if (selectedPlayableUnit == null)
		{
			return;
		}
		foreach (KeyValuePair<string, Perk> perk in selectedPlayableUnit.Perks)
		{
			if (perk.Value.PerkTier != null)
			{
				selectedPlayableUnit.Perks[perk.Key].PerkController.Lock(selectedPlayableUnit);
			}
		}
		TPSingleton<PlayableUnitManager>.Instance.StartCoroutine(Debug_RerollPerksCoroutine(selectedPlayableUnit));
	}

	[DevConsoleCommand("PerkLineReroll")]
	private static void Debug_RerollPerksLine(int lineIndex = 0)
	{
		PlayableUnit selectedPlayableUnit = TileObjectSelectionManager.SelectedPlayableUnit;
		if (selectedPlayableUnit != null && lineIndex < selectedPlayableUnit.PerkTree.UnitPerkTiers.Count)
		{
			selectedPlayableUnit.PerkTree.UnitPerkTreeController.RerollPerksInTier(lineIndex, payDamnedSouls: false);
			UnitManagementView<PlayableUnitManagementView>.Refresh();
			TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.Refresh();
		}
	}

	[DevConsoleCommand("PerkColumnReroll")]
	private static void Debug_RerollPerksColumn(int columnIndex = 0)
	{
		PlayableUnit selectedPlayableUnit = TileObjectSelectionManager.SelectedPlayableUnit;
		if (selectedPlayableUnit == null)
		{
			return;
		}
		List<UnitPerkCollectionDefinition> list = selectedPlayableUnit.PerkTree.UnitPerkCollectionIds.Select((string id) => PlayableUnitDatabase.UnitPerkCollectionDefinitions[id]).ToList();
		HashSet<int> lockedPerkCollectionSlots = TPSingleton<MetaUpgradesManager>.Instance.GetLockedPerkCollectionSlots();
		while (lockedPerkCollectionSlots.Contains(columnIndex + 1))
		{
			if (columnIndex >= list.Count)
			{
				return;
			}
			columnIndex++;
		}
		if (selectedPlayableUnit.PerkTree.UnitPerkTreeController.TryRerollPerksInCollection(columnIndex, payDamnedSouls: false))
		{
			UnitManagementView<PlayableUnitManagementView>.Refresh();
			TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.Refresh();
		}
	}

	private static IEnumerator Debug_RerollPerksCoroutine(PlayableUnit playableUnit)
	{
		yield return new WaitForSeconds(0.1f + TPSingleton<PlayableUnitManager>.Instance.perkReplacementRandomDelay.y);
		playableUnit.PerkTree.UnitPerkTiers.Clear();
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, Perk> perk in playableUnit.Perks)
		{
			Perk value = perk.Value;
			if (value.PerkTier != null || !perk.Value.Unlocked)
			{
				if (value.OnlyUnlockedByItem)
				{
					value.PerkController.ChangePerkTierAndView(null, null);
				}
				else
				{
					list.Add(perk.Key);
				}
			}
		}
		foreach (string item in list)
		{
			playableUnit.Perks.Remove(item);
		}
		playableUnit.PerkTree.UnitPerkTreeController.GeneratePerkTree(playableUnit, checkExistingPerks: true);
		TPSingleton<CharacterSheetPanel>.Instance.Refresh();
		UnitManagementView<PlayableUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("PerkShowAll")]
	private static void Debug_PerkShowAll()
	{
		PlayableUnit selectedPlayableUnit = TileObjectSelectionManager.SelectedPlayableUnit;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Perk value in selectedPlayableUnit.Perks.Values)
		{
			stringBuilder.Append($"id: {value.PerkDefinition.Id}, unlocked: {value.Unlocked}, unlockers nb: {value.Unlockers.Count}").AppendLine();
		}
		Debug.Log(stringBuilder.ToString());
	}

	[DevConsoleCommand("UltimateCheat")]
	private static void Debug_UltimateCheat()
	{
		if (TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			DebugSetStat("PhysicalDamage", 500f);
			DebugSetStat("MagicalDamage", 500f);
			DebugSetStat("RangedDamage", 500f);
			DebugSetStat("OverallDamage", 500f);
			DebugSetStat("Accuracy", 500f);
			DebugSetStat("Reliability", 500f);
			DebugSetStat("MovePointsTotal", 500f);
			DebugSetStat("MovePoints", 500f);
			DebugSetStat("ActionPointsTotal", 500f);
			DebugSetStat("ActionPoints", 500f);
			DebugSetStat("HealthTotal", 1000f);
			DebugSetStat("Health", 1000f);
			DebugSetStat("ArmorTotal", 1000f);
			DebugSetStat("Armor", 1000f);
			DebugSetStat("ManaTotal", 500f);
			DebugSetStat("Mana", 500f);
			DebugSetStat("Resistance", 500f);
			DebugSetStat("Dodge", 500f);
			DebugSetStat("Block", 500f);
			DebugSetStat("PropagationBouncesModifier", 500f);
			DebugSetStat("PropagationDamage", 500f);
			DebugSetStat("PoisonDamageModifier", 500f);
			DebugSetStat("Critical", 0f);
			DebugSetStat("CriticalPower", 100f);
		}
		ItemManager.DebugGenerateItem("HandCrossbow0");
		ItemManager.DebugGenerateItem("DruidicStaff0");
		ItemManager.DebugGenerateItem("TomeOfMagic0");
		InventoryManager.Debug_ForceInventoryAccess();
		ShopManager.DebugForceShopAccess();
		BuildingManager.DebugToggleMagicCircleIndestructibility();
		TurnEndValidationManager.DebugByPassEndTurnChecks();
		SkillManager.DebugToggleSkillCastValidityCheck();
		SkillManager.DebugSetSkillsAllowAllPhases();
		ConstructionManager.DBG_ForceConstructionAllowed();
		ResourceManager.Debug_GainResources();
		MetaShopsManager.DebugForceOraculumAccess();
	}

	[DevConsoleCommand("SetStatSwole")]
	private static void Debug_SetStatSwole()
	{
		if (TileObjectSelectionManager.SelectedPlayableUnit != null)
		{
			DebugRemoveAllTraits();
			DebugSetStat("PhysicalDamage", 500f);
			DebugSetStat("MagicalDamage", 500f);
			DebugSetStat("RangedDamage", 500f);
			DebugSetStat("OverallDamage", 500f);
			DebugSetStat("Accuracy", 500f);
			DebugSetStat("Critical", 500f);
			DebugSetStat("CriticalPower", 500f);
			DebugSetStat("Reliability", 500f);
			DebugSetStat("MovePointsTotal", 500f);
			DebugSetStat("MovePoints", 500f);
			DebugSetStat("ActionPointsTotal", 500f);
			DebugSetStat("ActionPoints", 500f);
			DebugSetStat("HealthTotal", 1000f);
			DebugSetStat("Health", 1000f);
			DebugSetStat("ArmorTotal", 1000f);
			DebugSetStat("Armor", 1000f);
			DebugSetStat("ManaTotal", 500f);
			DebugSetStat("Mana", 500f);
			DebugSetStat("Resistance", 500f);
			DebugSetStat("Dodge", 500f);
			DebugSetStat("Block", 500f);
			DebugSetStat("PropagationBouncesModifier", 500f);
			DebugSetStat("PropagationDamage", 500f);
			DebugSetStat("PoisonDamageModifier", 500f);
			DebugSetStat("MultiHitsCountModifier", 500f);
			DebugSetStat("SkillRangeModifier", 500f);
		}
	}

	[DevConsoleCommand("BestFiendShowList")]
	private static void Debug_BestFiendShowList()
	{
		PlayableUnit selectedPlayableUnit = TileObjectSelectionManager.SelectedPlayableUnit;
		if (selectedPlayableUnit == null)
		{
			return;
		}
		string text = string.Empty;
		float num = 0f;
		foreach (KeyValuePair<string, float> damagesInflictedToEnemy in selectedPlayableUnit.LifetimeStats.DamagesInflictedToEnemies)
		{
			num += damagesInflictedToEnemy.Value;
			text += $"\n{damagesInflictedToEnemy.Key} : {damagesInflictedToEnemy.Value}";
		}
		text += $"\nTotal : {num}";
		selectedPlayableUnit.Log(text, CLogLevel.MAJOR, forcePrintInUnity: true);
		TPSingleton<DebugManager>.Instance.LogDevConsole(text);
	}

	private static void RefreshHealthDisplays()
	{
		foreach (PlayableUnit playableUnit in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
		{
			if (playableUnit.PlayableUnitView != null && playableUnit.PlayableUnitView.UnitHUD != null && playableUnit.PlayableUnitView.UnitHUD.BgHighlightCanvas != null)
			{
				playableUnit.PlayableUnitView.UnitHUD.BgHighlightCanvas.enabled = !TPSingleton<PlayableUnitManager>.Instance.debugDisableHealthDisplay;
				playableUnit.PlayableUnitView.UnitHUD.DisplayHealthIfNeeded();
			}
		}
		foreach (EnemyUnit enemyUnit in TPSingleton<EnemyUnitManager>.Instance.EnemyUnits)
		{
			if (enemyUnit.EnemyUnitView != null && enemyUnit.EnemyUnitView.UnitHUD != null)
			{
				enemyUnit.EnemyUnitView?.UnitHUD?.DisplayHealthIfNeeded();
			}
		}
		foreach (TheLastStand.Model.Building.Building building in TPSingleton<BuildingManager>.Instance.Buildings)
		{
			if (building.BuildingView != null && building.BuildingView.BuildingHUD != null)
			{
				building.BuildingView.BuildingHUD.DisplayHealthIfNeeded();
			}
		}
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		PlayableUnits = new List<PlayableUnit>();
		DeadPlayableUnits = new Dictionary<int, List<PlayableUnit>>();
		PlayableUnitView.UsedUnitPortrait.Clear();
		if (container is SerializedPlayableUnits serializedPlayableUnits)
		{
			foreach (SerializedPlayableUnit ownedUnit in serializedPlayableUnits.OwnedUnits)
			{
				PlayableUnit playableUnit = new PlayableUnitController(ownedUnit, saveVersion).PlayableUnit;
				InstantiateUnit(playableUnit, playableUnit.OriginTile, saveVersion, onLoad: true);
			}
			TPSingleton<MetaConditionManager>.Instance.RefreshMaxPlayableUnitStatReached(PlayableUnits);
			Recruitment = new Recruitment(serializedPlayableUnits.Recruitment, saveVersion);
			foreach (SerializedDeadUnit deadUnit in serializedPlayableUnits.DeadUnits)
			{
				if (!DeadPlayableUnits.ContainsKey(deadUnit.DeathTurn))
				{
					DeadPlayableUnits.Add(deadUnit.DeathTurn, new List<PlayableUnit>());
				}
				PlayableUnit playableUnit2 = new PlayableUnitController(deadUnit.Unit, -1, isDead: true).PlayableUnit;
				InstantiateDeadUnit(playableUnit2, TileMapManager.GetTile(0, 0));
				DeadPlayableUnits[deadUnit.DeathTurn].Add(playableUnit2);
			}
			TileObjectSelectionManager.EnsureUnitSelection();
		}
		selectedSkill = null;
		PlayableUnitGhostView playableUnitGhostView = playableUnitGhostParent.GetComponentInChildren<PlayableUnitGhostView>();
		if (playableUnitGhostView == null)
		{
			playableUnitGhostView = UnityEngine.Object.Instantiate(playableUnitGhostViewPrefab, playableUnitGhostParent);
		}
		this.playableUnitGhostView.Add(playableUnitGhostView);
		playableUnitGhostView.Display(displayed: false);
		if (container == null)
		{
			RecruitmentController.InitMageGenerationProbability();
		}
		movePath = new MovePathController(movePathView).MovePath;
		TPSingleton<MovePathCounterHUD>.Instance.MovePath = movePath;
	}

	public ISerializedData Serialize()
	{
		SerializedPlayableUnits serializedPlayableUnits = new SerializedPlayableUnits
		{
			OwnedUnits = PlayableUnits.Select((PlayableUnit o) => o.Serialize() as SerializedPlayableUnit).ToList(),
			DeadUnits = new List<SerializedDeadUnit>(),
			Recruitment = (Recruitment.Serialize() as SerializedRecruitment)
		};
		foreach (KeyValuePair<int, List<PlayableUnit>> entry in DeadPlayableUnits)
		{
			serializedPlayableUnits.DeadUnits.AddRange(entry.Value.Select((PlayableUnit o) => new SerializedDeadUnit
			{
				Unit = (o.Serialize() as SerializedPlayableUnit),
				DeathTurn = entry.Key
			}).ToList());
		}
		return serializedPlayableUnits;
	}
}
