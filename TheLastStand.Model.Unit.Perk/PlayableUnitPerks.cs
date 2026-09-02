using System.Collections.Generic;
using TheLastStand.Controller.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;

namespace TheLastStand.Model.Unit.Perk;

public class PlayableUnitPerks
{
	public Dictionary<string, List<string>> ActiveReplaceEffects = new Dictionary<string, List<string>>();

	public Dictionary<string, Perk> Perks => PlayableUnit.Perks;

	public PlayableUnit PlayableUnit { get; }

	public PlayableUnitPerksController PlayableUnitPerksController { get; }

	public List<ReplaceItemSkillEffect> ReplaceItemSkillEffects { get; } = new List<ReplaceItemSkillEffect>();

	public PlayableUnitPerks(PlayableUnitPerksController playableUnitPerksController, PlayableUnit playableUnit)
	{
		PlayableUnitPerksController = playableUnitPerksController;
		PlayableUnit = playableUnit;
	}
}
