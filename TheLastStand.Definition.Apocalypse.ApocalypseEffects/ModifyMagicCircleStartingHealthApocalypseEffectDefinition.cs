using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Apocalypse.ApocalypseEffects;

public class ModifyMagicCircleStartingHealthApocalypseEffectDefinition : ApocalypseEffectDefinition
{
	public float HealthMultiplier { get; private set; }

	public ModifyMagicCircleStartingHealthApocalypseEffectDefinition(XContainer xContainer, Dictionary<string, string> tokenVariables = null)
		: base(xContainer, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container != null)
		{
			base.Deserialize(container);
			string text = (container as XElement).Attribute("HealthMultiplier").Value.Replace(base.TokenVariables);
			if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				CLoggerManager.Log("ModifyMagicCircleStartingHealth effect " + HasAnInvalidFloat(text) + ".", LogType.Error);
			}
			HealthMultiplier = result;
		}
	}
}
