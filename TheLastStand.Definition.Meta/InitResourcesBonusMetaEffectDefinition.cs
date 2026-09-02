using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class InitResourcesBonusMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "InitResourcesBonus";

	public int GoldBonus { get; private set; }

	public int MaterialsBonus { get; private set; }

	public InitResourcesBonusMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XElement xElement = obj.Element("GoldBonus");
		if (xElement != null)
		{
			if (int.TryParse(xElement.Value, out var result))
			{
				GoldBonus = result;
			}
			else
			{
				Debug.LogError("GoldBonus element as an invalid value!");
			}
		}
		XElement xElement2 = obj.Element("MaterialsBonus");
		if (xElement2 != null)
		{
			if (int.TryParse(xElement2.Value, out var result2))
			{
				MaterialsBonus = result2;
			}
			else
			{
				Debug.LogError("MaterialsBonus element as an invalid value!");
			}
		}
	}

	public override string ToString()
	{
		return string.Format("{0} ({1} gold / {2} materials)", "InitResourcesBonus", GoldBonus, MaterialsBonus);
	}
}
