using System.Collections.Generic;
using Coffee.UIExtensions;
using DG.Tweening;
using TMPro;
using TPLib;
using TheLastStand.Database;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model.Trophy;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Trophy;

public class TrophyDisplay : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup achievementCanvasGroup;

	[SerializeField]
	private TextMeshProUGUI achievementTitleText;

	[SerializeField]
	private TextMeshProUGUI damnedSoulsValueText;

	[SerializeField]
	private GenericTooltipDisplayer genericTooltipDisplayer;

	[SerializeField]
	private Image jewelImage;

	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private List<Sprite> jewels = new List<Sprite>();

	[SerializeField]
	private UIParticle particlesBurstSquare;

	[SerializeField]
	private UIParticle particlesBurstRays;

	[SerializeField]
	private Selectable selectable;

	[SerializeField]
	private RectTransform rectTransform;

	private RectTransform parentRect;

	private RectTransform scrollViewPort;

	private bool hasFocus;

	public Selectable Selectable => selectable;

	public void Init(RectTransform parent, RectTransform viewPort)
	{
		parentRect = parent;
		scrollViewPort = viewPort;
		HUDJoystickNavigationManager.TooltipsToggled += OnTooltipsToggled;
	}

	public void Refresh(TheLastStand.Model.Trophy.Trophy trophy)
	{
		achievementTitleText.text = trophy.Name;
		damnedSoulsValueText.text = trophy.TrophyDefinition.DamnedSoulsEarned.ToString();
		if (genericTooltipDisplayer != null)
		{
			genericTooltipDisplayer.LocaKey = "TrophyDescription_" + trophy.TrophyDefinition.Id;
			genericTooltipDisplayer.LocalizationArguments = trophy.TrophyConditionController.TrophyConditionDefinition.DescriptionLocalizationParameters;
		}
		jewelImage.enabled = !trophy.TrophyDefinition.IgnoreGem;
		if (!trophy.TrophyDefinition.IgnoreGem)
		{
			int gemRarity = (int)TrophyDatabase.GetGemStageData(trophy.TrophyDefinition.DamnedSoulsEarned).GemRarity;
			if (jewels.Count > gemRarity)
			{
				jewelImage.sprite = jewels[gemRarity];
			}
		}
	}

	public void Refresh(string name, uint damnedSoulsEarned, string description, bool ignoreGem, string backgroundPath)
	{
		achievementTitleText.text = name;
		damnedSoulsValueText.text = damnedSoulsEarned.ToString();
		if (genericTooltipDisplayer != null)
		{
			genericTooltipDisplayer.LocaKey = description;
		}
		backgroundImage.sprite = ResourcePooler.LoadOnce<Sprite>(backgroundPath);
		jewelImage.enabled = !ignoreGem;
		if (!ignoreGem)
		{
			int gemRarity = (int)TrophyDatabase.GetGemStageData(damnedSoulsEarned).GemRarity;
			if (jewels.Count > gemRarity)
			{
				jewelImage.sprite = jewels[gemRarity];
			}
		}
	}

	public void Show()
	{
		particlesBurstSquare.Play();
		particlesBurstRays.Play();
		achievementCanvasGroup.alpha = 0f;
		achievementCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.OutCubic);
	}

	public void OnSelect()
	{
		GUIHelpers.AdjustHorizontalScrollViewToFocusedItem(rectTransform, scrollViewPort, parentRect, 2f);
		hasFocus = true;
		if (TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
		{
			genericTooltipDisplayer.DisplayTooltip();
		}
	}

	public void OnDeselect()
	{
		hasFocus = false;
		genericTooltipDisplayer.HideTooltip();
	}

	private void OnDestroy()
	{
		HUDJoystickNavigationManager.TooltipsToggled -= OnTooltipsToggled;
	}

	private void OnDisable()
	{
		hasFocus = false;
		genericTooltipDisplayer.HideTooltip();
	}

	private void OnTooltipsToggled(bool showTooltips)
	{
		if (hasFocus)
		{
			if (showTooltips)
			{
				genericTooltipDisplayer.DisplayTooltip();
			}
			else
			{
				genericTooltipDisplayer.HideTooltip();
			}
		}
	}
}
