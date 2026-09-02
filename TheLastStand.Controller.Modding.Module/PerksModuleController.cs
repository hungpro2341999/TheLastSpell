using System;
using System.Collections.Generic;
using System.IO;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Modding.ModuleConfig.Perks;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Model.Modding.Module;

namespace TheLastStand.Controller.Modding.Module;

public class PerksModuleController : ModuleController
{
	public PerksModule PerksModule => module as PerksModule;

	public PerksModuleController(DirectoryInfo directory)
		: base(directory)
	{
		module = new PerksModule(this, directory);
		if (PerksModule.PerksModuleConfigDefinition.ModdedPerkCollectionsSetsDefinitions == null || PerksModule.PerksModuleConfigDefinition.ModdedPerkCollectionsSetsDefinitions.Count == 0)
		{
			return;
		}
		foreach (ModdedPerkCollectionsSetsDefinition moddedPerkCollectionsSetsDefinition in PerksModule.PerksModuleConfigDefinition.ModdedPerkCollectionsSetsDefinitions)
		{
			if (moddedPerkCollectionsSetsDefinition.PerkCollectionSetDefinitions.Count == 0)
			{
				continue;
			}
			foreach (KeyValuePair<int, UnitPerkCollectionSetDefinition> perkCollectionSetDefinition in moddedPerkCollectionsSetsDefinition.PerkCollectionSetDefinitions)
			{
				int key = perkCollectionSetDefinition.Key;
				if (PlayableUnitDatabase.UnitPerkTemplateDefinition.UnitPerkCollectionSetDefinitions.TryGetValue(key, out var value))
				{
					foreach (Tuple<string, int, string> collectionAndWeight in perkCollectionSetDefinition.Value.CollectionsPerWeight)
					{
						value.CollectionsPerWeight.RemoveWhere((Tuple<string, int, string> x) => x.Item1 == collectionAndWeight.Item1);
						value.CollectionsPerWeight.Add(collectionAndWeight);
					}
				}
				else
				{
					PlayableUnitDatabase.UnitPerkTemplateDefinition.UnitPerkCollectionSetDefinitions[key] = perkCollectionSetDefinition.Value;
				}
			}
		}
	}
}
