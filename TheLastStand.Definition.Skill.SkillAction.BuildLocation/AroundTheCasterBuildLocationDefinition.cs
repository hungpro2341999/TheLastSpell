using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillAction.BuildLocation;

public class AroundTheCasterBuildLocationDefinition : BuildLocationDefinition
{
	public override string Name => "AroundTheCaster";

	public AroundTheCasterBuildLocationDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
