using System.Collections.Generic;
using TPLib;
using TheLastStand.View.CharacterSheet;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Perk;

public class PerkCollectionBannerView : UnitPerkRerollTarget
{
	[SerializeField]
	private Image crest;

	[SerializeField]
	private Image topBackground;

	[SerializeField]
	private Image collectionCompleteRerollHoverFeedback;

	[SerializeField]
	private Image collectionCompleteRerollSelectionFeedback;

	[SerializeField]
	private Sprite collectionSelectionFeedbackPayableSprite;

	[SerializeField]
	private Sprite collectionSelectionFeedbackNotPayableSprite;

	[SerializeField]
	private Animator collectionRerollSuccessAnimator;

	private static readonly int RerollHash = Animator.StringToHash("reroll");

	public int CollectionIndex { get; set; }

	protected override bool ForceHighlightSelectedPerks => CanCollectionRerollsCompletely;

	public override bool CanCollectionRerollsCompletely
	{
		get
		{
			if (DoesCollectionRerollsCompletely)
			{
				return HasPotentialReroll();
			}
			return false;
		}
	}

	public override bool IsCollectionRerollCompletelyLocked
	{
		get
		{
			if (DoesCollectionRerollsCompletely)
			{
				return !HasPotentialReroll();
			}
			return false;
		}
	}

	public override bool DoesCollectionRerollsCompletely
	{
		get
		{
			if (TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.UnitPerkTree != null)
			{
				return TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.UnitPerkTree.DoesCollectionRerollsCompletely(CollectionIndex);
			}
			return false;
		}
	}

	public override List<UnitPerkDisplay> PerkDisplays { get; } = new List<UnitPerkDisplay>();

	protected override int RerollPrice => TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.UnitPerkTree.CollectionRerollPrices[CollectionIndex];

	public void Refresh(string collectionId)
	{
		Sprite collectionAssetOrDefault = UnitPerkTreeView.GetCollectionAssetOrDefault<Sprite>("View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Top", collectionId);
		topBackground.sprite = collectionAssetOrDefault;
		collectionAssetOrDefault = UnitPerkTreeView.GetCollectionAssetOrDefault<Sprite>("View/Sprites/UI/CharacterSheet/PerkTree/{0}/Crest_{0}_Off", collectionId);
		crest.sprite = collectionAssetOrDefault;
	}

	public override bool HasPotentialReroll()
	{
		return TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.UnitPerkTree.UnitPerkTreeController.CollectionHasPotentialReroll(CollectionIndex);
	}

	public override void ExecuteReroll()
	{
		if (CanPayTargetReroll())
		{
			TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.UnitPerkTree.UnitPerkTreeController.TryRerollPerksInCollection(CollectionIndex);
			if (DoesCollectionRerollsCompletely)
			{
				collectionRerollSuccessAnimator.SetTrigger(RerollHash);
			}
		}
	}

	public override void ChangeSelection(bool isSelected)
	{
		base.ChangeSelection(isSelected);
		bool flag = isSelected && CanCollectionRerollsCompletely;
		collectionCompleteRerollSelectionFeedback.enabled = flag;
		if (flag)
		{
			collectionCompleteRerollSelectionFeedback.sprite = (CanPayTargetReroll() ? collectionSelectionFeedbackPayableSprite : collectionSelectionFeedbackNotPayableSprite);
		}
	}

	public override void OnRerollPointerEnter()
	{
		base.OnRerollPointerEnter();
		collectionCompleteRerollHoverFeedback.enabled = CanCollectionRerollsCompletely;
	}

	public override void OnRerollPointerExit()
	{
		base.OnRerollPointerExit();
		collectionCompleteRerollHoverFeedback.enabled = false;
	}
}
