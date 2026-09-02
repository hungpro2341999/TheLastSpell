using TPLib;
using TheLastStand.Controller;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Unit;
using TheLastStand.View.Camera;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.Skill.UI;
using TheLastStand.View.Unit.Stat;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.Unit;

public class UnitPortraitView : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public static class Constants
	{
		public const string PortraitPath = "View/Sprites/UI/Units/Portaits/Playable Unit/Foreground/";

		public const string PortraitBackgroundPath = "View/Sprites/UI/Units/Portaits/Playable Unit/Background/";
	}

	[SerializeField]
	private UnitStatDisplay unitHealthStatDefinition;

	[SerializeField]
	private UnitStatDisplay unitMovePointStatDefinition;

	[SerializeField]
	private UnitStatDisplay unitActionPointStatDefinition;

	[SerializeField]
	protected BetterToggle unitportraitToggle;

	[SerializeField]
	protected Image unitPortraitImage;

	[SerializeField]
	protected Image unitPortraitBoxHovered;

	[SerializeField]
	protected Image unitPortraitBGImage;

	[SerializeField]
	protected SkillTargetingMark skillTargetingMark;

	[SerializeField]
	private Material material;

	private static readonly int SwapTex = Shader.PropertyToID("_SwapTex");

	public PlayableUnit PlayableUnit;

	private bool isInited;

	private Texture2D currentTexture;

	public BetterToggle UnitPortraitToggle => unitportraitToggle;

	public SkillTargetingMark SkillTargetingMark => skillTargetingMark;

	public virtual void DisplayUnitPortraitBoxHovered(bool value)
	{
		if (unitPortraitBoxHovered != null)
		{
			unitPortraitBoxHovered.enabled = value;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		DisplayUnitPortraitBoxHovered(value: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		DisplayUnitPortraitBoxHovered(value: false);
	}

	public void OnUnitPortraitClick()
	{
		if (TileObjectSelectionManager.IsProcessingASelection || (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.NightTurn != Game.E_NightTurn.PlayableUnits))
		{
			return;
		}
		if ((TPSingleton<GameManager>.Instance.Game.State == Game.E_State.UnitPreparingSkill || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.UnitExecutingSkill) && PlayableUnitManager.SelectedSkill != null)
		{
			ValidTargets validTargets = PlayableUnitManager.SelectedSkill.SkillDefinition.ValidTargets;
			if (validTargets != null && validTargets.PlayableUnits)
			{
				return;
			}
		}
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.UnitExecutingSkill && TileObjectSelectionManager.HasUnitSelected && TileObjectSelectionManager.SelectedUnit is PlayableUnit unit)
		{
			GameView.TopScreenPanel.UnitPortraitsPanel.ToggleSelectedUnit(unit);
			return;
		}
		BetterToggle betterToggle = unitportraitToggle;
		if (((object)betterToggle == null || betterToggle.isOn) && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.BuildingPreparingAction && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.BuildingExecutingAction)
		{
			if (TileObjectSelectionManager.SelectedUnit != PlayableUnit)
			{
				if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Day)
				{
					PathfindingManager.Pathfinding.PathfindingController.ClearReachableTiles();
				}
				if (PlayableUnitManager.SelectedSkill != null)
				{
					PlayableUnitManager.SelectedSkill = null;
				}
				TileObjectSelectionManager.SetSelectedPlayableUnit(PlayableUnit, CameraView.CameraUIMasksHandler.IsPointOffscreenOrHiddenByUI(PlayableUnit.UnitView.transform.position));
				switch (TPSingleton<GameManager>.Instance.Game.State)
				{
				case Game.E_State.CharacterSheet:
					TPSingleton<CharacterSheetPanel>.Instance.Refresh();
					break;
				case Game.E_State.Construction:
					GameController.SetState(Game.E_State.Management);
					break;
				}
				if (TPSingleton<PlayableUnitManager>.Instance.HasToRecomputeReachableTiles)
				{
					PlayableUnit.PlayableUnitController.ComputeReachableTiles();
				}
			}
			else
			{
				ACameraView.MoveTo(PlayableUnit.UnitView.transform);
			}
		}
		else
		{
			BetterToggle betterToggle2 = unitportraitToggle;
			if (((object)betterToggle2 == null || !betterToggle2.group.AnyTogglesOn()) && TileObjectSelectionManager.HasUnitSelected && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.BuildingPreparingAction && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.BuildingExecutingAction)
			{
				TileObjectSelectionManager.DeselectUnit();
			}
		}
	}

	public void OnUnitPortraitHoverEnter()
	{
		GameView.TopScreenPanel.UnitPortraitsPanel.SetPortraitIsHovered(this);
	}

	public void OnUnitPortraitHoverExit()
	{
		GameView.TopScreenPanel.UnitPortraitsPanel.SetPortraitIsHovered();
	}

	public void RefreshPortrait()
	{
		unitPortraitImage.enabled = PlayableUnit != null;
		unitPortraitBGImage.enabled = PlayableUnit != null;
		if (PlayableUnit != null)
		{
			unitPortraitBGImage.sprite = PlayableUnit.PortraitBackgroundSprite;
			unitPortraitBGImage.color = PlayableUnit.PortraitColor._Color;
			unitPortraitImage.sprite = PlayableUnit.PortraitSprite;
			Texture2D src = PlayableUnit.PlayableUnitView.ColorSwapPortraitMaterial.GetTexture(SwapTex) as Texture2D;
			if (currentTexture == null)
			{
				currentTexture = new Texture2D(100, 1, TextureFormat.RGBA32, mipChain: false, linear: false)
				{
					filterMode = FilterMode.Point,
					wrapMode = TextureWrapMode.Clamp
				};
			}
			Graphics.CopyTexture(src, currentTexture);
			currentTexture.Apply();
			material.SetTexture(SwapTex, currentTexture);
		}
	}

	public virtual void RefreshStats()
	{
		if (!isInited && PlayableUnit != null)
		{
			Init();
		}
		if (unitHealthStatDefinition != null)
		{
			unitHealthStatDefinition.Refresh();
		}
		if (unitActionPointStatDefinition != null)
		{
			unitActionPointStatDefinition.Refresh();
		}
		if (unitMovePointStatDefinition != null)
		{
			unitMovePointStatDefinition.Refresh();
		}
	}

	protected virtual void Awake()
	{
		material = new Material(material);
		unitPortraitImage.material = material;
		if (currentTexture == null)
		{
			currentTexture = new Texture2D(100, 1, TextureFormat.RGBA32, mipChain: false, linear: false)
			{
				filterMode = FilterMode.Point,
				wrapMode = TextureWrapMode.Clamp
			};
		}
		if (!isInited && PlayableUnit != null)
		{
			Init();
		}
	}

	private void Init()
	{
		if (unitHealthStatDefinition != null)
		{
			unitHealthStatDefinition.TargetUnit = PlayableUnit;
			unitHealthStatDefinition.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Health];
			unitHealthStatDefinition.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.HealthTotal];
		}
		if (unitActionPointStatDefinition != null)
		{
			unitActionPointStatDefinition.TargetUnit = PlayableUnit;
			unitActionPointStatDefinition.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ActionPoints];
			unitActionPointStatDefinition.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ActionPointsTotal];
		}
		if (unitMovePointStatDefinition != null)
		{
			unitMovePointStatDefinition.TargetUnit = PlayableUnit;
			unitMovePointStatDefinition.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.MovePoints];
			unitMovePointStatDefinition.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.MovePointsTotal];
		}
		if (unitPortraitBoxHovered != null)
		{
			unitPortraitBoxHovered.enabled = false;
		}
		isInited = true;
	}
}
