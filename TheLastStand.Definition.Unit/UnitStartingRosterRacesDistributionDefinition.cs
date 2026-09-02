using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class UnitStartingRosterRacesDistributionDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Constants
	{
		public const string NonHumanRaceType = "NonHuman";
	}

	public int UnlockedNonHumanRacesNb { get; private set; }

	public int HumanWeight { get; private set; }

	public int NonHumanWeight { get; private set; }

	public UnitStartingRosterRacesDistributionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("UnlockedNonHumanRacesNb");
		if (!int.TryParse(xAttribute.Value, out var result))
		{
			CLoggerManager.Log("The UnitStartingRosterRacesDistributionDefinition " + xAttribute.Value + " " + HasAnInvalidInt(xAttribute.Value), LogType.Error);
		}
		UnlockedNonHumanRacesNb = result;
		foreach (XElement item in obj.Elements("RaceDistribution"))
		{
			XAttribute xAttribute2 = item.Attribute("RaceType");
			if (xAttribute2 == null)
			{
				CLoggerManager.Log("A RaceDistribution has no RaceType !", LogType.Error);
				continue;
			}
			if (xAttribute2.Value != "Human" && xAttribute2.Value != "NonHuman")
			{
				CLoggerManager.Log("A RaceDistribution has an invalid RaceType: " + xAttribute2.Value, LogType.Error);
				continue;
			}
			string value = xAttribute2.Value;
			XAttribute xAttribute3 = item.Attribute("Weight");
			if (xAttribute3 == null)
			{
				CLoggerManager.Log("A RaceDistribution has no Weight !", LogType.Error);
				continue;
			}
			if (!int.TryParse(xAttribute3.Value, out var result2))
			{
				CLoggerManager.Log("A RaceDistribution " + xAttribute3.Value + " " + HasAnInvalidInt(xAttribute3.Value), LogType.Error);
			}
			switch (value)
			{
			case "Human":
				HumanWeight = result2;
				break;
			case "NonHuman":
				NonHumanWeight = result2;
				break;
			}
		}
	}
}
