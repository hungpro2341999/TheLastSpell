using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class LineOfSightDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<string> BuildingBlockingExceptions { get; private set; }

	public List<string> EnemyUnitsBlocking { get; set; }

	public bool PlayableUnitsBlocking { get; set; }

	public LineOfSightDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container.Element("LineOfSightDefinition");
		if (xElement == null)
		{
			Debug.LogError("The document must have LineOfSightDefinition");
			return;
		}
		XElement xElement2 = xElement.Element("Buildings");
		if (xElement2 != null)
		{
			BuildingBlockingExceptions = new List<string>();
			foreach (XElement item in xElement2.Elements("BuildingException"))
			{
				XAttribute xAttribute = item.Attribute("Id");
				if (xAttribute.IsNullOrEmpty())
				{
					Debug.LogError("BuildingException must have a valid Id");
				}
				else
				{
					BuildingBlockingExceptions.Add(xAttribute.Value);
				}
			}
		}
		XElement xElement3 = xElement.Element("PlayableUnits");
		PlayableUnitsBlocking = xElement3 != null;
		XElement xElement4 = xElement.Element("EnemyUnits");
		if (xElement4 == null)
		{
			return;
		}
		EnemyUnitsBlocking = new List<string>();
		foreach (XElement item2 in xElement4.Elements("EnemyUnitsBlocking"))
		{
			XAttribute xAttribute2 = item2.Attribute("Id");
			if (xAttribute2.IsNullOrEmpty())
			{
				Debug.LogError("EnemyException must have a valid Id ");
			}
			else if (!EnemyUnitsBlocking.Contains(xAttribute2.Value))
			{
				EnemyUnitsBlocking.Add(xAttribute2.Value);
			}
		}
	}
}
