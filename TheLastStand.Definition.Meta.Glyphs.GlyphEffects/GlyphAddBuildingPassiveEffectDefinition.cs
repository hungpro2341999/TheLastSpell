using System.Collections.Generic;
using System.Xml.Linq;
using Sirenix.Utilities;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphAddBuildingPassiveEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "AddBuildingPassive";

	public BuildingPassiveDefinition BuildingPassiveDefinition { get; private set; }

	public HashSet<string> BuildingIds { get; } = new HashSet<string>();

	public GlyphAddBuildingPassiveEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		string text = xElement.Attribute("PassiveId").Value.Replace(base.TokenVariables);
		if (BuildingDatabase.BuildingPassiveDefinitions.TryGetValue(text, out var value))
		{
			BuildingPassiveDefinition = value;
		}
		else
		{
			CLoggerManager.Log("AddBuildingPassive BuildingPassive " + text + " was not found!", LogType.Error, CLogLevel.MAJOR);
		}
		foreach (XElement item in xElement.Elements("Building"))
		{
			XAttribute xAttribute = item.Attribute("Id");
			BuildingIds.Add(xAttribute.Value);
		}
		foreach (XElement item2 in xElement.Elements("BuildingsList"))
		{
			XAttribute xAttribute2 = item2.Attribute("Id");
			BuildingIds.AddRange(GenericDatabase.IdsListDefinitions[xAttribute2.Value].Ids);
		}
	}
}
