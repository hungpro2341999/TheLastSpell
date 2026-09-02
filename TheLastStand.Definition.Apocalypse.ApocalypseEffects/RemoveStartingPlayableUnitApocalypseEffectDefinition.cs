using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Apocalypse.ApocalypseEffects;

public class RemoveStartingPlayableUnitApocalypseEffectDefinition : ApocalypseEffectDefinition
{
	public int Amount { get; private set; }

	public RemoveStartingPlayableUnitApocalypseEffectDefinition(XContainer xContainer, Dictionary<string, string> tokenVariables = null)
		: base(xContainer, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container != null)
		{
			base.Deserialize(container);
			string text = (container as XElement).Attribute("Amount").Value.Replace(base.TokenVariables);
			if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
			{
				CLoggerManager.Log("RemoveStartingPlayableUnit effect " + HasAnInvalidInt(text) + ".", LogType.Error);
			}
			Amount = result;
		}
	}
}
