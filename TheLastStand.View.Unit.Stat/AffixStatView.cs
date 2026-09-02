using TMPro;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using UnityEngine;

namespace TheLastStand.View.Unit.Stat;

[SelectionBase]
public class AffixStatView : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI statModifierText;

	public void Init(UnitStatDefinition.E_Stat stat, float modifierValue)
	{
		statModifierText.text = stat.GetValueStylized(modifierValue) + "  " + $"<style={UnitDatabase.UnitStatDefinitions[stat].Id}></style>" + UnitDatabase.UnitStatDefinitions[stat].Name;
	}
}
