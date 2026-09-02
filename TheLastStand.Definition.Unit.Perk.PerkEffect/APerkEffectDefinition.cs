using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Unit.Perk.PerkDataCondition;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public abstract class APerkEffectDefinition : TheLastStand.Framework.Serialization.Definition
{
	public bool CanBeTriggeredByPerk;

	public PerkDataConditionsDefinition PerkDataConditionsDefinition { get; private set; }

	public APerkEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		PerkDataConditionsDefinition = new PerkDataConditionsDefinition(xElement.Element("Conditions"), base.TokenVariables);
	}
}
