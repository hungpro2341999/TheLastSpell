using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.Definition.Panic;

public class PanicDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<int> GoldValues { get; private set; }

	public List<int> MaterialValues { get; private set; }

	public float PanicAttackMultiplier { get; private set; }

	public PanicLevelDefinition[] PanicLevelDefinitions { get; private set; }

	public float ValueMax { get; private set; }

	public PanicDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Element("ValueMax").Attribute("Value");
		if (!float.TryParse(xAttribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			Debug.LogError("Invalid ValueMax " + xAttribute.Value);
		}
		ValueMax = result;
		XAttribute xAttribute2 = xElement.Element("PanicAttackMultiplier").Attribute("Value");
		if (!float.TryParse(xAttribute2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
		{
			Debug.LogError("Invalid PanicAttackMultiplier " + xAttribute2.Value);
		}
		PanicAttackMultiplier = result2;
		XElement xElement2 = xElement.Element("RewardValues");
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		int i = 1;
		foreach (XElement item in xElement2.Elements("RewardValue"))
		{
			if (!int.TryParse(item.Attribute("Index").Value, out var result3))
			{
				CLoggerManager.Log("Could not cast the index into an int !", TPSingleton<PanicManager>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PanicManager");
			}
			if (!int.TryParse(item.Element("Gold").Value, out var result4))
			{
				CLoggerManager.Log("Could not cast the gold value into an int !", LogType.Error);
			}
			if (!int.TryParse(item.Element("Material").Value, out var result5))
			{
				CLoggerManager.Log("Could not cast the material value into an int !", LogType.Error);
			}
			if (i == 1)
			{
				if (result3 != 1)
				{
					CLoggerManager.Log("The RewardValue for the first day (Index=\"1\") is required and must be placed first !", TPSingleton<PanicManager>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PanicManager");
					list.Add(0);
					list2.Add(0);
				}
				else
				{
					list.Add(result4);
					list2.Add(result5);
				}
				i++;
			}
			else if (result3 < i)
			{
				CLoggerManager.Log("The order of the RewardValues isn't respected, it might lead to errors !", LogType.Error);
			}
			else
			{
				for (; i < result3; i++)
				{
					CLoggerManager.Log("Some indexes are missing in the RewardValues, it might be unintended !", LogType.Warning);
					list.Add(list[list.Count - 1]);
					list2.Add(list2[list2.Count - 1]);
				}
				list.Add(result4);
				list2.Add(result5);
				i++;
			}
		}
		GoldValues = list;
		MaterialValues = list2;
		XElement xElement3 = xElement.Element("Levels");
		IEnumerable<XElement> source = xElement3.Elements("Level");
		PanicLevelDefinitions = new PanicLevelDefinition[source.Count()];
		int num = 0;
		foreach (XElement item2 in xElement3.Elements("Level"))
		{
			PanicLevelDefinitions[num++] = new PanicLevelDefinition(item2);
		}
	}
}
