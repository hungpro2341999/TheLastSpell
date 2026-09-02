using System.Collections.Generic;
using System.Text;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Database;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.Manager;
using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.Tooltip;

public class ApocalypseEffectsTooltip : TooltipBase
{
	public static class Constants
	{
		public const string Title = "WorldMap_ApocalypseEffects";

		public const string TitleBaseGameDifficulty = "WorldMap_ApocalypseDifficulty_Normal";

		public const string MaxModifiersDisplayedReached = "ApocalypseEffectsTooltip_MaxModifiersDisplayedReached";

		public const string DamnedSoulsModifier = "WorldMap_ApocalypseDamnedSoulsModifier";

		public const int ApocalypsePreviewMaximumModifiersDisplayed = 10;

		public const int DefaultMaximumModifiersDisplayed = 14;

		public const string BaseGameDifficulty = "WorldMap_ApocalypseDescription_00";

		public const string DotLightOn = "Glyphs_GlyphPoint_On";

		public const string DotLightOff = "Glyphs_GlyphPoint_Off";
	}

	[SerializeField]
	private TextMeshProUGUI effectsDescription;

	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private TextMeshProUGUI damnedSoulsModifier;

	[SerializeField]
	private GameObject moreInfoContainer;

	[SerializeField]
	private bool canDisplayMoreInfo = true;

	private uint? damnedSoulsPercentageModifier;

	private List<ApocalypseModifierStepDefinition> apocalypseModifierStepDefinitions;

	private int maximumModifiersDisplayed = 14;

	private bool? isApocalypseUnlocked;

	public bool MustDisplayNormalDifficulty
	{
		get
		{
			if (apocalypseModifierStepDefinitions != null)
			{
				return apocalypseModifierStepDefinitions.Count == 0;
			}
			return true;
		}
	}

	public static string GetModifierStepDotsStr(ApocalypseModifierDefinition modifierDefinition, int stepIndex)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < modifierDefinition.StepDefinitions.Count; i++)
		{
			if (stepIndex >= i)
			{
				stringBuilder.Append("<sprite name=Glyphs_GlyphPoint_On>");
			}
			else
			{
				stringBuilder.Append("<sprite name=Glyphs_GlyphPoint_Off>");
			}
		}
		return stringBuilder.ToString();
	}

	public void SetDamnedSoulsPercentageModifier(uint? value)
	{
		damnedSoulsPercentageModifier = value;
	}

	public void SetApocalypseModifierStepDefinitions(List<ApocalypseModifierStepDefinition> stepDefinitions)
	{
		apocalypseModifierStepDefinitions = stepDefinitions;
	}

	public void SetIsApocalypseSystemUnlocked(bool? isUnlocked)
	{
		isApocalypseUnlocked = isUnlocked;
	}

	public void SetMaximumModifiersDisplayed(int newMaximum = -1)
	{
		if (newMaximum == -1)
		{
			maximumModifiersDisplayed = 14;
		}
		else
		{
			maximumModifiersDisplayed = newMaximum;
		}
	}

	protected override bool CanBeDisplayed()
	{
		return isApocalypseUnlocked ?? ApocalypseManager.IsApocalypseUnlocked;
	}

	protected override void RefreshContent()
	{
		title.text = Localizer.Get(MustDisplayNormalDifficulty ? "WorldMap_ApocalypseDifficulty_Normal" : "WorldMap_ApocalypseEffects");
		uint num = damnedSoulsPercentageModifier ?? TPSingleton<ApocalypseManager>.Instance.DamnedSoulsPercentageModifier;
		if (num != 0)
		{
			damnedSoulsModifier.text = string.Format(Localizer.Get("WorldMap_ApocalypseDamnedSoulsModifier"), num);
		}
		else
		{
			damnedSoulsModifier.text = string.Empty;
		}
		DisplayApocalypses();
	}

	private void DisplayApocalypses()
	{
		DisplayMoreInfo(mustDisplay: false);
		effectsDescription.text = string.Empty;
		if (MustDisplayNormalDifficulty)
		{
			effectsDescription.text = Localizer.Get("WorldMap_ApocalypseDescription_00");
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		int count = apocalypseModifierStepDefinitions.Count;
		foreach (ApocalypseModifierStepDefinition apocalypseModifierStepDefinition in apocalypseModifierStepDefinitions)
		{
			if (num >= maximumModifiersDisplayed)
			{
				break;
			}
			if (!ApocalypseDatabase.ModifierDefinitionsFromStepDefinitions.TryGetValue(apocalypseModifierStepDefinition, out var value))
			{
				Debug.LogError("Couldn't find the definition of the modifier from the step: " + apocalypseModifierStepDefinition.Id);
			}
			if (value == null)
			{
				stringBuilder.Append(apocalypseModifierStepDefinition.Id ?? "");
			}
			else
			{
				int modifierStepIndexFromDefinition = value.GetModifierStepIndexFromDefinition(apocalypseModifierStepDefinition);
				stringBuilder.Append("<style=KeyWord>• " + value.GetLocalizedTitle() + "</style> " + GetModifierStepDotsStr(value, modifierStepIndexFromDefinition));
				string viewTooltipStepValue = apocalypseModifierStepDefinition.ViewTooltipStepValue;
				if (!string.IsNullOrEmpty(viewTooltipStepValue))
				{
					stringBuilder.Append(": " + viewTooltipStepValue);
				}
			}
			if (num + 1 < maximumModifiersDisplayed && num + 1 < count)
			{
				stringBuilder.AppendLine();
			}
			num++;
		}
		int count2 = apocalypseModifierStepDefinitions.Count;
		if (maximumModifiersDisplayed < count2)
		{
			stringBuilder.AppendLine().Append(Localizer.Format("ApocalypseEffectsTooltip_MaxModifiersDisplayedReached", count2 - num));
		}
		effectsDescription.text = stringBuilder.ToString();
		if (canDisplayMoreInfo)
		{
			DisplayMoreInfo(mustDisplay: true);
		}
	}

	private void DisplayMoreInfo(bool mustDisplay)
	{
		moreInfoContainer.SetActive(mustDisplay);
	}
}
