using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Building.BuildingSlotEffect;

public class GainExperienceDefinition : BuildingSlotEffectDefinition
{
	public const string Name = "GainExperience";

	public Node Value { get; set; }

	public GainExperienceDefinition(XContainer container)
		: base("GainExperience", container)
	{
	}

	public override BuildingSlotEffectDefinition Clone()
	{
		GainExperienceDefinition obj = base.Clone() as GainExperienceDefinition;
		obj.Value = Value?.Clone();
		return obj;
	}
}
