using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Building;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Definition.Apocalypse.ApocalypseEffects;

public class ForbidBuildingCategoryAroundMagicCircleApocalypseEffectDefinition : ApocalypseEffectDefinition
{
	public BuildingDefinition.E_BuildingCategory BuildingCategory { get; private set; }

	public int RadiusRange { get; private set; }

	public ForbidBuildingCategoryAroundMagicCircleApocalypseEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("BuildingCategory");
		XAttribute xAttribute2 = obj.Attribute("RadiusRange");
		if (!Enum.TryParse<BuildingDefinition.E_BuildingCategory>(xAttribute.Value, out var result))
		{
			CLoggerManager.Log("An Apocalypse ForbidBuildingCategoryAroundMagicCircle Effect's " + HasAnInvalid("BuildingCategory", xAttribute.Value) + "!", LogType.Error);
			return;
		}
		BuildingCategory = result;
		RadiusRange = Parser.Parse(xAttribute2.Value, base.TokenVariables).EvalToInt();
	}
}
