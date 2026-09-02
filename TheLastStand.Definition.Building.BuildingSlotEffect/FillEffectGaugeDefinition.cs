using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Building.BuildingSlotEffect;

public class FillEffectGaugeDefinition : BuildingSlotEffectDefinition
{
	public const string Name = "FillEffectGauge";

	public Node Value { get; set; }

	public FillEffectGaugeDefinition(XContainer container)
		: base("FillEffectGauge", container)
	{
	}
}
