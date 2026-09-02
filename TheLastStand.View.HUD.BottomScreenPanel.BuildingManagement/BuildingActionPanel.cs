using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TheLastStand.Controller;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Unit;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;

public class BuildingActionPanel : BuildingCapacityPanel
{
	[SerializeField]
	private TextMeshProUGUI workersCost;

	[SerializeField]
	private Canvas usesPerTurnCanvas;

	[SerializeField]
	private TextMeshProUGUI usesPerTurnText;

	[SerializeField]
	private Image actionIcon;

	[SerializeField]
	private SimpleFontLocalizedParent fontLocalizedParent;

	private Color workersCostColorInit = Color.white;

	public BuildingAction BuildingAction { get; set; }

	public static Sprite GetActionSprite(string buildingActionId)
	{
		return ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Buildings/Actions/" + buildingActionId);
	}

	public void Display(bool show)
	{
		base.BuildingCapacityRect.gameObject.SetActive(show);
		if (show)
		{
			fontLocalizedParent?.RegisterChildren();
		}
		else
		{
			fontLocalizedParent?.UnregisterChildren();
		}
	}

	public override void OnSkillPanelHovered(bool hover)
	{
		BuildingManager.OnBuildingActionHovered(BuildingAction, hover);
		if (hover)
		{
			if (BuildingAction.BuildingActionDefinition.ContainsRepelFogEffect)
			{
				TileMapView.DebugShowFogMinMax();
			}
			BuildingManager.BuildingActionTooltip.Init(BuildingAction);
			BuildingManager.BuildingActionTooltip.FollowElement.ChangeTarget(base.transform);
			foreach (PlayableUnit playableUnit in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
			{
				playableUnit.UnitView.UnitHUD.AttackEstimationDisplay.DisplayHoveredBuildingActionEffects(BuildingAction, playableUnit);
			}
		}
		else
		{
			if (BuildingAction.BuildingActionDefinition.ContainsRepelFogEffect)
			{
				TileMapView.FogMinMaxTilemap.ClearAllTiles();
			}
			BuildingManager.BuildingActionTooltip.Init();
			foreach (PlayableUnit playableUnit2 in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
			{
				playableUnit2.UnitView.UnitHUD.AttackEstimationDisplay.Hide();
			}
		}
		BuildingManager.BuildingActionTooltip.Display();
	}

	public void OnBuildingActionClick()
	{
		TPSingleton<BuildingManager>.Instance.Log("Building action " + BuildingAction.BuildingActionDefinition.Id + " was clicked.", CLogLevel.DETAILED);
		if (BuildingManager.SelectedBuildingAction != BuildingAction)
		{
			BuildingManager.SelectedBuildingAction = BuildingAction;
			buildingCapacitiesPanel.ChangeSelectedCapacityPanel(this);
			GameController.SetState(Game.E_State.BuildingPreparingAction);
		}
		else if (!InputManager.IsLastControllerJoystick)
		{
			BuildingManager.SelectedBuildingAction = null;
			buildingCapacitiesPanel.ChangeSelectedCapacityPanel(null);
			buildingCapacitiesPanel.JoystickSkillBar.DeselectCurrentSkill();
		}
	}

	public override void Refresh()
	{
		if (BuildingAction != null)
		{
			int modifiedWorkersCost = ResourceManager.GetModifiedWorkersCost(BuildingAction.BuildingActionDefinition);
			workersCost.color = ((modifiedWorkersCost <= TPSingleton<ResourceManager>.Instance.Workers) ? workersCostColorInit : Color.red);
			workersCost.text = $"{modifiedWorkersCost}";
			button.Interactable = BuildingAction.BuildingActionController.CanExecuteAction();
			actionIcon.sprite = GetActionSprite(BuildingAction.BuildingActionDefinition.Id);
			usesPerTurnCanvas.enabled = BuildingAction.BuildingActionDefinition.UsesPerTurnCount != -1;
			if (BuildingAction.BuildingActionDefinition.UsesPerTurnCount != -1)
			{
				usesPerTurnText.text = $"{BuildingAction.UsesPerTurnRemaining}/{BuildingAction.BuildingActionDefinition.UsesPerTurnCount}";
			}
		}
	}

	private void Awake()
	{
		workersCostColorInit = workersCost.color;
	}
}
