using System.Collections.Generic;
using System.Linq;
using TMPro;
using TPLib.Localization;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Stat;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Injury;

public class InjuryTooltip : TooltipBase
{
	public class Constants
	{
		public const string WoundIconPath = "View/Sprites/UI/Units/Injury/Icon_Wounds";

		public const string InjuryTitleWound = "Injury_Title_Wound";

		public const string InjuryPanicMalus = "Injury_Panic_Malus";
	}

	[SerializeField]
	private InjuriedStatDisplay injuriedStatDisplayPrefab;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private Image tooltipIcon;

	private List<InjuriedStatDisplay> injuriedStatDisplayPool = new List<InjuriedStatDisplay>();

	public void SetContent(InjuryDefinition injuryDefinition, int injuryIndex, TheLastStand.Model.Unit.Unit unit)
	{
		tooltipIcon.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Units/Injury/Icon_Wounds" + (injuryIndex + 1));
		title.text = Localizer.Get("Injury_Title_Wound" + (injuryIndex + 1));
		int i = 0;
		int num = 0;
		while (num < injuryDefinition.StatModifiers.Count)
		{
			AdjustPoolLength(i);
			KeyValuePair<UnitStatDefinition.E_Stat, float> keyValuePair = injuryDefinition.StatModifiers.ElementAt(num);
			UnitStatDefinition unitStatDefinition = UnitDatabase.UnitStatDefinitions[keyValuePair.Key];
			injuriedStatDisplayPool[i].Display(show: true);
			injuriedStatDisplayPool[i].RefreshAsModifier(keyValuePair.Value, keyValuePair.Key, unitStatDefinition.Id.ShownAsPercentage());
			num++;
			i++;
		}
		int num2 = 0;
		while (num2 < injuryDefinition.StatMultipliers.Count)
		{
			AdjustPoolLength(i);
			KeyValuePair<UnitStatDefinition.E_Stat, InjuryDefinition.E_ValueMultiplier> keyValuePair2 = injuryDefinition.StatMultipliers.ElementAt(num2);
			UnitStat stat = TileObjectSelectionManager.SelectedUnit.UnitStatsController.GetStat(keyValuePair2.Key);
			UnitStatDefinition unitStatDefinition2 = UnitDatabase.UnitStatDefinitions[keyValuePair2.Key];
			injuriedStatDisplayPool[i].Display(show: true);
			injuriedStatDisplayPool[i].RefreshAsMultiplier(keyValuePair2.Value, keyValuePair2.Key, stat.Base - ((keyValuePair2.Value == InjuryDefinition.E_ValueMultiplier.Half) ? Mathf.Floor(stat.Base * InjuryDefinition.Multipliers[keyValuePair2.Value]) : Mathf.Round(stat.Base * InjuryDefinition.Multipliers[keyValuePair2.Value])), unitStatDefinition2.Id.ShownAsPercentage());
			num2++;
			i++;
		}
		int num3 = 0;
		while (num3 < injuryDefinition.Statuses.Count)
		{
			AdjustPoolLength(i);
			injuriedStatDisplayPool[i].Display(show: true);
			injuriedStatDisplayPool[i].RefreshAsStatus(injuryDefinition.Statuses[num3]);
			num3++;
			i++;
		}
		List<string> list = ((unit == null) ? injuryDefinition.PreventedSkillsIds : unit.GetPreventedSkillsLocalizationIds(injuryDefinition.PreventedSkillsByGroupId));
		int num4 = 0;
		while (num4 < list.Count)
		{
			AdjustPoolLength(i);
			injuriedStatDisplayPool[i].Display(show: true);
			injuriedStatDisplayPool[i].RefreshAsPreventedSkill(list[num4]);
			num4++;
			i++;
		}
		int num5 = 0;
		while (num5 < injuryDefinition.RemoveStatusDefinitions.Count)
		{
			AdjustPoolLength(i);
			injuriedStatDisplayPool[i].Display(show: true);
			injuriedStatDisplayPool[i].RefreshAsRemovedStatus(injuryDefinition.RemoveStatusDefinitions[num5]);
			num5++;
			i++;
		}
		for (; i < injuriedStatDisplayPool.Count; i++)
		{
			injuriedStatDisplayPool[i].Display(show: false);
		}
	}

	protected override bool CanBeDisplayed()
	{
		return true;
	}

	protected override void OnDisplay()
	{
		canvas.enabled = true;
	}

	protected override void OnHide()
	{
		canvas.enabled = false;
	}

	protected override void RefreshContent()
	{
	}

	private void AdjustPoolLength(int injuryDisplayIndex)
	{
		if (injuriedStatDisplayPool.Count <= injuryDisplayIndex)
		{
			injuriedStatDisplayPool.Add(Object.Instantiate(injuriedStatDisplayPrefab, tooltipPanel.transform));
			injuriedStatDisplayPool[injuryDisplayIndex].transform.SetAsLastSibling();
		}
	}
}
