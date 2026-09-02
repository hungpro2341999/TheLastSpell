using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class UnitLevelUpDefinition : TheLastStand.Framework.Serialization.Definition
{
	public ProbabilityTreeEntriesDefinition RaritiesList { get; private set; }

	public List<int> MainStatDraws { get; private set; } = new List<int>();

	public int MaxAmountOfReroll { get; private set; }

	public List<int> SecondaryStatDraws { get; private set; } = new List<int>();

	public UnitLevelUpDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		XElement xElement = xContainer as XElement;
		XElement xElement2 = xElement.Element("RaritiesList");
		if (xElement2.IsNullOrEmpty())
		{
			Debug.LogError("UnitLevelUpDefinition must have a RaritiesList");
			return;
		}
		RaritiesList = new ProbabilityTreeEntriesDefinition(xElement2);
		int.TryParse(xElement.Element("MaxAmountOfReroll").Value, out var result);
		MaxAmountOfReroll = result;
		foreach (XElement item in xElement.Element("MainStatDraws").Elements("Draw"))
		{
			if (item.IsNullOrEmpty())
			{
				TPDebug.Log("ConstructionDefinition must have a Draw");
				return;
			}
			if (!int.TryParse(item.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result2))
			{
				TPDebug.Log("MagicCircle Draw must be a valid int");
				return;
			}
			MainStatDraws.Add(result2);
		}
		foreach (XElement item2 in xElement.Element("SecondaryStatDraws").Elements("Draw"))
		{
			if (item2.IsNullOrEmpty())
			{
				TPDebug.Log("ConstructionDefinition must have a Draw");
				break;
			}
			if (!int.TryParse(item2.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result3))
			{
				TPDebug.Log("MagicCircle Draw must be a valid int");
				break;
			}
			SecondaryStatDraws.Add(result3);
		}
	}
}
