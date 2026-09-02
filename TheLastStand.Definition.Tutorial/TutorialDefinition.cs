using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Tutorial;

namespace TheLastStand.Definition.Tutorial;

public class TutorialDefinition : TheLastStand.Framework.Serialization.Definition
{
	public class StringToTutorialIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(TutorialDatabase.TutorialsDefinitions.Keys);
	}

	public string Id { get; private set; }

	public E_TutorialCategory Category { get; private set; }

	public E_TutorialTrigger Trigger { get; private set; }

	public List<TutorialConditionDefinition> ConditionDefinitions { get; private set; }

	public List<RewiredAction> RewiredActions { get; private set; }

	public List<string> LockTutorials { get; private set; }

	public bool HiddenUntilReadOnce { get; private set; }

	public TutorialDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		Id = xAttribute.Value;
		XElement xElement2 = xElement.Element("Category");
		if (!Enum.TryParse<E_TutorialCategory>(xElement2.Value, out var result))
		{
			CLoggerManager.Log("Could not parse TutorialDefinition " + Id + " category value " + xElement2.Value + " to a valid Category!");
			return;
		}
		Category = result;
		XElement xElement3 = xElement.Element("Conditions");
		if (xElement3 != null)
		{
			ConditionDefinitions = new List<TutorialConditionDefinition>();
			foreach (XElement item2 in xElement3.Elements())
			{
				TutorialConditionDefinition item = item2.Name.LocalName switch
				{
					"InCity" => new InCityTutorialConditionDefinition(item2), 
					"DuringDayTurn" => new DuringDayTurnTutorialConditionDefinition(item2), 
					"DuringNight" => new DuringNightTutorialConditionDefinition(item2), 
					"CurrentNightHour" => new CurrentNightHourTutorialConditionDefinition(item2), 
					"TotalActionPointsSpent" => new TotalActionPointsSpentTutorialConditionDefinition(item2), 
					"TutorialMapSkipped" => new TutorialMapSkippedTutorialConditionDefinition(item2), 
					"IsWeaponRestrictionAvailable" => new IsWeaponRestrictionAvailableTutorialConditionDefinition(item2), 
					_ => null, 
				};
				ConditionDefinitions.Add(item);
			}
		}
		XElement xElement4 = xElement.Element("Actions");
		if (xElement4 != null)
		{
			RewiredActions = new List<RewiredAction>();
			foreach (XElement item3 in xElement4.Elements("Action"))
			{
				XAttribute xAttribute2 = item3.Attribute("Id");
				XAttribute xAttribute3 = item3.Attribute("Index");
				RewiredActions.Add(new RewiredAction
				{
					RewiredLabel = xAttribute2.Value,
					SpecificIndex = int.Parse(xAttribute3.Value)
				});
			}
		}
		XElement xElement5 = xElement.Element("Trigger");
		if (!Enum.TryParse<E_TutorialTrigger>(xElement5.Value, out var result2))
		{
			CLoggerManager.Log("Could not parse TutorialDefinition " + Id + " trigger value " + xElement5.Value + " to a valid Trigger!");
			return;
		}
		Trigger = result2;
		XElement xElement6 = xElement.Element("LockTutorials");
		if (xElement6 != null)
		{
			LockTutorials = new List<string>();
			foreach (XElement item4 in xElement6.Elements("LockTutorial"))
			{
				LockTutorials.Add(item4.Value);
			}
		}
		HiddenUntilReadOnce = xElement.Element("HiddenUntilReadOnce") != null;
	}
}
