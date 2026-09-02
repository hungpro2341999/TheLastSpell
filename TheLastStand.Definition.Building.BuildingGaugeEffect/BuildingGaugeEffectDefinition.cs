using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingGaugeEffect;

public abstract class BuildingGaugeEffectDefinition : TheLastStand.Framework.Serialization.Definition
{
	public int FirstGaugeUnits { get; private set; }

	public int GaugeUnitsIncrease { get; private set; }

	public string Id { get; private set; }

	public string ProductionBoxId { get; private set; }

	public bool TriggeredOnConstruction { get; set; }

	public BuildingGaugeEffectDefinition(string id, XContainer container)
		: base(container)
	{
		Id = id;
	}

	public virtual BuildingGaugeEffectDefinition Clone()
	{
		return MemberwiseClone() as BuildingGaugeEffectDefinition;
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XElement xElement = obj.Element("ProductionBoxId");
		if (xElement == null)
		{
			Debug.LogError("BuildingGaugeEffectDefinition must have ProductionBoxId");
		}
		XAttribute xAttribute = xElement.Attribute("Value");
		if (xAttribute.IsNullOrEmpty())
		{
			Debug.LogError("ProductionBoxId must have Value");
		}
		ProductionBoxId = xAttribute.Value;
		XElement xElement2 = obj.Element("FirstGauge");
		if (xElement2 != null)
		{
			FirstGaugeUnits = int.Parse(xElement2.Value);
		}
		XElement xElement3 = obj.Element("GaugeIncrease");
		if (xElement3 != null)
		{
			GaugeUnitsIncrease = int.Parse(xElement3.Value);
		}
	}
}
