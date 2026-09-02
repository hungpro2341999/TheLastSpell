using TPLib;
using TheLastStand.Model.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Stat;

public class StatLevelUpDisplay : UnitStatDisplay
{
	[SerializeField]
	private GameObject normalDisplayGameObject;

	[SerializeField]
	private GameObject maxReachedDisplayGameObject;

	[SerializeField]
	private Button upgradeButton;

	[Tooltip("Color the 'Texts To Color' this way if the max BaseValue has been reached")]
	[SerializeField]
	private DataColor maxReachedColor;

	private bool maxReached;

	private bool MaxReached
	{
		get
		{
			return maxReached;
		}
		set
		{
			if (maxReached != value)
			{
				maxReached = value;
				RefreshColor();
				RefreshMaxReachedDependantStuff();
			}
		}
	}

	protected override void CacheStatValues()
	{
		base.CacheStatValues();
		float num = ((base.SecondaryStatDefinition == null) ? Mathf.Floor(base.TargetUnit.UnitStatsController.GetStat(base.StatDefinition.Id).Boundaries.y) : Mathf.Floor(base.TargetUnit.UnitStatsController.GetStat(base.SecondaryStatDefinition.Id).Boundaries.y));
		MaxReached = base.TargetUnit.UnitStatsController.GetStat(base.SecondaryStatDefinition?.Id ?? base.StatDefinition.Id).Base >= num;
	}

	protected override void RefreshInternal()
	{
		base.RefreshInternal();
		RefreshUpgradeButton();
		if (fullRefreshNeeded)
		{
			RefreshMaxReachedDependantStuff();
		}
	}

	private void RefreshUpgradeButton()
	{
		if (upgradeButton == null)
		{
			return;
		}
		if (!valuesCached)
		{
			CacheStatValues();
		}
		upgradeButton.gameObject.SetActive((base.TargetUnit as PlayableUnit).StatsPoints > 0 && !MaxReached);
		if (fullRefreshNeeded)
		{
			upgradeButton.onClick.RemoveAllListeners();
			if (base.SecondaryStatDefinition == null)
			{
				_ = base.StatDefinition.Id;
			}
			else
			{
				_ = base.SecondaryStatDefinition.Id;
			}
		}
	}

	protected override void RefreshColor()
	{
		if (labelsToTint == null || labelsToTint.Length == 0)
		{
			return;
		}
		Color? color = null;
		if (!base.ColorOverride.HasValue)
		{
			if (maxReachedColor != null && MaxReached)
			{
				color = maxReachedColor._Color;
			}
			else if (useStatColor && useStatColor)
			{
				color = statsColors.GetColorById(base.StatDefinition.Id.ToString());
			}
		}
		else
		{
			color = base.ColorOverride.Value;
		}
		int i = 0;
		for (int num = labelsToTint.Length; i < num; i++)
		{
			if (labelsToTint[i] != null)
			{
				labelsToTint[i].color = color ?? textsOriginalColor[i];
			}
		}
	}

	private void RefreshMaxReachedDependantStuff()
	{
		if (normalDisplayGameObject != null)
		{
			normalDisplayGameObject.SetActive(!MaxReached);
		}
		if (maxReachedDisplayGameObject != null)
		{
			maxReachedDisplayGameObject.SetActive(MaxReached);
		}
	}
}
