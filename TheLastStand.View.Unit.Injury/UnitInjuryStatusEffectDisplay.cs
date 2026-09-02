using System;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Model.Status;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Injury;

public class UnitInjuryStatusEffectDisplay : MonoBehaviour
{
	[SerializeField]
	private Image injuryIcon;

	[SerializeField]
	private DataSpriteTable injuryIcons;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private GameObject turnPanel;

	[SerializeField]
	private TextMeshProUGUI remainingTurnsText;

	[SerializeField]
	protected GameObject turnInfiniteIcon;

	private Status status;

	private UnitStatDefinition.E_Stat stat = UnitStatDefinition.E_Stat.Undefined;

	private float modifier;

	public virtual void Init(Status status, int injuryStage)
	{
		this.status = status;
		turnPanel.SetActive(value: true);
		if ((float)this.status.RemainingTurnsCount == -1f)
		{
			turnInfiniteIcon.SetActive(value: true);
			remainingTurnsText.gameObject.SetActive(value: false);
		}
		else
		{
			turnInfiniteIcon.SetActive(value: false);
			remainingTurnsText.gameObject.SetActive(value: true);
			remainingTurnsText.text = status.RemainingTurnsCount.ToString();
		}
		if (status is StatModifierStatus statModifierStatus)
		{
			stat = statModifierStatus.Stat;
			modifier = statModifierStatus.ModifierValue;
		}
		injuryIcon.sprite = injuryIcons.GetSpriteAt(injuryStage - 1);
		RefreshTitle();
	}

	private void Awake()
	{
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
			RefreshTitle();
		}
	}

	private void RefreshTitle()
	{
		if (stat != UnitStatDefinition.E_Stat.Undefined)
		{
			titleText.text = string.Format("{0}{1}{2} <sprite name={3}>{4}", (modifier >= 0f) ? "+" : string.Empty, modifier, stat.ShownAsPercentage() ? "%" : string.Empty, stat, UnitDatabase.UnitStatDefinitions[stat].Name);
		}
		else
		{
			titleText.text = $"<sprite name={status.StatusType}>{status.Name} {((status is PoisonStatus poisonStatus) ? $"<color=#FF0000>({poisonStatus.DamagePerTurn})</color>" : string.Empty)}";
		}
	}
}
