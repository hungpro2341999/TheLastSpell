using System;
using Sirenix.OdinInspector;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TheLastStand.Controller.Unit;
using TheLastStand.Framework.EventSystem;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Events;
using TheLastStand.View.Building.UI;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;

public class BuildingManagementPanel : SerializedMonoBehaviour
{
	public enum E_State
	{
		Closed,
		Opened,
		Hidden
	}

	private static class Constants
	{
		public const string BuildingIndestructible = "Building_Indestructible";
	}

	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private BuildingCapacitiesPanel buildingSkillsPanel;

	[SerializeField]
	private Image portraitBackground;

	[SerializeField]
	private Sprite portraitDefaultBackground;

	[SerializeField]
	private Sprite portraitBrazierBackground;

	[SerializeField]
	private Sprite portraitProductionBackground;

	[SerializeField]
	private TextMeshProUGUI buildingIdText;

	[SerializeField]
	private Image buildingPortraitImage;

	[SerializeField]
	private RectTransform buildingDescription;

	[SerializeField]
	private Transform buildingDescriptionTooltipAnchor;

	[SerializeField]
	private TextMeshProUGUI buildingDescriptionText;

	[SerializeField]
	private TPEmptyGraphic descriptionHitbox;

	[SerializeField]
	private Slider buildingHealthGaugeSlider;

	[SerializeField]
	private Image buildingHealthGaugeImage;

	[SerializeField]
	private TextMeshProUGUI buildingHealthGaugeText;

	[SerializeField]
	private Sprite healthGaugeClassic;

	[SerializeField]
	private Sprite healthGaugeInvulnerable;

	[SerializeField]
	private GameObject brazierHealth;

	[SerializeField]
	private Slider brazierHealthGaugeSlider;

	[SerializeField]
	private TextMeshProUGUI brazierHealthGaugeText;

	[SerializeField]
	private BuildingProductionManagementPanel buildingProductionManagementPanel;

	[SerializeField]
	private GameObject magicCircleProduction;

	[SerializeField]
	private Transform magicCircleUnitsBackground;

	[SerializeField]
	private Image buildingProductionGaugeRewardImage;

	[SerializeField]
	private BuildingManagementPanelGauge buildingProductionUnitsGaugeMagicCircle;

	[SerializeField]
	private Button shopButton;

	[SerializeField]
	private Button innButton;

	[SerializeField]
	private Image shopChains;

	[SerializeField]
	private Image innChains;

	private TheLastStand.Model.Building.Building building;

	public BuildingCapacitiesPanel BuildingCapacitiesPanel => buildingSkillsPanel;

	public E_State State { get; private set; }

	public void Close()
	{
		building = null;
		BuildingCapacitiesPanel.ChangeSelectedCapacityPanel(null);
		BuildingCapacitiesPanel.JoystickSkillBar.DeselectCurrentSkill();
		BuildingCapacitiesPanel.HideAllGroups();
		canvas.enabled = false;
		simpleFontLocalizedParent?.UnregisterChildren();
		State = E_State.Closed;
		SkillManager.SkillInfoPanel.Hide();
	}

	public void OnGameStateChange(Game.E_State state)
	{
		if (State == E_State.Hidden)
		{
			Unhide();
		}
	}

	public void OnInnButtonClick()
	{
		if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Recruitment && RecruitmentController.CanOpenRecruitmentPanel())
		{
			RecruitmentController.OpenRecruitmentPanel();
		}
	}

	public void OnShopButtonClick()
	{
		if (TPSingleton<BuildingManager>.Instance.Shop.ShopController.CanOpenShopPanel())
		{
			TPSingleton<BuildingManager>.Instance.Shop.ShopController.OpenShopPanel();
		}
	}

	public void Open()
	{
		canvas.enabled = UIManager.DebugToggleUI != false;
		simpleFontLocalizedParent?.RegisterChildren();
		State = E_State.Opened;
		Refresh();
	}

	public void Refresh()
	{
		if (!TileObjectSelectionManager.HasBuildingSelected)
		{
			Close();
			return;
		}
		TileMapView.ClearTiles(TileMapView.ReachableTilesTilemap);
		SkillManager.SkillInfoPanel.Hide();
		if (TileObjectSelectionManager.SelectedBuilding != building)
		{
			building = TileObjectSelectionManager.SelectedBuilding;
			buildingPortraitImage.sprite = building.BuildingView.GetPortraitSprite();
			shopButton.image.enabled = building.BuildingDefinition.Id == "Shop";
			if (building.BuildingDefinition.Id == "Shop")
			{
				bool flag = TPSingleton<BuildingManager>.Instance.Shop.ShopController.CanOpenShopPanel();
				shopButton.interactable = flag;
				shopButton.image.color = (flag ? Color.white : Color.gray);
				shopChains.enabled = !flag;
			}
			else
			{
				shopChains.enabled = false;
			}
			innButton.image.enabled = building.BuildingDefinition.Id == "Inn";
			if (building.BuildingDefinition.Id == "Inn")
			{
				bool flag2 = RecruitmentController.CanOpenRecruitmentPanel();
				innButton.interactable = flag2;
				innButton.image.color = (flag2 ? Color.white : Color.gray);
				innChains.enabled = !flag2;
			}
			else
			{
				innChains.enabled = false;
			}
		}
		if (building.IsTrap)
		{
			buildingPortraitImage.sprite = building.BuildingView.GetPortraitSprite();
		}
		if (!(building is MagicCircle magicCircle))
		{
			if (building.BlueprintModule.IsIndestructible)
			{
				buildingHealthGaugeImage.sprite = healthGaugeInvulnerable;
				buildingHealthGaugeSlider.value = 1f;
				buildingHealthGaugeText.text = AtlasIcons.InvulnerableIcon + " " + Localizer.Get("Building_Indestructible");
			}
			else
			{
				buildingHealthGaugeImage.sprite = healthGaugeClassic;
				buildingHealthGaugeSlider.value = ((building.DamageableModule.HealthTotal > 0f) ? (building.DamageableModule.Health / building.DamageableModule.HealthTotal) : 1f);
				buildingHealthGaugeText.text = ((building.DamageableModule.HealthTotal > 0f) ? $"{building.DamageableModule.Health}/{building.DamageableModule.HealthTotal}" : Localizer.Get("Building_Indestructible"));
			}
		}
		else
		{
			buildingHealthGaugeImage.sprite = healthGaugeClassic;
			buildingHealthGaugeSlider.value = ((magicCircle.CurrentHealthTotal > 0f) ? (building.DamageableModule.Health / magicCircle.CurrentHealthTotal) : 1f);
			buildingHealthGaugeText.text = ((magicCircle.CurrentHealthTotal > 0f) ? $"{building.DamageableModule.Health}/{magicCircle.CurrentHealthTotal}" : Localizer.Get("Building_Indestructible"));
		}
		if (building.BrazierModule != null && building.BrazierModule.BrazierPointsTotal > 0)
		{
			portraitBackground.sprite = portraitBrazierBackground;
			brazierHealth.SetActive(value: true);
			brazierHealthGaugeSlider.value = (float)building.BrazierModule.BrazierPoints / (float)building.BrazierModule.BrazierPointsTotal;
			brazierHealthGaugeText.text = $"{AtlasIcons.BrazierPointsIcon} {building.BrazierModule.BrazierPoints} / {building.BrazierModule.BrazierPointsTotal}";
		}
		else
		{
			portraitBackground.sprite = portraitDefaultBackground;
			brazierHealth.SetActive(value: false);
		}
		RefreshProduction();
		BuildingCapacitiesPanel.Refresh(building);
		RefreshLocalizedTexts();
	}

	public void RefreshBuildingPortrait()
	{
		if (TileObjectSelectionManager.HasBuildingSelected && building != null)
		{
			buildingPortraitImage.sprite = building.BuildingView.GetPortraitSprite();
		}
	}

	public void Unhide()
	{
		State = E_State.Opened;
		canvas.enabled = true;
		simpleFontLocalizedParent?.RegisterChildren();
	}

	private void Awake()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		if (canvas.enabled)
		{
			RefreshLocalizedTexts();
		}
	}

	private void OnWorkersChange(TheLastStand.Framework.EventSystem.Event e)
	{
		Refresh();
	}

	private void RefreshLocalizedTexts()
	{
		buildingIdText.text = Localizer.Get("BuildingName_" + building.BuildingDefinition.Id);
		buildingProductionManagementPanel.RefreshLocalizedTexts(building);
		if (Localizer.TryGet("BuildingDescription_" + building.BuildingDefinition.Id, out var value))
		{
			buildingDescription.gameObject.SetActive(value: true);
			descriptionHitbox.raycastTarget = true;
			buildingDescriptionText.text = value;
		}
		else
		{
			descriptionHitbox.raycastTarget = false;
			buildingDescription.gameObject.SetActive(value: false);
		}
	}

	private void RefreshProduction()
	{
		buildingProductionManagementPanel.Refresh(building);
		if (building.ProductionModule?.BuildingGaugeEffect == null)
		{
			DisableProduction();
			return;
		}
		if (building is MagicCircle)
		{
			RefreshProductionAsMagicCircle();
			return;
		}
		portraitBackground.sprite = portraitProductionBackground;
		magicCircleProduction.SetActive(value: false);
	}

	private void RefreshProductionAsMagicCircle()
	{
		magicCircleProduction.SetActive(value: true);
		buildingProductionGaugeRewardImage.sprite = building.ProductionModule.BuildingGaugeEffect.BuildingGaugeEffectView.GetProductionRewardIconSpriteBig();
		buildingProductionUnitsGaugeMagicCircle.SetUnitsCount(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.VictoryDaysCount);
		for (int i = 0; i < magicCircleUnitsBackground.childCount; i++)
		{
			magicCircleUnitsBackground.GetChild(i).gameObject.SetActive(i < TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.VictoryDaysCount);
		}
		buildingProductionUnitsGaugeMagicCircle.Clear();
		buildingProductionUnitsGaugeMagicCircle.SetUnits(building.ProductionModule.BuildingGaugeEffect.Units);
	}

	private void DisableProduction()
	{
		magicCircleProduction.SetActive(value: false);
	}

	private void Start()
	{
		Close();
		EventManager.AddListener(typeof(WorkersChangeEvent), OnWorkersChange);
	}
}
