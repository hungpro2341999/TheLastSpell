using System.Collections;
using TPLib;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Status;
using TheLastStand.View.HUD;
using UnityEngine;

namespace TheLastStand.View.Unit.UI;

public class PlayableUnitHUD : UnitHUD
{
	[SerializeField]
	protected UnitStatGaugeDisplay manaDisplay;

	[SerializeField]
	protected Sprite stunHighlightSprite;

	[SerializeField]
	protected Sprite friendlyFireHighlightSprite;

	[SerializeField]
	protected Sprite baseHighlightSprite;

	[SerializeField]
	protected GameObject stunStatusGo;

	public override bool Highlight
	{
		get
		{
			if (highlightImage != null)
			{
				return highlightImage.enabled;
			}
			return false;
		}
		set
		{
			if (highlightImage != null)
			{
				highlightCanvas.enabled = value || (base.Unit.StatusOwned & Status.E_StatusType.Stun) != 0;
				highlightImage.sprite = (((base.Unit.StatusOwned & Status.E_StatusType.Stun) != Status.E_StatusType.None) ? stunHighlightSprite : baseHighlightSprite);
			}
		}
	}

	protected override bool ShouldArmorBeDisplayed
	{
		get
		{
			if (base.ShouldArmorBeDisplayed)
			{
				return TPSingleton<GameManager>.Instance.Game.State != Game.E_State.CutscenePlaying;
			}
			return false;
		}
	}

	protected override bool ShouldHealthBeDisplayed => TPSingleton<GameManager>.Instance.Game.State != Game.E_State.CutscenePlaying;

	public override void OnSkillTargetHover(bool hover)
	{
		base.OnSkillTargetHover(hover);
		(base.Unit.UnitView as PlayableUnitView).PortraitPanel.SkillTargetingMark.SetHoverAnimatorState(hover);
	}

	public void PlayManaGainAnim(float manaGain, float manaAfterGain)
	{
		StartCoroutine(GaugeManaGainDisplayCoroutine(manaGain, manaAfterGain));
	}

	public void PlayManaLossAnim(float manaLoss, float manaAfterLoss)
	{
		StartCoroutine(GaugeManaLossDisplayCoroutine(manaLoss, manaAfterLoss));
	}

	public void RefreshMana()
	{
		if (base.Unit != null)
		{
			manaDisplay.RefreshStatInstantly(base.Unit.GetClampedStatValue(UnitStatDefinition.E_Stat.Mana), base.Unit.GetClampedStatValue(UnitStatDefinition.E_Stat.ManaTotal));
			DisplayHealthIfNeeded();
		}
	}

	public override void RefreshStat(UnitStatDefinition.E_Stat stat)
	{
		if ((uint)(stat - 4) <= 1u)
		{
			RefreshMana();
		}
		else
		{
			base.RefreshStat(stat);
		}
	}

	public override void RefreshStatuses()
	{
		if (base.Unit != null)
		{
			Highlight = base.Unit.StatusOwned.HasFlag(Status.E_StatusType.Stun);
			stunStatusGo?.SetActive(base.Unit.StatusOwned.HasFlag(Status.E_StatusType.Stun));
			base.RefreshStatuses();
		}
	}

	protected override void OnChildViewToggled(bool toggle)
	{
	}

	private IEnumerator GaugeManaGainDisplayCoroutine(float manaGain, float manaAfterGain)
	{
		if (!(manaGain <= 0f))
		{
			float targetNormalizedValue = manaAfterGain / base.Unit.GetClampedStatValue(UnitStatDefinition.E_Stat.ManaTotal);
			yield return manaDisplay.IncreaseDisplayCoroutine(targetNormalizedValue);
		}
	}

	private IEnumerator GaugeManaLossDisplayCoroutine(float manaLoss, float manaAfterLoss)
	{
		if (!(manaLoss <= 0f))
		{
			float targetNormalizedValue = manaAfterLoss / base.Unit.GetClampedStatValue(UnitStatDefinition.E_Stat.ManaTotal);
			yield return manaDisplay.DecreaseDisplayCoroutine(targetNormalizedValue);
		}
	}
}
