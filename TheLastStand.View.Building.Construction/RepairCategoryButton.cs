using System.Collections.Generic;
using TPLib;
using TheLastStand.Definition.Building;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Building.Construction;

[RequireComponent(typeof(Button))]
public class RepairCategoryButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IJoystickSelect
{
	private static class Constants
	{
		public const string IconPathFormat = "View/Sprites/UI/Construction/RepairCategory_{0}";
	}

	[SerializeField]
	private Button button;

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private CostPanel costPanel;

	public bool IsGold { get; private set; }

	public int Cost { get; private set; }

	public string Id { get; private set; }

	public TheLastStand.Model.Building.Construction.E_UnusableActionCause UnusableActionCause { get; private set; }

	public List<BuildingDefinition.E_BuildingCategory> BuildingCategories { get; private set; }

	public void Init(UnityAction onClick, string id, List<BuildingDefinition.E_BuildingCategory> categories)
	{
		Id = id;
		BuildingCategories = categories;
		for (int i = 0; i < BuildingCategories.Count; i++)
		{
			if (i == 0)
			{
				IsGold = BuildingCategories[i] == BuildingDefinition.E_BuildingCategory.Production;
			}
			else if (IsGold && BuildingCategories[i] != BuildingDefinition.E_BuildingCategory.Production)
			{
				TPSingleton<ConstructionManager>.Instance.LogError("RepairCategoryButton categories contains both gold and material costs.");
			}
		}
		button.onClick.AddListener(onClick);
		if (iconImage != null)
		{
			iconImage.sprite = ResourcePooler.LoadOnce<Sprite>($"View/Sprites/UI/Construction/RepairCategory_{Id}");
		}
		else
		{
			TPSingleton<ConstructionManager>.Instance.LogError("Missing iconImage reference to display RepairCategoryButton with Id " + Id);
		}
	}

	public void RefreshAvailability()
	{
		Cost = ConstructionManager.ComputeCategoriesRepairCost(BuildingCategories);
		bool flag = (IsGold ? (TPSingleton<ResourceManager>.Instance.Gold >= Cost) : (TPSingleton<ResourceManager>.Instance.Materials >= Cost));
		costPanel.Refresh(Cost, IsGold);
		if (Cost == 0)
		{
			UnusableActionCause = TheLastStand.Model.Building.Construction.E_UnusableActionCause.NoValidBuilding;
		}
		else if (!flag)
		{
			UnusableActionCause = TheLastStand.Model.Building.Construction.E_UnusableActionCause.NotEnoughResources;
		}
		else
		{
			UnusableActionCause = TheLastStand.Model.Building.Construction.E_UnusableActionCause.None;
		}
		button.interactable = Cost > 0 && flag;
	}

	public void RefreshDisplay()
	{
		base.gameObject.SetActive(TPSingleton<ConstructionView>.Instance.IsRepairCategoryButtonAvailable(BuildingCategories));
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
		TPSingleton<ConstructionView>.Instance.HoveredRepairCategoryButton = this;
		for (int num = TPSingleton<BuildingManager>.Instance.Buildings.Count - 1; num >= 0; num--)
		{
			TheLastStand.Model.Building.Building building = TPSingleton<BuildingManager>.Instance.Buildings[num];
			if (building.ConstructionModule.NeedRepair)
			{
				foreach (BuildingDefinition.E_BuildingCategory buildingCategory in BuildingCategories)
				{
					if (building.BlueprintModule.BlueprintModuleDefinition.Category.HasFlag(buildingCategory))
					{
						ConstructionView.DisplayTilesFeedback(building);
						break;
					}
				}
			}
		}
		BuildingManager.BuildingRepairTooltip.FollowElement.ChangeTarget(base.transform);
		BuildingManager.BuildingRepairTooltip.Display();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (TPSingleton<ConstructionView>.Instance.HoveredRepairCategoryButton == this)
		{
			ConstructionView.ClearRepairTilesFeedback();
			TPSingleton<ConstructionView>.Instance.HoveredRepairCategoryButton = null;
			BuildingManager.BuildingRepairTooltip.Display();
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
		EventSystem.current.SetSelectedGameObject(select ? button.gameObject : null);
	}

	private void OnDestroy()
	{
		button.onClick.RemoveAllListeners();
	}
}
