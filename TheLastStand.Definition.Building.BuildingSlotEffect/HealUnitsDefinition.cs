using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Building.BuildingSlotEffect;

public class HealUnitsDefinition : BuildingSlotEffectDefinition
{
	public const string Name = "HealUnits";

	public Node Value { get; set; }

	public HealUnitsDefinition(XContainer container)
		: base("HealUnits", container)
	{
	}

	public override BuildingSlotEffectDefinition Clone()
	{
		HealUnitsDefinition obj = base.Clone() as HealUnitsDefinition;
		obj.Value = Value?.Clone();
		return obj;
	}
}
