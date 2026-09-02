using System.Linq;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.Unit.Perk;

namespace TheLastStand.Controller.Unit.Perk;

public class UnitPerkTierController
{
	public UnitPerkTier UnitPerkTier { get; private set; }

	public UnitPerkTierController(UnitPerkTierView perkTierView, UnitPerkTree unitPerkTree, int requiredPerksCount, int tier)
	{
		UnitPerkTier = new UnitPerkTier(this, perkTierView, unitPerkTree, requiredPerksCount, tier);
	}

	public bool HasPotentialReroll()
	{
		return UnitPerkTier.Perks.Any((TheLastStand.Model.Unit.Perk.Perk perk) => !perk.UnlockedInPerkTree && perk.HasPotentialReroll());
	}

	public void Unlock()
	{
		UnitPerkTier.Available = true;
	}
}
