using System.Collections.Generic;
using TMPro;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Unit;
using TheLastStand.View.Unit.Trait;
using UnityEngine;

namespace TheLastStand.View.CharacterSheet;

public class UnitDetailsView : TabbedPageView
{
	[SerializeField]
	private List<UnitTraitDisplay> unitTraits;

	[SerializeField]
	private TextMeshProUGUI unitNameDetails;

	public override void Close()
	{
		if (base.IsOpened)
		{
			PlayableUnitManager.TraitTooltip.Hide();
			base.Close();
		}
	}

	public override void Open()
	{
		if (!base.IsOpened)
		{
			base.Open();
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		PlayableUnitManager.TraitTooltip.Hide();
		PlayableUnit selectedPlayableUnit = TileObjectSelectionManager.SelectedPlayableUnit;
		for (int i = 0; i < unitTraits.Count; i++)
		{
			unitTraits[i].UnitTraitDefinition = ((selectedPlayableUnit.UnitTraitDefinitions.Count > i) ? selectedPlayableUnit.UnitTraitDefinitions[i] : null);
			unitTraits[i].Refresh();
		}
		unitNameDetails.text = selectedPlayableUnit.Name;
	}
}
