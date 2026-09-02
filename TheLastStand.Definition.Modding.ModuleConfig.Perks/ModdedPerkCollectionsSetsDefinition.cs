using System.Collections.Generic;
using System.Xml.Linq;
using Sirenix.Utilities;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Modding.ModuleConfig.Perks;

public class ModdedPerkCollectionsSetsDefinition : TheLastStand.Framework.Serialization.Definition
{
	public Dictionary<int, UnitPerkCollectionSetDefinition> PerkCollectionSetDefinitions { get; } = new Dictionary<int, UnitPerkCollectionSetDefinition>();

	public ModdedPerkCollectionsSetsDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		IEnumerable<XElement> enumerable = (container as XDocument).Element("UnitPerkCollectionSetDefinitions")?.Elements("UnitPerkCollectionSetDefinition");
		if (enumerable == null)
		{
			return;
		}
		foreach (XElement item in enumerable)
		{
			UnitPerkCollectionSetDefinition unitPerkCollectionSetDefinition = new UnitPerkCollectionSetDefinition(item);
			if (PerkCollectionSetDefinitions.ContainsKey(unitPerkCollectionSetDefinition.Index))
			{
				PerkCollectionSetDefinitions[unitPerkCollectionSetDefinition.Index].CollectionsPerWeight.AddRange(unitPerkCollectionSetDefinition.CollectionsPerWeight);
			}
			else
			{
				PerkCollectionSetDefinitions[unitPerkCollectionSetDefinition.Index] = unitPerkCollectionSetDefinition;
			}
		}
	}
}
