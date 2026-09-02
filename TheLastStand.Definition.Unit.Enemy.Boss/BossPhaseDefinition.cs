using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseCondition;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Enemy.Boss;

public class BossPhaseDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Constants
	{
		public const string EndNightCleaningActionsName = "EndNightCleaningActions";

		public const string DestroyBuildingCleaningActionName = "DestroyBuildingActor";
	}

	public string Id { get; }

	public Dictionary<string, ActorDefinition> ActorDefinitions { get; } = new Dictionary<string, ActorDefinition>();

	public List<BossPhaseHandlerDefinition> BossPhaseHandlerDefinitions { get; } = new List<BossPhaseHandlerDefinition>();

	public List<IBossPhaseConditionDefinition> DefeatConditionsDefinitions { get; } = new List<IBossPhaseConditionDefinition>();

	public List<string> EndNightBuildingsActorsToCleanIds { get; } = new List<string>();

	public List<IBossPhaseConditionDefinition> VictoryConditionsDefinitions { get; } = new List<IBossPhaseConditionDefinition>();

	public BossPhaseDefinition(XContainer container, string id)
		: base(container)
	{
		Id = id;
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XElement xElement2 = xElement.Element("ActorsList");
		if (xElement2 != null)
		{
			foreach (XElement item in xElement2.Elements("Actor"))
			{
				ActorDefinition actorDefinition = new ActorDefinition(item);
				ActorDefinitions.Add(actorDefinition.ActorId, actorDefinition);
			}
		}
		XElement xElement3 = xElement.Element("VictoryConditions");
		if (xElement3 != null)
		{
			foreach (XElement item2 in xElement3.Elements())
			{
				if (BossPhaseConditionsFactory.BossPhaseConditionDefinitionFromXElement(item2, out var bossPhaseContentDefinition))
				{
					VictoryConditionsDefinitions.Add(bossPhaseContentDefinition);
				}
			}
		}
		XElement xElement4 = xElement.Element("DefeatConditions");
		if (xElement4 != null)
		{
			foreach (XElement item3 in xElement4.Elements())
			{
				if (BossPhaseConditionsFactory.BossPhaseConditionDefinitionFromXElement(item3, out var bossPhaseContentDefinition2))
				{
					DefeatConditionsDefinitions.Add(bossPhaseContentDefinition2);
				}
			}
		}
		foreach (XElement item4 in xElement.Elements("PhaseHandler"))
		{
			BossPhaseHandlerDefinitions.Add(new BossPhaseHandlerDefinition(item4));
		}
		XElement xElement5 = xElement.Element("EndNightCleaningActions");
		if (xElement5 == null)
		{
			return;
		}
		foreach (XElement item5 in xElement5.Elements("DestroyBuildingActor"))
		{
			XAttribute xAttribute = item5.Attribute("Id");
			EndNightBuildingsActorsToCleanIds.Add(xAttribute.Value);
		}
	}
}
