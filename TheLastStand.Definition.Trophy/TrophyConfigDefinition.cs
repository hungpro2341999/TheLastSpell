using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Trophy;

public class TrophyConfigDefinition : TheLastStand.Framework.Serialization.Definition
{
	public enum E_GemRarity
	{
		Common,
		Uncommon,
		Rare,
		Epic
	}

	public class GemStageData
	{
		public E_GemRarity GemRarity { get; private set; }

		public int Min { get; private set; }

		public int Max { get; private set; }

		public GemStageData(E_GemRarity gemRarity, int min, int max)
		{
			GemRarity = gemRarity;
			Min = min;
			Max = max;
		}
	}

	public List<GemStageData> GemStageDatas { get; private set; } = new List<GemStageData>();

	public TrophyConfigDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = (container as XElement).Element("GemsStages");
		if (xElement == null)
		{
			return;
		}
		foreach (XElement item in xElement.Elements("Gem"))
		{
			XAttribute xAttribute = item.Attribute("Id");
			XAttribute xAttribute2 = item.Attribute("Min");
			XAttribute xAttribute3 = item.Attribute("Max");
			if (xAttribute == null)
			{
				continue;
			}
			if (!Enum.TryParse<E_GemRarity>(xAttribute.Value, out var result))
			{
				TPDebug.LogError("A Gem stage has an invalid Id value : " + xAttribute.Value);
			}
			if (xAttribute2 == null)
			{
				continue;
			}
			if (!int.TryParse(xAttribute2.Value, out var result2))
			{
				TPDebug.LogError("A Gem stage has an invalid min value (should be an integer) : " + xAttribute2.Value);
			}
			if (xAttribute3 != null)
			{
				if (!int.TryParse(xAttribute3.Value, out var result3))
				{
					result3 = -1;
					TPDebug.LogError("A Gem stage has an invalid max value (should be an integer) : " + xAttribute3.Value);
				}
				GemStageDatas.Add(new GemStageData(result, result2, (result3 != -1) ? result3 : int.MaxValue));
			}
		}
	}
}
