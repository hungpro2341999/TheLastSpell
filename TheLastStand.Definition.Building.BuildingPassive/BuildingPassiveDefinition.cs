using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;

namespace TheLastStand.Definition.Building.BuildingPassive;

public class BuildingPassiveDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; private set; }

	public bool HasOnDeathEffect { get; private set; }

	public List<BuildingPassiveEffectDefinition> PassiveEffectDefinitions { get; private set; }

	public List<PassiveTriggerDefinition> TriggerDefinitions { get; private set; }

	public BuildingPassiveDefinition(XContainer container)
		: base(container)
	{
	}

	public virtual BuildingPassiveDefinition Clone()
	{
		return MemberwiseClone() as BuildingPassiveDefinition;
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		Id = xAttribute.Value;
		PassiveEffectDefinitions = new List<BuildingPassiveEffectDefinition>();
		foreach (XElement item in xElement.Elements("UpdateShopLevel"))
		{
			PassiveEffectDefinitions.Add(new UpdateShopLevelDefinition(item));
		}
		foreach (XElement item2 in xElement.Elements("UpdateBonePileLevel"))
		{
			PassiveEffectDefinitions.Add(new UpdateBonePileLevelDefinition(item2));
		}
		foreach (XElement item3 in xElement.Elements("FillEffectGauge"))
		{
			PassiveEffectDefinitions.Add(new FillEffectGaugeDefinition(item3));
		}
		foreach (XElement item4 in xElement.Elements("GenerateNewItemsRoster"))
		{
			PassiveEffectDefinitions.Add(new GenerateNewItemsRosterDefinition(item4));
		}
		foreach (XElement item5 in xElement.Elements("ImproveSpawnWaveInfo"))
		{
			PassiveEffectDefinitions.Add(new ImproveSpawnWaveInfoDefinition(item5));
		}
		foreach (XElement item6 in xElement.Elements("IncreaseWorkers"))
		{
			PassiveEffectDefinitions.Add(new IncreaseWorkersDefinition(item6));
		}
		foreach (XElement item7 in xElement.Elements("DestroyBuilding"))
		{
			PassiveEffectDefinitions.Add(new DestroyBuildingDefinition(item7));
		}
		foreach (XElement item8 in xElement.Elements("TransformBuilding"))
		{
			PassiveEffectDefinitions.Add(new TransformBuildingDefinition(item8));
		}
		foreach (XElement item9 in xElement.Elements("GenerateLightFog"))
		{
			PassiveEffectDefinitions.Add(new GenerateLightFogDefinition(item9));
		}
		foreach (XElement item10 in xElement.Elements("GenerateGuardian"))
		{
			PassiveEffectDefinitions.Add(new GenerateGuardianDefinition(item10));
		}
		foreach (XElement item11 in xElement.Elements("GainResources"))
		{
			PassiveEffectDefinitions.Add(new GainResourcesDefinition(item11));
		}
		TriggerDefinitions = new List<PassiveTriggerDefinition>();
		XElement xElement2 = xElement.Element("Triggers");
		foreach (XElement item12 in xElement2.Elements("Permanent"))
		{
			TriggerDefinitions.Add(new PermanentTriggerDefinition(item12));
		}
		foreach (XElement item13 in xElement2.Elements("StartProductionTurn"))
		{
			TriggerDefinitions.Add(new StartOfProductionTriggerDefinition(item13));
		}
		foreach (XElement item14 in xElement2.Elements("EndProductionTurn"))
		{
			TriggerDefinitions.Add(new EndOfProductionTriggerDefinition(item14));
		}
		foreach (XElement item15 in xElement2.Elements("OnDeath"))
		{
			TriggerDefinitions.Add(new OnDeathTriggerDefinition(item15));
		}
		foreach (XElement item16 in xElement2.Elements("OnConstruction"))
		{
			TriggerDefinitions.Add(new OnConstructionTriggerDefinition(item16));
		}
		foreach (XElement item17 in xElement2.Elements("AfterXProductionPhases"))
		{
			TriggerDefinitions.Add(new AfterXProductionPhasesTriggerDefinition(item17));
		}
		foreach (XElement item18 in xElement2.Elements("AfterXNightEnd"))
		{
			TriggerDefinitions.Add(new AfterXNightEndTriggerDefinition(item18));
		}
		foreach (XElement item19 in xElement2.Elements("AfterXNightTurns"))
		{
			TriggerDefinitions.Add(new AfterXNightTurnsTriggerDefinition(item19));
		}
		foreach (XElement item20 in xElement2.Elements("OnExtinguish"))
		{
			TriggerDefinitions.Add(new OnExtinguishTriggerDefinition(item20));
		}
		foreach (XElement item21 in xElement2.Elements("StartNightEnemyTurn"))
		{
			TriggerDefinitions.Add(new StartOfNightEnemyTurnTriggerDefinition(item21));
		}
		foreach (XElement item22 in xElement2.Elements("StartNightPlayableTurn"))
		{
			TriggerDefinitions.Add(new StartOfNightPlayableTurnTriggerDefinition(item22));
		}
		HasOnDeathEffect = TriggerDefinitions.Any((PassiveTriggerDefinition x) => x.EffectTime == E_EffectTime.OnDeath || x.EffectTime == E_EffectTime.OnExtinguish);
	}
}
