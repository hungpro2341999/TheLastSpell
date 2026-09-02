using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Trait;

public class UnitTraitGenerationDefinition : TheLastStand.Framework.Serialization.Definition
{
	public int StartTraitTotalPoints { get; private set; }

	public int StartTraitTotalPointsWithModifiers => StartTraitTotalPoints + TPSingleton<MetaUpgradesManager>.Instance.ComputeStartTraitTotalPointsModifiers();

	public Vector2Int UnitTraitPointBoundaries { get; private set; }

	public Vector2Int UnitTraitPointBoundariesWithModifiers => UnitTraitPointBoundaries + TPSingleton<MetaUpgradesManager>.Instance.ComputeUnitTraitPointsBoundariesModifiers();

	public UnitTraitGenerationDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container.Element("UnitTraitGenerationDefinition");
		XElement xElement2 = xElement.Element("StartTraitTotalPoints");
		if (xElement2 == null)
		{
			TPDebug.LogError("UnitTraitGenerationDefinition must have an element StartTraitTotalPoints");
			return;
		}
		XAttribute xAttribute = xElement2.Attribute("Value");
		if (xAttribute.IsNullOrEmpty())
		{
			TPDebug.LogError("UnitTraitGenerationDefinition StartTraitTotalPoints must have an attribute Value");
			return;
		}
		if (!int.TryParse(xAttribute.Value, out var result))
		{
			TPDebug.LogError("UnitTraitGenerationDefinition StartTraitTotalPoints must have a valid attribute Value (int)");
			return;
		}
		StartTraitTotalPoints = result;
		XElement xElement3 = xElement.Element("UnitTraitPointsBoundaries");
		if (xElement3 == null)
		{
			TPDebug.LogError("UnitTraitGenerationDefinition must have an element UnitTraitPointsBoundaries");
			return;
		}
		XAttribute xAttribute2 = xElement3.Attribute("Min");
		if (xAttribute2.IsNullOrEmpty())
		{
			TPDebug.LogError("UnitTraitGenerationDefinition UnitTraitPointsBoundaries must have an attribute Min");
			return;
		}
		if (!int.TryParse(xAttribute2.Value, out var result2))
		{
			TPDebug.LogError("UnitTraitGenerationDefinition UnitTraitPointsBoundaries must have a valid attribute Min (int)");
			return;
		}
		XAttribute xAttribute3 = xElement3.Attribute("Max");
		int result3;
		if (xAttribute3.IsNullOrEmpty())
		{
			TPDebug.LogError("UnitTraitGenerationDefinition UnitTraitPointsBoundaries must have an attribute Max");
		}
		else if (!int.TryParse(xAttribute3.Value, out result3))
		{
			TPDebug.LogError("UnitTraitGenerationDefinition UnitTraitPointsBoundaries must have a valid attribute Max (int)");
		}
		else
		{
			UnitTraitPointBoundaries = new Vector2Int(result2, result3);
		}
	}
}
