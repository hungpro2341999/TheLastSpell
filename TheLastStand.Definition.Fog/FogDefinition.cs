using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database.Fog;
using TheLastStand.Definition.Hazard;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Fog;

public class FogDefinition : HazardDefinition
{
	public struct FogDayException
	{
		public int DayNumber;

		public string FogDensityName;

		public FogDayException(int dayNumber, string fogDensityName)
		{
			DayNumber = dayNumber;
			FogDensityName = fogDensityName;
		}
	}

	public struct FogDensity
	{
		public string Name;

		public int Value;

		public FogDensity(string name, int value)
		{
			Name = name;
			Value = value;
		}
	}

	public List<FogDayException> DayExceptions { get; private set; }

	public string Id { get; private set; }

	public int IncreaseEveryXDays { get; private set; }

	public int InitialDensityIndex { get; private set; }

	public List<FogDensity> FogDensities { get; private set; }

	public override E_HazardType HazardType => E_HazardType.Fog;

	public FogDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		FogDefinition fogDefinition = null;
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute.IsNullOrEmpty())
		{
			CLoggerManager.Log("FogDefinition must have an Id!", LogType.Error);
			return;
		}
		Id = xAttribute.Value;
		XAttribute xAttribute2 = xElement.Attribute("TemplateId");
		if (xAttribute2 != null)
		{
			fogDefinition = FogDatabase.FogsDefinitions[xAttribute2.Value];
		}
		XElement xElement2 = xElement.Element("IncreaseEveryXDays");
		if (xElement2.IsNullOrEmpty())
		{
			if (fogDefinition == null)
			{
				CLoggerManager.Log("FogDefinition " + Id + " has no IncreaseEveryXDays and no Template to copy it from!", LogType.Error);
				return;
			}
			IncreaseEveryXDays = fogDefinition.IncreaseEveryXDays;
		}
		else
		{
			if (!int.TryParse(xElement2.Value, out var result))
			{
				CLoggerManager.Log("FogDefinition " + Id + " IncreaseEveryXDays " + HasAnInvalidInt(xElement2.Value) + " !", LogType.Error);
				return;
			}
			IncreaseEveryXDays = result;
		}
		XElement xElement3 = xElement.Element("InitialDensityIndex");
		if (xElement3.IsNullOrEmpty())
		{
			if (fogDefinition == null)
			{
				CLoggerManager.Log("FogDefinition " + Id + " has no InitialDensityIndex and no Template to copy it from!", LogType.Error);
				return;
			}
			InitialDensityIndex = fogDefinition.InitialDensityIndex;
		}
		else
		{
			if (!int.TryParse(xElement3.Value, out var result2))
			{
				CLoggerManager.Log("FogDefinition " + Id + " InitialDensityIndex " + HasAnInvalidInt(xElement3.Value) + " !", LogType.Error);
				return;
			}
			InitialDensityIndex = result2;
		}
		XElement xElement4 = xElement.Element("FogDensities");
		if (xElement4 == null)
		{
			if (fogDefinition == null)
			{
				CLoggerManager.Log("FogDefinition " + Id + " has no FogDensities and no Template to copy it from!", LogType.Error);
				return;
			}
			FogDensities = new List<FogDensity>(fogDefinition.FogDensities);
		}
		else
		{
			FogDensities = new List<FogDensity>();
			foreach (XElement item in xElement4.DescendantNodes())
			{
				string value = item.Attribute("Name").Value;
				if (!int.TryParse(item.Attribute("Value").Value, out var result3))
				{
					CLoggerManager.Log("FogDefinition's FogDensity " + HasAnInvalidInt(item.Attribute("Value").Value), LogType.Error);
					return;
				}
				FogDensities.Add(new FogDensity(value, result3));
			}
			FogDensities = FogDensities.OrderByDescending((FogDensity o) => o.Value).ToList();
			if (FogDensities.Count == 0)
			{
				CLoggerManager.Log("Error while deserializing FogDefinition : There should be at least one FogDensity specified.", LogType.Error);
			}
		}
		DayExceptions = new List<FogDayException>();
		XElement xElement6 = xElement.Element("DayExceptions");
		if (xElement6 == null)
		{
			if (fogDefinition == null)
			{
				return;
			}
			{
				foreach (FogDayException dayException in fogDefinition.DayExceptions)
				{
					DayExceptions.Add(new FogDayException(dayException.DayNumber, dayException.FogDensityName));
				}
				return;
			}
		}
		List<XElement> list = new List<XElement>(xElement6.Elements("DayException"));
		if (list == null || list.Count <= 0)
		{
			return;
		}
		foreach (XElement item2 in list)
		{
			XAttribute xAttribute3 = item2.Attribute("DayId");
			if (!int.TryParse(xAttribute3.Value, out var result4))
			{
				CLoggerManager.Log("Could not parse the DayId attribute of DayException in FogDefinition " + Id + " into an int. value : " + xAttribute3.Value, LogType.Error);
				break;
			}
			XElement xElement7 = item2.Element("FogDensity");
			DayExceptions.Add(new FogDayException(result4, xElement7.Value));
		}
	}
}
