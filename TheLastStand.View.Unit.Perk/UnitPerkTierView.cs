using System;
using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Database.Unit;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.CharacterSheet;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Perk;

public class UnitPerkTierView : UnitPerkRerollTarget
{
	[SerializeField]
	private float unavailableOpacity;

	[SerializeField]
	private TextMeshProUGUI requiredPerksText;

	[SerializeField]
	private Image perksTierIndex;

	[SerializeField]
	private Image separator;

	[SerializeField]
	private List<UnitPerkDisplay> perkDisplays = new List<UnitPerkDisplay>();

	[SerializeField]
	private Material unlockedPerkMaterial;

	[SerializeField]
	private Material availablePerkMaterial;

	[SerializeField]
	private Material unavailablePerkMaterial;

	[SerializeField]
	private Material availableBackgroundMaterial;

	[SerializeField]
	private Material unavailableBackgroundMaterial;

	[SerializeField]
	private Material availablePerkTierIndexMaterial;

	[SerializeField]
	private Material unavailablePerkTierIndexMaterial;

	[SerializeField]
	private Sprite thresholdOn;

	[SerializeField]
	private Sprite thresholdOff;

	public RectTransform Separator => separator.rectTransform;

	public UnitPerkTier UnitPerkTier { get; set; }

	public override List<UnitPerkDisplay> PerkDisplays => perkDisplays;

	public int Tier { get; set; }

	protected override int RerollPrice => UnitPerkTier.RerollPrice;

	public void RefreshAvailability(bool isFirstUnavailable)
	{
		perksTierIndex.material = (UnitPerkTier.Available ? availablePerkTierIndexMaterial : unavailablePerkTierIndexMaterial);
		foreach (UnitPerkDisplay perkDisplay in PerkDisplays)
		{
			perkDisplay.SetAvailabilityMaterial((perkDisplay.Perk != null && perkDisplay.IsPerkUnlockedForCurrentDisplay()) ? unlockedPerkMaterial : (UnitPerkTier.Available ? availablePerkMaterial : unavailablePerkMaterial), UnitPerkTier.Available ? availableBackgroundMaterial : unavailableBackgroundMaterial);
		}
		for (int i = 0; i < PerkDisplays.Count; i++)
		{
			PerkDisplays[i].Refresh();
		}
		separator.sprite = (isFirstUnavailable ? thresholdOn : thresholdOff);
		requiredPerksText.gameObject.SetActive(isFirstUnavailable);
		if (isFirstUnavailable)
		{
			RefreshText();
		}
	}

	public void RefreshText()
	{
		if (UnitPerkTier != null)
		{
			requiredPerksText.text = Localizer.Format("CharacterSheet_RequiredPerks", PlayableUnitDatabase.UnitPerkTemplateDefinition.RequiredPerksCountPerTier[Tier + 1] - UnitPerkTier.UnitPerkTree.PlayableUnit.UnlockedPerksCount);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		if (base.gameObject.activeInHierarchy)
		{
			RefreshText();
		}
	}

	public override bool HasPotentialReroll()
	{
		return UnitPerkTier.UnitPerkTierController.HasPotentialReroll();
	}

	public override void ExecuteReroll()
	{
		if (CanPayTargetReroll())
		{
			TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.UnitPerkTree.UnitPerkTreeController.RerollPerksInTier(Tier);
		}
	}
}
