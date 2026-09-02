using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class ComboDefinition : TheLastStand.Framework.Serialization.Definition
{
	public Dictionary<int, int> Multipliers { get; private set; } = new Dictionary<int, int>();

	public int EnemyAttacksReceivedForOnePenalty { get; private set; } = 1;

	public ComboDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in container.Elements("Multiplier"))
		{
			XAttribute xAttribute = item.Attribute("Step");
			if (xAttribute.IsNullOrEmpty())
			{
				Debug.LogError("The Multiplier must have Step!");
				continue;
			}
			if (!int.TryParse(xAttribute.Value, out var result))
			{
				Debug.LogError("The Step must be a valid integer!");
				continue;
			}
			XElement xElement = item.Element("KillsNeeded");
			int result2;
			if (xElement.IsNullOrEmpty())
			{
				Debug.LogError("The Multiplier must have KillsNeeded!");
			}
			else if (!int.TryParse(xElement.Value, out result2))
			{
				Debug.LogError("The killsNeeded must be a valid integer!");
			}
			else
			{
				Multipliers.Add(result, result2);
			}
		}
		if (!Multipliers.ContainsKey(1))
		{
			Multipliers.Add(1, 0);
		}
		Multipliers = Multipliers.OrderBy((KeyValuePair<int, int> x) => x.Key).ToDictionary((KeyValuePair<int, int> x) => x.Key, (KeyValuePair<int, int> x) => x.Value);
		XElement xElement2 = container.Element("EnemyAttacksReceivedForOnePenalty");
		int result3;
		if (xElement2.IsNullOrEmpty())
		{
			Debug.LogError("The Combo Definition must have EnemyAttacksReceivedForOnePenalty!");
		}
		else if (!int.TryParse(xElement2.Value, out result3) && result3 > 0)
		{
			Debug.LogError("The enemyAttacksReceivedForOnePenalty must be a valid integer and higher than 0!");
		}
		else
		{
			EnemyAttacksReceivedForOnePenalty = result3;
		}
	}
}
