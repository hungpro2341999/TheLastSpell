using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Perk;

namespace TheLastStand.Definition.Skill.SkillEffect;

public abstract class SkillEffectDefinition : TheLastStand.Framework.Serialization.Definition
{
	public abstract string Id { get; }

	public virtual bool DisplayCompendiumEntry => true;

	public bool HasPerkContext => !string.IsNullOrEmpty(PerkIdContext);

	public string PerkIdContext { get; protected set; }

	public virtual bool ShouldBeDisplayed => true;

	public SkillEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public Perk GetPerkContext(ISkillCaster skillCaster)
	{
		if (HasPerkContext && skillCaster is PlayableUnit playableUnit && playableUnit.Perks.ContainsKey(PerkIdContext))
		{
			return playableUnit.Perks[PerkIdContext];
		}
		return null;
	}

	public override void Deserialize(XContainer container)
	{
		if (!string.IsNullOrEmpty(PerkDefinition.CurrentPerkId))
		{
			PerkIdContext = PerkDefinition.CurrentPerkId;
		}
	}
}
