using TMPro;
using TPLib;
using TheLastStand.Definition.Building;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Building.Construction;

public class BuildingButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IJoystickSelect
{
	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private Image portrait;

	[SerializeField]
	private Button button;

	[SerializeField]
	private CostPanel costPanel;

	private BuildingDefinition buildingDefinition;

	private bool initialized;

	private bool isDisplayed = true;

	private UnityAction onClick;

	private Color originalMaterialsTextColor;

	private Color originalGoldTextColor;

	private Color originalNameTextColor;

	public BuildingDefinition BuildingDefinition
	{
		get
		{
			return buildingDefinition;
		}
		set
		{
			if (onClick != null)
			{
				GetComponent<Button>().onClick.RemoveListener(onClick);
			}
			buildingDefinition = value;
			RefreshText();
			RefreshCost();
			portrait.sprite = BuildingView.GetPortraitSprite(buildingDefinition.Id);
			onClick = delegate
			{
				ConstructionView.OnBuildingButtonClick(buildingDefinition);
				TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.ButtonClickAudioClip);
				OnPointerExit(null);
			};
			GetComponent<Button>().onClick.AddListener(onClick);
		}
	}

	public void Display(bool show)
	{
		if (isDisplayed != show)
		{
			isDisplayed = show;
			base.gameObject.SetActive(show);
		}
	}

	public void OnDisplayTooltip(bool display)
	{
		if (!InputManager.JoystickConfig.HUDNavigation.AlwaysShowTooltipOnConstruction)
		{
			if (display)
			{
				OnPointerEnter(null);
			}
			else
			{
				OnPointerExit(null);
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.ButtonHoverAudioClip);
		if (TPSingleton<ConstructionManager>.Instance.Construction.State != TheLastStand.Model.Building.Construction.E_State.PlaceBuilding)
		{
			TPSingleton<ConstructionView>.Instance.HoveredBuildingButton = this;
			BuildingManager.BuildingConstructionTooltip.Init(buildingDefinition);
			BuildingManager.BuildingConstructionTooltip.FollowElement.ChangeTarget(base.transform);
			BuildingManager.BuildingConstructionTooltip.Display();
			RefreshText();
			nameText.enabled = true;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (TPSingleton<ConstructionView>.Instance.HoveredBuildingButton == this)
		{
			TPSingleton<ConstructionView>.Instance.HoveredBuildingButton = null;
			BuildingManager.BuildingConstructionTooltip.Init(null);
			BuildingManager.BuildingConstructionTooltip.Hide();
			nameText.enabled = false;
		}
	}

	public void OnSkillHover(bool select)
	{
		if (TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips || InputManager.JoystickConfig.HUDNavigation.AlwaysShowTooltipOnConstruction)
		{
			if (select)
			{
				OnPointerEnter(null);
			}
			else
			{
				OnPointerExit(null);
			}
		}
		TPSingleton<ConstructionView>.Instance.AdjustScrollbar(GetComponent<RectTransform>());
		EventSystem.current.SetSelectedGameObject(select ? button.gameObject : null);
	}

	public void Refresh()
	{
		Init();
		ResourceManager instance = TPSingleton<ResourceManager>.Instance;
		int num = BuildingManager.ComputeBuildingCost(buildingDefinition.ConstructionModuleDefinition);
		bool num2 = ((buildingDefinition.ConstructionModuleDefinition.NativeGoldCost > 0) ? (instance.Gold >= num) : (instance.Materials >= num));
		bool flag = ConstructionManager.IsUnderBuildLimit(buildingDefinition.ConstructionModuleDefinition);
		bool flag2 = num2 && flag;
		RefreshCost();
		portrait.color = (flag2 ? Color.white : GameView.NegativeColor);
		nameText.color = (flag2 ? originalNameTextColor : GameView.NegativeColor);
		if (button != null)
		{
			button.enabled = flag2;
		}
	}

	private void Awake()
	{
		Init();
	}

	private void OnDisable()
	{
		nameText.enabled = false;
	}

	private void Init()
	{
		if (!initialized)
		{
			originalNameTextColor = nameText.color;
			nameText.enabled = false;
			initialized = true;
		}
	}

	private void RefreshText()
	{
		nameText.text = buildingDefinition.Name.ToUpper();
	}

	private void RefreshCost()
	{
		int cost = BuildingManager.ComputeBuildingCost(buildingDefinition.ConstructionModuleDefinition);
		bool isGold = buildingDefinition.ConstructionModuleDefinition.NativeGoldCost > 0;
		costPanel.Refresh(cost, isGold);
	}
}
