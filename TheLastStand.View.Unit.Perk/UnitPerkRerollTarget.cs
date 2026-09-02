using System.Collections.Generic;
using TMPro;
using TPLib;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.View.CharacterSheet;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Perk;

public abstract class UnitPerkRerollTarget : MonoBehaviour, IPerkRerollSelectable
{
	[SerializeField]
	private CanvasGroup rerollSelectionButtonParent;

	[SerializeField]
	private BetterButton rerollSelectionButton;

	[SerializeField]
	private TextMeshProUGUI damnedSoulsCountText;

	[SerializeField]
	private TextMeshProUGUI tooExpensiveFeedback;

	[SerializeField]
	private Image selectionFeedback;

	[SerializeField]
	private Sprite selectionFeedbackPayableSprite;

	[SerializeField]
	private Sprite selectionFeedbackNotPayableSprite;

	private bool IsSelected { get; set; }

	protected abstract int RerollPrice { get; }

	public abstract List<UnitPerkDisplay> PerkDisplays { get; }

	protected virtual bool ForceHighlightSelectedPerks => false;

	public virtual bool CanCollectionRerollsCompletely => false;

	public virtual bool DoesCollectionRerollsCompletely => false;

	public virtual bool IsCollectionRerollCompletelyLocked => false;

	public void DisplayRerollFeedback(bool show)
	{
		rerollSelectionButtonParent.Display(show);
	}

	public virtual void ChangeSelection(bool isSelected)
	{
		IsSelected = isSelected;
		selectionFeedback.enabled = isSelected;
		bool canReroll = isSelected && CanReroll();
		if (isSelected)
		{
			tooExpensiveFeedback.enabled = !CanPayTargetReroll();
			selectionFeedback.sprite = (canReroll ? selectionFeedbackPayableSprite : selectionFeedbackNotPayableSprite);
		}
		else
		{
			tooExpensiveFeedback.enabled = false;
		}
		PerkDisplays.ForEach(delegate(UnitPerkDisplay perkDisplay)
		{
			perkDisplay.OnRerollSelection(isSelected, ForceHighlightSelectedPerks, canReroll);
		});
		if (!IsSelected)
		{
			OnRerollPointerExit();
		}
	}

	public void OnRerollButtonClick()
	{
		if ((UnitPerkRerollTarget)TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.RerollTargetSelected == this)
		{
			TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.ChangeTargetSelectedToReroll(null);
			rerollSelectionButton.UnSelect();
		}
		else
		{
			TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.ChangeTargetSelectedToReroll(this);
		}
		damnedSoulsCountText.text = $"<style=\"DamnedSouls\">{RerollPrice}</style>";
	}

	public virtual void OnRerollPointerEnter()
	{
		damnedSoulsCountText.text = $"<style=\"DamnedSouls\">{RerollPrice}</style>";
		PerkDisplays.ForEach(delegate(UnitPerkDisplay perkDisplay)
		{
			perkDisplay.OnRerollPointerEnter(ForceHighlightSelectedPerks);
		});
	}

	public virtual void OnRerollPointerExit()
	{
		if (!IsSelected)
		{
			damnedSoulsCountText.text = string.Empty;
		}
		PerkDisplays.ForEach(delegate(UnitPerkDisplay perkDisplay)
		{
			perkDisplay.OnRerollPointerExit();
		});
	}

	public virtual void OnJoystickSelect()
	{
		OnRerollButtonClick();
	}

	public bool CanReroll()
	{
		if (TPSingleton<SinkManager>.Instance.IsSinkUnlocked && HasPotentialReroll())
		{
			return CanPayTargetReroll();
		}
		return false;
	}

	public bool CanPayTargetReroll()
	{
		return ApplicationManager.Application.DamnedSouls >= RerollPrice;
	}

	public abstract bool HasPotentialReroll();

	public abstract void ExecuteReroll();

	protected virtual void Awake()
	{
		damnedSoulsCountText.text = string.Empty;
	}
}
