using System.Collections.Generic;
using TheLastStand.Controller.Unit.Perk;
using TheLastStand.View.Unit.Perk;

namespace TheLastStand.Model.Unit.Perk;

public class UnitPerkTier
{
	public List<Perk> Perks { get; set; } = new List<Perk>();

	public int RequiredPerksCount { get; private set; }

	public int Tier { get; private set; }

	public UnitPerkTierController UnitPerkTierController { get; private set; }

	public UnitPerkTierView UnitPerkTierView { get; private set; }

	public UnitPerkTree UnitPerkTree { get; private set; }

	public bool Available { get; set; }

	public int RerollPrice { get; set; }

	public UnitPerkTier(UnitPerkTierController controller, UnitPerkTierView view, UnitPerkTree unitPerkTree, int requiredPerksCount, int tier)
	{
		UnitPerkTierController = controller;
		UnitPerkTierView = view;
		UnitPerkTree = unitPerkTree;
		RequiredPerksCount = requiredPerksCount;
		Tier = tier;
	}
}
