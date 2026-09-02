using TMPro;
using TPLib.Localization;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Unit.Stat;

public class UnitStatBreakdownDisplay : MonoBehaviour
{
	[SerializeField]
	[Range(50f, 200f)]
	[Tooltip("If true, the size of numbers will be bigger than the (potential) other parts of the strings.")]
	private int breakdownValuesNumbersSizeMult = 100;

	[SerializeField]
	private bool showValueSourceTitles = true;

	[SerializeField]
	private bool canColorize = true;

	[SerializeField]
	private bool canShowAsPercentage = true;

	[SerializeField]
	private bool canShowAsModifier = true;

	[SerializeField]
	private bool canShowColon;

	[SerializeField]
	private TextMeshProUGUI valueText;

	public void Refresh(string valueString, string locaKey, bool showValue = true)
	{
		base.gameObject.SetActive(showValue);
		if (showValue)
		{
			valueText.text = ((showValueSourceTitles && locaKey != string.Empty) ? (Localizer.Get(locaKey) + (canShowColon ? ":" : string.Empty) + " ") : string.Empty) + valueString;
		}
	}

	public string FormatStatValue(float value, bool statIsPercentage, string locaKeyOverride = null)
	{
		if (locaKeyOverride != null)
		{
			return Localizer.Get(locaKeyOverride);
		}
		bool flag = breakdownValuesNumbersSizeMult != 100;
		string text = string.Format("{0}{1}", (canShowAsModifier && value > 0f) ? "+" : string.Empty, value);
		text += ((statIsPercentage && canShowAsPercentage) ? "<size=80%>%</size>" : string.Empty);
		if (canColorize)
		{
			text = "<color=#" + ColorUtility.ToHtmlStringRGBA((value > 0f) ? GameView.PositiveColor : GameView.NegativeColor) + ">" + text + "</color>";
		}
		if (flag)
		{
			text = $"<size={breakdownValuesNumbersSizeMult}%>{text}</size>";
		}
		return text;
	}

	public void RefreshAdditionalStat(AdditionalUnitStatDisplay additionalUnitStatDisplay)
	{
		UnitStatDefinition.E_Stat id = additionalUnitStatDisplay.AdditionalStatDefinition.Id;
		float clampedStatValue = TileObjectSelectionManager.SelectedUnit.GetClampedStatValue(id);
		string text = Localizer.Get("UnitStat_ShortName_" + id);
		valueText.text = string.Format("<style={0}>{1}</style>: <style=KeywordNb>{2}</style>{3}", id, text, clampedStatValue, id.ShownAsPercentage() ? "%" : "");
	}
}
