using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Apocalypse.ApocalypseEffects;

public class AddSkillProgressionFlagApocalypseEffectDefinition : ApocalypseEffectDefinition
{
	public string Flag { get; private set; }

	public AddSkillProgressionFlagApocalypseEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute xAttribute = (container as XElement).Attribute("Flag");
		Flag = xAttribute.Value.Replace(base.TokenVariables);
	}
}
