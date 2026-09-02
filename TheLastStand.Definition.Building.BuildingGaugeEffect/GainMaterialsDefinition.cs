using System.Globalization;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingGaugeEffect;

public class GainMaterialsDefinition : BuildingGaugeEffectDefinition
{
	public const string Name = "GainMaterials";

	public Node MaterialsGain { get; private set; }

	public GainMaterialsDefinition(XContainer container)
		: base("GainMaterials", container)
	{
	}

	public override BuildingGaugeEffectDefinition Clone()
	{
		GainMaterialsDefinition obj = base.Clone() as GainMaterialsDefinition;
		obj.MaterialsGain = MaterialsGain.Clone();
		return obj;
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = (container as XElement).Element("Materials");
		if (xElement.IsNullOrEmpty())
		{
			Debug.LogError("BuildingGaugeEffectDefinition must have Materials");
			return;
		}
		float result = 1f;
		XAttribute xAttribute = xElement.Attribute("Proba");
		if (xAttribute != null && !float.TryParse(xAttribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
		{
			Debug.LogError("Materials must have a valid Proba");
		}
		else
		{
			MaterialsGain = Parser.Parse(xElement.Value);
		}
	}
}
