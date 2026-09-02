using System;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Controller;
using TheLastStand.Controller.Meta;
using TheLastStand.Controller.Unit;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.HUD.UnitManagement;
using TheLastStand.View.ToDoList;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel;

public class BottomLeftPanel : MonoBehaviour
{
	[Serializable]
	public struct BottomLeftButton
	{
		public BetterButton Button;

		public Image Image;

		public GameObject Chains;

		public List<Selectable> NavigationUp;

		public List<Selectable> NavigationDown;
	}

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private GameObject joystickHUDNavigationIcon;

	[SerializeField]
	private BottomLeftButton inventoryButton;

	[SerializeField]
	private BottomLeftButton characterSheetButton;

	[SerializeField]
	private BottomLeftButton shopButton;

	[SerializeField]
	private BottomLeftButton constructionButton;

	[SerializeField]
	private BottomLeftButton metaShopsButton;

	[SerializeField]
	private BottomLeftButton innButton;

	[SerializeField]
	private Button settingsButton;

	[SerializeField]
	[Range(0f, 1f)]
	private float nonInteractableButtonAlpha = 0.33f;

	[SerializeField]
	private BuildingDefinition.E_ConstructionCategory constructionCategoryOnOpen = BuildingDefinition.E_ConstructionCategory.Production;

	[SerializeField]
	private CancelMovementPanel cancelMovementPanel;

	[SerializeField]
	private Image noSelectionBackgroundImage;

	[SerializeField]
	private Image selectionBackgroundImage;

	[SerializeField]
	private Image constructionBackgroundImage;

	private List<Selectable> characterSheetNavigationLeft;

	private List<Selectable> characterSheetNavigationDown;

	private List<Selectable> inventoryNavigationRight;

	private List<Selectable> inventoryNavigationDown;

	private List<Selectable> metaShopsNavigationRight;

	private List<Selectable> metaShopsNavigationUp;

	private List<Selectable> metaShopsNavigationDown;

	private List<Selectable> innNavigationLeft;

	private List<Selectable> innNavigationUp;

	private List<Selectable> innNavigationDown;

	private List<Selectable> shopNavigationRight;

	private List<Selectable> shopNavigationDown;

	private List<Selectable> shopNavigationUp;

	private List<Selectable> constructionNavigationLeft;

	private List<Selectable> constructionNavigationDown;

	private List<Selectable> constructionNavigationUp;

	private List<Selectable> settingsNavigationUp;

	public CancelMovementPanel CancelMovementPanel => cancelMovementPanel;

	public Canvas Canvas => canvas;

	public void OnGameStateChange(Game.E_State state)
	{
		if (joystickHUDNavigationIcon != null)
		{
			switch (state)
			{
			case Game.E_State.Management:
			case Game.E_State.UnitPreparingSkill:
			case Game.E_State.BuildingPreparingSkill:
			case Game.E_State.Construction:
			case Game.E_State.BuildingUpgrade:
			case Game.E_State.Wait:
				joystickHUDNavigationIcon.SetActive(value: true);
				break;
			default:
				joystickHUDNavigationIcon.SetActive(value: false);
				break;
			}
		}
	}

	public void OnCancelMovementButtonClick()
	{
		PlayableUnitManager.UndoLastCommand();
	}

	public void OnSettingsButtonClick()
	{
		if (SettingsManager.CanOpenSettings())
		{
			ApplicationManager.Application.ApplicationController.SetState("Settings");
		}
	}

	private void OnSelectionChange()
	{
		RefreshConstructionBackground();
		selectionBackgroundImage.enabled = TileObjectSelectionManager.HasAnythingSelected;
		noSelectionBackgroundImage.enabled = !selectionBackgroundImage.enabled && !constructionBackgroundImage.enabled;
	}

	public void Refresh()
	{
		if (UIManager.DebugToggleUI == false || (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.NightTurn != Game.E_NightTurn.PlayableUnits))
		{
			canvas.enabled = false;
			return;
		}
		switch (TPSingleton<GameManager>.Instance.Game.State)
		{
		case Game.E_State.Management:
		case Game.E_State.CharacterSheet:
		case Game.E_State.UnitPreparingSkill:
		case Game.E_State.UnitExecutingSkill:
		case Game.E_State.BuildingPreparingAction:
		case Game.E_State.BuildingExecutingAction:
		case Game.E_State.BuildingPreparingSkill:
		case Game.E_State.BuildingExecutingSkill:
		case Game.E_State.Construction:
		case Game.E_State.Recruitment:
		case Game.E_State.Shopping:
		case Game.E_State.ProductionReport:
		case Game.E_State.Settings:
		case Game.E_State.ApocalypseMoreInfo:
			canvas.enabled = true;
			cancelMovementPanel.Refresh();
			RefreshButtons();
			RefreshJoystickNavigation();
			OnSelectionChange();
			TPSingleton<PlayableUnitManagementView>.Instance.PlayableSkillBar.Refresh(fullRefresh: true);
			break;
		case Game.E_State.Wait:
			canvas.enabled = true;
			if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night)
			{
				cancelMovementPanel.Refresh();
				RefreshButtons();
				RefreshJoystickNavigation();
			}
			OnSelectionChange();
			break;
		default:
			canvas.enabled = false;
			break;
		}
	}

	public void RefreshConstructionBackground()
	{
		bool flag = TPSingleton<ConstructionManager>.Instance.Construction.State == Construction.E_State.PlaceBuilding;
		if (flag)
		{
			noSelectionBackgroundImage.enabled = true;
		}
		constructionBackgroundImage.enabled = TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Construction && !flag;
	}

	private void OnCharacterSheetButtonClick()
	{
		if (CharacterSheetManager.CanOpenCharacterSheetPanel())
		{
			TileObjectSelectionManager.EnsureUnitSelection();
			CharacterSheetManager.OpenCharacterSheetPanel();
			TPSingleton<CharacterSheetPanel>.Instance.OpenUnitDetails();
		}
	}

	private void OnConstructionButtonClick()
	{
		ConstructionManager.OpenConstructionMode(constructionCategoryOnOpen);
	}

	private void OnInventoryButtonClick()
	{
		TileObjectSelectionManager.EnsureUnitSelection();
		CharacterSheetManager.OpenCharacterSheetPanel();
		TPSingleton<CharacterSheetPanel>.Instance.OpenInventory();
	}

	private void Awake()
	{
		inventoryButton.Button.onClick.AddListener(OnInventoryButtonClick);
		characterSheetButton.Button.onClick.AddListener(OnCharacterSheetButtonClick);
		shopButton.Button.onClick.AddListener(ShopManager.OpenShop);
		constructionButton.Button.onClick.AddListener(OnConstructionButtonClick);
		metaShopsButton.Button.onClick.AddListener(TPSingleton<MetaShopsManager>.Instance.OpenShops);
		innButton.Button.onClick.AddListener(RecruitmentController.OpenRecruitmentPanel);
		InitJoystickNavigation();
	}

	private void OnDestroy()
	{
		inventoryButton.Button.onClick.RemoveListener(OnInventoryButtonClick);
		characterSheetButton.Button.onClick.RemoveListener(OnCharacterSheetButtonClick);
		shopButton.Button.onClick.RemoveListener(ShopManager.OpenShop);
		constructionButton.Button.onClick.RemoveListener(OnConstructionButtonClick);
		if (TPSingleton<MetaShopsManager>.Exist())
		{
			metaShopsButton.Button.onClick.RemoveListener(TPSingleton<MetaShopsManager>.Instance.OpenShops);
		}
		innButton.Button.onClick.RemoveListener(RecruitmentController.OpenRecruitmentPanel);
	}

	private void RefreshButtons()
	{
		RefreshInventoryButton();
		RefreshCharacterSheetButton();
		RefreshMetaShopsButton();
		RefreshInnButton();
		RefreshShopButton();
		RefreshConstructionButton();
	}

	private void RefreshButton(BottomLeftButton button, bool interactable)
	{
		button.Button.interactable = interactable;
		button.Image.color = new Color(1f, 1f, 1f, interactable ? 1f : nonInteractableButtonAlpha);
		button.Chains.SetActive(!interactable);
	}

	private void RefreshButtonNavigationLeft(Selectable button, IEnumerable<Selectable> targets)
	{
		button.SetSelectOnLeft(targets.FirstOrDefault((Selectable t) => t.gameObject.activeInHierarchy && t.IsInteractable()));
	}

	private void RefreshButtonNavigationRight(Selectable button, IEnumerable<Selectable> targets)
	{
		button.SetSelectOnRight(targets.FirstOrDefault((Selectable t) => t.gameObject.activeInHierarchy && t.IsInteractable()));
	}

	private void RefreshButtonNavigationUp(Selectable button, IEnumerable<Selectable> targets)
	{
		button.SetSelectOnUp(targets.FirstOrDefault((Selectable t) => t.gameObject.activeInHierarchy && t.IsInteractable()));
	}

	private void RefreshButtonNavigationDown(Selectable button, IEnumerable<Selectable> targets)
	{
		button.SetSelectOnDown(targets.FirstOrDefault((Selectable t) => t.gameObject.activeInHierarchy && t.IsInteractable()));
	}

	private void RefreshCharacterSheetButton()
	{
		RefreshButton(characterSheetButton, CharacterSheetManager.CanOpenCharacterSheetPanel());
	}

	private void RefreshInventoryButton()
	{
		RefreshButton(inventoryButton, TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.CanOpenInventory());
	}

	private void RefreshMetaShopsButton()
	{
		if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.IsTutorialMap && !MetaShopsManager.OraculumForceAccess)
		{
			metaShopsButton.Button.gameObject.SetActive(value: false);
			metaShopsButton.Chains.SetActive(value: false);
		}
		else
		{
			metaShopsButton.Button.gameObject.SetActive(value: true);
			metaShopsButton.Chains.SetActive(value: true);
			RefreshButton(metaShopsButton, MetaShopsManager.CanOpenShops() && TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management);
		}
	}

	private void RefreshInnButton()
	{
		List<UnlockBuildingMetaEffectDefinition> list = null;
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockBuildingMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
		{
			list = effects.ToList();
		}
		if (MetaUpgradesManager.IsThisBuildingUnlockedByDefault("Inn") || (list != null && list.Any((UnlockBuildingMetaEffectDefinition x) => x.BuildingId == "Inn")))
		{
			innButton.Button.gameObject.SetActive(value: true);
			RefreshButton(innButton, RecruitmentController.CanOpenRecruitmentPanel());
		}
		else
		{
			innButton.Button.gameObject.SetActive(value: false);
			innButton.Chains.SetActive(value: false);
		}
	}

	private void RefreshShopButton()
	{
		RefreshButton(shopButton, TPSingleton<BuildingManager>.Instance.Shop.ShopController.CanOpenShopPanel());
	}

	private void RefreshConstructionButton()
	{
		RefreshButton(constructionButton, TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Construction && GameController.CanOpenConstructionMode(BuildingDefinition.E_ConstructionCategory.Defensive));
	}

	private void RefreshJoystickNavigation()
	{
		Selectable lastActiveSelectable = TPSingleton<ToDoListView>.Instance.GetLastActiveSelectable();
		Selectable selectOnUp = (InputManager.JoystickConfig.HUDNavigation.SelectLastButtonFromBottomPanel ? lastActiveSelectable : TPSingleton<ToDoListView>.Instance.GetFirstActiveSelectable());
		if (lastActiveSelectable != null)
		{
			lastActiveSelectable.SetSelectOnDown(inventoryButton.Button);
		}
		RefreshButtonNavigationLeft(characterSheetButton.Button, characterSheetNavigationLeft);
		RefreshButtonNavigationDown(characterSheetButton.Button, characterSheetNavigationDown);
		characterSheetButton.Button.SetSelectOnUp(selectOnUp);
		RefreshButtonNavigationRight(inventoryButton.Button, inventoryNavigationRight);
		RefreshButtonNavigationDown(inventoryButton.Button, inventoryNavigationDown);
		inventoryButton.Button.SetSelectOnUp(selectOnUp);
		RefreshButtonNavigationRight(metaShopsButton.Button, metaShopsNavigationRight);
		RefreshButtonNavigationUp(metaShopsButton.Button, metaShopsNavigationUp);
		RefreshButtonNavigationDown(metaShopsButton.Button, metaShopsNavigationDown);
		RefreshButtonNavigationLeft(innButton.Button, innNavigationLeft);
		RefreshButtonNavigationUp(innButton.Button, innNavigationUp);
		RefreshButtonNavigationDown(innButton.Button, innNavigationDown);
		RefreshButtonNavigationRight(shopButton.Button, shopNavigationRight);
		RefreshButtonNavigationDown(shopButton.Button, shopNavigationDown);
		RefreshButtonNavigationUp(shopButton.Button, shopNavigationUp);
		RefreshButtonNavigationUp(constructionButton.Button, constructionNavigationUp);
		RefreshButtonNavigationLeft(constructionButton.Button, constructionNavigationLeft);
		RefreshButtonNavigationDown(constructionButton.Button, constructionNavigationDown);
		RefreshButtonNavigationUp(settingsButton, settingsNavigationUp);
	}

	private void InitJoystickNavigation()
	{
		inventoryButton.Button.SetMode(Navigation.Mode.Explicit);
		characterSheetButton.Button.SetMode(Navigation.Mode.Explicit);
		shopButton.Button.SetMode(Navigation.Mode.Explicit);
		constructionButton.Button.SetMode(Navigation.Mode.Explicit);
		metaShopsButton.Button.SetMode(Navigation.Mode.Explicit);
		innButton.Button.SetMode(Navigation.Mode.Explicit);
		settingsButton.SetMode(Navigation.Mode.Explicit);
		characterSheetNavigationLeft = new List<Selectable> { inventoryButton.Button };
		characterSheetNavigationDown = new List<Selectable> { constructionButton.Button, innButton.Button, metaShopsButton.Button, settingsButton };
		inventoryNavigationRight = new List<Selectable> { characterSheetButton.Button };
		inventoryNavigationDown = new List<Selectable> { shopButton.Button, metaShopsButton.Button, innButton.Button, settingsButton };
		metaShopsNavigationUp = new List<Selectable> { shopButton.Button, inventoryButton.Button };
		metaShopsNavigationRight = new List<Selectable> { innButton.Button };
		metaShopsNavigationDown = new List<Selectable> { settingsButton };
		innNavigationLeft = new List<Selectable> { metaShopsButton.Button };
		innNavigationUp = new List<Selectable> { constructionButton.Button, characterSheetButton.Button };
		innNavigationDown = new List<Selectable> { settingsButton };
		shopNavigationRight = new List<Selectable> { constructionButton.Button };
		shopNavigationDown = new List<Selectable> { metaShopsButton.Button, innButton.Button, settingsButton };
		shopNavigationUp = new List<Selectable> { inventoryButton.Button };
		constructionNavigationUp = new List<Selectable> { characterSheetButton.Button };
		constructionNavigationLeft = new List<Selectable> { shopButton.Button };
		constructionNavigationDown = new List<Selectable> { innButton.Button, metaShopsButton.Button, settingsButton };
		settingsNavigationUp = new List<Selectable> { metaShopsButton.Button, innButton.Button, shopButton.Button, constructionButton.Button, inventoryButton.Button, characterSheetButton.Button };
	}

	private void OnEnable()
	{
		TileObjectSelectionManager.OnUnitSelectionChange += OnSelectionChange;
	}

	private void OnDisable()
	{
		TileObjectSelectionManager.OnUnitSelectionChange -= OnSelectionChange;
	}
}
