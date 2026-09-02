using System;
using TMPro;
using TPLib.Localization;
using TheLastStand.Model.Building;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;

public class BuildingSkillTooltip : TooltipBase
{
	public static class Consts
	{
		public const string BuildingSkillTooltipNamePrefix = "BuildingSkillTooltipName_";

		public const string BuildingSkillTooltipDescriptionPrefix = "BuildingSkillTooltipDescription_";
	}

	[SerializeField]
	private TextMeshProUGUI buildingSkillIdText;

	[SerializeField]
	private TextMeshProUGUI buildingSkillCostText;

	[SerializeField]
	private TextMeshProUGUI buildingSkillDescriptionText;

	[SerializeField]
	private LayoutElement descriptionSpacingLayoutElement;

	private TheLastStand.Model.Building.Building building;

	private string buildingSkillId = string.Empty;

	public Transform FollowTarget
	{
		set
		{
			base.FollowElement.ChangeTarget(value);
		}
	}

	public void SetContent(string buildingSkillId, TheLastStand.Model.Building.Building building = null)
	{
		this.buildingSkillId = buildingSkillId;
		this.building = building;
		RefreshLocalizedTexts();
		if (this.building != null)
		{
			string text = (this.building.ConstructionModule.CostsMaterials ? "Materials" : "Gold");
			if (string.IsNullOrEmpty(this.building.BuildingView.GetBuildingSkillCostString(this.buildingSkillId)))
			{
				buildingSkillCostText.enabled = false;
				return;
			}
			buildingSkillCostText.enabled = true;
			buildingSkillCostText.text = "<style=\"" + text + "\"><style=\"Number\">" + this.building.BuildingView.GetBuildingSkillCostString(this.buildingSkillId) + "</style></style>";
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	protected override bool CanBeDisplayed()
	{
		return true;
	}

	protected override void RefreshContent()
	{
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		if (base.Displayed)
		{
			RefreshLocalizedTexts();
		}
	}

	private void RefreshLocalizedTexts()
	{
		if (buildingSkillId != string.Empty)
		{
			buildingSkillIdText.text = Localizer.Get("BuildingSkillTooltipName_" + buildingSkillId);
			if (Localizer.TryGet("BuildingSkillTooltipDescription_" + buildingSkillId, out var value) && !string.IsNullOrEmpty(value))
			{
				buildingSkillDescriptionText.text = value;
				descriptionSpacingLayoutElement.ignoreLayout = false;
			}
			else
			{
				buildingSkillDescriptionText.text = string.Empty;
				descriptionSpacingLayoutElement.ignoreLayout = true;
			}
		}
	}
}
