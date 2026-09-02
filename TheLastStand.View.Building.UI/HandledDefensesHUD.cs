using Sirenix.OdinInspector;
using TMPro;
using TPLib.Localization.Fonts;
using TheLastStand.Model.Building;
using TheLastStand.Model.Skill;
using TheLastStand.View.Camera;
using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.Building.UI;

public class HandledDefensesHUD : SerializedMonoBehaviour
{
	public static class Constants
	{
		public const string TrapUseTag = "<sprite name=\"TrapUse\">";

		public const string OverallUseTag = "<sprite name=\"UsePerNight\">";
	}

	[SerializeField]
	private Canvas chargesCanvas;

	[SerializeField]
	private TextMeshProUGUI chargesText;

	[SerializeField]
	private Canvas overallUsesCanvas;

	[SerializeField]
	private TextMeshProUGUI OverallUsesText;

	[SerializeField]
	private FollowElement followElement;

	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	private TheLastStand.Model.Building.Building building;

	public TheLastStand.Model.Building.Building Building
	{
		get
		{
			return building;
		}
		set
		{
			if (value != null)
			{
				building = value;
				base.name = building.UniqueIdentifier + " HandledDefenses HUD";
				if (followElement != null)
				{
					followElement.ChangeTarget(Building.BuildingView.HudFollowTarget);
					followElement.AutoMove();
				}
			}
		}
	}

	private void Awake()
	{
		if (chargesCanvas != null)
		{
			chargesCanvas.worldCamera = ACameraView.MainCam;
		}
		if (overallUsesCanvas != null)
		{
			overallUsesCanvas.worldCamera = ACameraView.MainCam;
		}
	}

	public void DisplayHandledDefensesUses(bool state)
	{
		string text = null;
		TextMeshProUGUI textToFill = null;
		Canvas canvasToActivate = null;
		if (building.IsTrap)
		{
			RetrieveDataForTrapCharges(state, out text, out textToFill, out canvasToActivate);
		}
		else if (building.IsHandledDefense)
		{
			RetrieveDataForOverallUses(state, out text, out textToFill, out canvasToActivate);
		}
		if (!string.IsNullOrEmpty(text) && textToFill != null && canvasToActivate != null)
		{
			canvasToActivate.gameObject.SetActive(value: true);
			textToFill.text = text;
			simpleFontLocalizedParent?.RefreshChildren();
		}
		else
		{
			chargesCanvas.gameObject.SetActive(value: false);
			overallUsesCanvas.gameObject.SetActive(value: false);
		}
	}

	public void RefreshPositionInstantly()
	{
		followElement.AutoMove();
	}

	private void RetrieveDataForOverallUses(bool state, out string text, out TextMeshProUGUI textToFill, out Canvas canvasToActivate)
	{
		int num = -1;
		int num2 = -1;
		foreach (TheLastStand.Model.Skill.Skill skill in Building.BattleModule.Skills)
		{
			if (skill != null && skill.OverallUses != 0)
			{
				num = skill.OverallUsesRemaining;
				num2 = skill.OverallUses;
				break;
			}
		}
		if (state && num != -1 && num2 != -1)
		{
			text = string.Format("{0}<style=Skill>{1}/{2}</style>", "<sprite name=\"UsePerNight\">", num, num2);
			textToFill = OverallUsesText;
			canvasToActivate = overallUsesCanvas;
			chargesCanvas.gameObject.SetActive(value: false);
		}
		else
		{
			text = string.Empty;
			textToFill = null;
			canvasToActivate = null;
		}
	}

	private void RetrieveDataForTrapCharges(bool state, out string text, out TextMeshProUGUI textToFill, out Canvas canvasToActivate)
	{
		int remainingTrapCharges = building.BattleModule.RemainingTrapCharges;
		int maximumTrapCharges = building.BattleModule.BattleModuleDefinition.MaximumTrapCharges;
		if (state && remainingTrapCharges != -1 && maximumTrapCharges > 0)
		{
			text = string.Format("{0}{1}/{2}", "<sprite name=\"TrapUse\">", remainingTrapCharges, maximumTrapCharges);
			textToFill = chargesText;
			canvasToActivate = chargesCanvas;
			overallUsesCanvas.gameObject.SetActive(value: false);
		}
		else
		{
			text = string.Empty;
			textToFill = null;
			canvasToActivate = null;
		}
	}
}
