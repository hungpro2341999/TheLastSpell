using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Apocalypse.ApocalypseEffects;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Apocalypse;

public class ApocalypseDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<ApocalypseEffectDefinition> Effects { get; private set; } = new List<ApocalypseEffectDefinition>();

	public int Id { get; private set; }

	public ApocalypseDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		if (!int.TryParse(xAttribute.Value, out var result))
		{
			CLoggerManager.Log("An apocalypse's Id " + HasAnInvalidInt(xAttribute.Value) + " !", LogType.Error);
			return;
		}
		Id = result;
		foreach (XElement item8 in xElement.Element("Effects").Elements())
		{
			if (item8.Name.LocalName == "EnemiesStatBaseValueModifier")
			{
				EnemiesStatBaseValueModifierApocalypseEffectDefinition item = new EnemiesStatBaseValueModifierApocalypseEffectDefinition(item8);
				Effects.Add(item);
			}
			else if (item8.Name.LocalName == "GenerateFogSpawners")
			{
				GenerateFogSpawnersApocalypseEffectDefinition item2 = new GenerateFogSpawnersApocalypseEffectDefinition(item8);
				Effects.Add(item2);
			}
			else if (item8.Name.LocalName == "GenerateMalusAffixes")
			{
				GenerateMalusAffixesApocalypseEffectDefinition item3 = new GenerateMalusAffixesApocalypseEffectDefinition(item8);
				Effects.Add(item3);
			}
			else if (item8.Name.LocalName == "IncreaseEnemiesNumber")
			{
				IncreaseEnemiesNumberApocalypseEffectDefinition item4 = new IncreaseEnemiesNumberApocalypseEffectDefinition(item8);
				Effects.Add(item4);
			}
			else if (item8.Name.LocalName == "IncreasePrices")
			{
				IncreasePricesApocalypseEffectDefinition item5 = new IncreasePricesApocalypseEffectDefinition(item8);
				Effects.Add(item5);
			}
			else if (item8.Name.LocalName == "IncreaseStartingFogDensity")
			{
				IncreaseStartingFogDensityApocalypseEffectDefinition item6 = new IncreaseStartingFogDensityApocalypseEffectDefinition(item8);
				Effects.Add(item6);
			}
			else if (item8.Name.LocalName == "IncreaseDailyFogUpdateFrequency")
			{
				IncreaseDailyFogUpdateFrequencyApocalypseEffectDefinition item7 = new IncreaseDailyFogUpdateFrequencyApocalypseEffectDefinition(item8);
				Effects.Add(item7);
			}
			else
			{
				CLoggerManager.Log("Unhandled Apocalypse effect name " + item8.Name.LocalName + "!", LogType.Error);
			}
		}
	}
}
