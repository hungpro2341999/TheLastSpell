using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;

namespace TheLastStand.Definition.Unit;

public class UnitLinkHairSkin : Dictionary<string, List<LinkedHairDefinition>>
{
	public UnitLinkHairSkin(XElement xUnitLinkHairSkinColorDefinition)
	{
		foreach (XElement item in xUnitLinkHairSkinColorDefinition.Elements("Skin"))
		{
			XAttribute xAttribute = item.Attribute("Name");
			if (ContainsKey(xAttribute.Value))
			{
				continue;
			}
			Add(xAttribute.Value, new List<LinkedHairDefinition>());
			foreach (XElement item2 in item.Elements("LinkedHair"))
			{
				base[xAttribute.Value].Add(new LinkedHairDefinition(item2));
			}
		}
	}

	public string GetRandomHairColorId(string skinColorId)
	{
		if (base[skinColorId].Count == 0)
		{
			return string.Empty;
		}
		int num = 0;
		for (int i = 0; i < base[skinColorId].Count; i++)
		{
			num += base[skinColorId][i].Weight;
		}
		int randomRange = RandomManager.GetRandomRange(TPSingleton<PlayableUnitManager>.Instance, 0, num);
		int num2 = 0;
		for (int j = 0; j < base[skinColorId].Count; j++)
		{
			if (j == 0)
			{
				if (randomRange >= 0 && randomRange < base[skinColorId][j].Weight)
				{
					return base[skinColorId][j].Name;
				}
				num2 += base[skinColorId][j].Weight;
			}
			else
			{
				if (randomRange >= num2 && randomRange < base[skinColorId][j].Weight + num2)
				{
					return base[skinColorId][j].Name;
				}
				num2 += base[skinColorId][j].Weight;
			}
		}
		return base[skinColorId][0].Name;
	}
}
