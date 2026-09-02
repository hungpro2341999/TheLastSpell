using System.Xml.Linq;
using TheLastStand.Definition.Unit;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class RemoveStatusEffectDefinition : AffectingUnitSkillEffectDefinition
{
	public override string Id => RemoveStatusDefinition.Id;

	public RemoveStatusDefinition RemoveStatusDefinition { get; private set; }

	public RemoveStatusEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		RemoveStatusDefinition = new RemoveStatusDefinition(container);
	}
}
