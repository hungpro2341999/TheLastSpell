using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class TraitsParametersMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "TraitsParameters";

	public int StartTraitTotalPointsModifier;

	public Vector2Int UnitTraitPointsBoundariesModifiers = Vector2Int.zero;

	public TraitsParametersMetaEffectDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container == null)
		{
			return;
		}
		XElement obj = container as XElement;
		XElement xElement = obj.Element("StartTraitTotalPointsModifier");
		if (xElement != null && int.TryParse(xElement.Value, out var result))
		{
			StartTraitTotalPointsModifier = result;
		}
		XElement xElement2 = obj.Element("UnitTraitPointsBoundariesModifiers");
		if (xElement2 != null)
		{
			XElement xElement3 = xElement2.Element("Min");
			XElement xElement4 = xElement2.Element("Max");
			if (xElement3 != null && int.TryParse(xElement3.Value, out var result2))
			{
				UnitTraitPointsBoundariesModifiers.x = result2;
			}
			if (xElement4 != null && int.TryParse(xElement4.Value, out var result3))
			{
				UnitTraitPointsBoundariesModifiers.y = result3;
			}
		}
	}
}
