using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.Apocalypse;
using TheLastStand.View.Apocalypse;
using TheLastStand.View.Tooltip;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.Apocalypse;

public class ApocalypseHeader : MonoBehaviour
{
	[SerializeField]
	protected ApocalypseGaugeDisplay apocalypseGaugeDisplay;

	[SerializeField]
	private ApocalypseLevelView apocalypseLevelView;

	[SerializeField]
	private Selectable apocalypseLevelViewSelectable;

	[SerializeField]
	private ApocalypseEffectsTooltip apocalypseEffectsTooltip;

	[SerializeField]
	private ApocalypseHeaderSealDisplay sealDisplay;

	[SerializeField]
	private TextMeshProUGUI cityName;

	[SerializeField]
	private GameObject damnedSoulsContainer;

	[SerializeField]
	private TextMeshProUGUI damnedSoulsModifier;

	[SerializeField]
	private bool reParentTooltipsToWorldMapTooltipsContainer = true;

	[SerializeField]
	private RectTransform tooltipsParent;

	private bool updateCityName;

	public ApocalypseEffectsTooltip ApocalypseEffectsTooltip => apocalypseEffectsTooltip;

	public ApocalypseGaugeDisplay ApocalypseGaugeDisplay => apocalypseGaugeDisplay;

	public Selectable ApocalypseLevelViewSelectable => apocalypseLevelViewSelectable;

	public void ContinueAnimations()
	{
		apocalypseLevelView.StopAnimation(stopAnimations: false);
		if (apocalypseGaugeDisplay != null)
		{
			apocalypseGaugeDisplay.ContinueAnimations();
		}
		if (sealDisplay != null)
		{
			sealDisplay.ContinueAnimations();
		}
	}

	public void OnApocalypseMoreInfoButtonClick()
	{
		TPSingleton<ApocalypseMoreInfoPanel>.Instance.Open();
	}

	public void PauseAnimations()
	{
		apocalypseLevelView.StopAnimation(stopAnimations: true);
		if (apocalypseGaugeDisplay != null)
		{
			apocalypseGaugeDisplay.PauseAnimations();
		}
		if (sealDisplay != null)
		{
			sealDisplay.PauseAnimations();
		}
	}

	public void RefreshApocalypseLevel(bool useTween)
	{
		int num;
		if (ApocalypseManager.CurrentApocalypseLevel <= 0)
		{
			TheLastStand.Model.Apocalypse.Apocalypse currentApocalypse = ApocalypseManager.CurrentApocalypse;
			num = ((currentApocalypse != null && currentApocalypse.CurrentLevel > 0) ? 1 : 0);
		}
		else
		{
			num = 1;
		}
		bool flag = (byte)num != 0;
		damnedSoulsContainer.SetActive(flag);
		if (flag)
		{
			damnedSoulsModifier.text = string.Format(Localizer.Get("WorldMap_ApocalypseDamnedSoulsModifier"), TPSingleton<ApocalypseManager>.Instance.DamnedSoulsPercentageModifier);
		}
		int num2 = ((ApocalypseManager.CurrentApocalypse != null) ? ApocalypseManager.CurrentApocalypse.CurrentLevel : 0);
		apocalypseLevelView.Init(num2, useTween);
		if (sealDisplay != null)
		{
			if (!useTween)
			{
				sealDisplay.Init(TPSingleton<WorldMapCityManager>.Instance.SelectedCity?.CityDefinition.Id, num2 > 0);
			}
			else if (num2 <= 0)
			{
				sealDisplay.Init(TPSingleton<WorldMapCityManager>.Instance.SelectedCity?.CityDefinition.Id, isAnimated: false);
			}
		}
		if (apocalypseGaugeDisplay != null)
		{
			apocalypseGaugeDisplay.SetSliderApocalypseValue(num2, useTween);
		}
	}

	public void RefreshCityName()
	{
		if (updateCityName)
		{
			cityName.text = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Name;
		}
	}

	public void RefreshRewardsFlames()
	{
		if (apocalypseGaugeDisplay != null)
		{
			apocalypseGaugeDisplay.RefreshRewardsFlames();
		}
	}

	private void Awake()
	{
		updateCityName = cityName != null;
		if (apocalypseLevelView != null && apocalypseLevelView.FlameKindleFeedback != null)
		{
			apocalypseLevelView.FlameKindleFeedback.OnFlameKindled += OnLevelViewFlameKindled;
		}
	}

	private void OnDestroy()
	{
		if (apocalypseLevelView != null && apocalypseLevelView.FlameKindleFeedback != null)
		{
			apocalypseLevelView.FlameKindleFeedback.OnFlameKindled -= OnLevelViewFlameKindled;
		}
	}

	private void OnLevelViewFlameKindled()
	{
		if (sealDisplay != null)
		{
			int num = ((ApocalypseManager.CurrentApocalypse != null) ? ApocalypseManager.CurrentApocalypse.CurrentLevel : 0);
			sealDisplay.Init(TPSingleton<WorldMapCityManager>.Instance.SelectedCity?.CityDefinition.Id, num > 0);
		}
	}

	private void Start()
	{
		if (!TPSingleton<GameManager>.Exist())
		{
			if (reParentTooltipsToWorldMapTooltipsContainer)
			{
				if (apocalypseEffectsTooltip != null)
				{
					apocalypseEffectsTooltip.transform.SetParent(TPSingleton<WorldMapUIManager>.Instance.TooltipsContainer);
				}
				apocalypseGaugeDisplay.ApocalypseLevelRewardTooltip.transform.SetParent(TPSingleton<WorldMapUIManager>.Instance.TooltipsContainer);
			}
		}
		else if (tooltipsParent != null)
		{
			if (apocalypseEffectsTooltip != null)
			{
				apocalypseEffectsTooltip.transform.SetParent(tooltipsParent);
			}
			apocalypseGaugeDisplay.ApocalypseLevelRewardTooltip.transform.SetParent(tooltipsParent);
		}
		apocalypseGaugeDisplay.InitApocalypseRewardsInSlider();
	}
}
