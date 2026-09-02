using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Modding.ModuleConfig.Perks;

public class ModdedPerkCollectionsDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<UnitPerkCollectionDefinition> PerkCollectionDefinitions { get; } = new List<UnitPerkCollectionDefinition>();

	public ModdedPerkCollectionsDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		IEnumerable<XElement> enumerable = (container as XDocument).Element("UnitPerkCollectionDefinitions")?.Elements("UnitPerkCollectionDefinition");
		if (enumerable == null)
		{
			return;
		}
		foreach (XElement item in enumerable)
		{
			UnitPerkCollectionDefinition unitPerkCollectionDefinition = new UnitPerkCollectionDefinition(item);
			PerkCollectionDefinitions.Add(unitPerkCollectionDefinition);
			PlayableUnitDatabase.UnitPerkCollectionDefinitions[unitPerkCollectionDefinition.Id] = unitPerkCollectionDefinition;
		}
	}
}
