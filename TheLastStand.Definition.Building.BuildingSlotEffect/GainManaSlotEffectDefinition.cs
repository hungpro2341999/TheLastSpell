using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Building.BuildingSlotEffect;

public class GainManaSlotEffectDefinition : BuildingSlotEffectDefinition
{
	public const string Name = "GainMana";

	public Node Value { get; set; }

	public GainManaSlotEffectDefinition(XContainer container)
		: base("GainMana", container)
	{
	}

	public override BuildingSlotEffectDefinition Clone()
	{
		GainManaSlotEffectDefinition obj = base.Clone() as GainManaSlotEffectDefinition;
		obj.Value = Value?.Clone();
		return obj;
	}
}
