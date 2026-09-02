using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.EasyMode;

public class EasyModeDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<EasyModeModifierDefinition> ModifiersDefinitions { get; private set; } = new List<EasyModeModifierDefinition>();

	public EasyModeDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in (container as XElement).Element("EasyModeModifiers").Elements())
		{
			switch (item.Name.LocalName)
			{
			case "DecreasePrices":
				ModifiersDefinitions.Add(new EasyModeDecreasePricesDefinition(item));
				break;
			case "DecreaseEnemiesCount":
				ModifiersDefinitions.Add(new EasyModeDecreaseEnemiesCountDefinition(item));
				break;
			case "IncreaseMagicCircleHealth":
				ModifiersDefinitions.Add(new EasyModeIncreaseMagicCircleHealthDefinition(item));
				break;
			case "InitResources":
				ModifiersDefinitions.Add(new EasyModeInitResourcesDefinition(item));
				break;
			}
		}
	}
}
