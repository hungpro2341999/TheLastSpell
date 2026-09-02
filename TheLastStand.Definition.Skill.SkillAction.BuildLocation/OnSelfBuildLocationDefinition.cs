using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillAction.BuildLocation;

public class OnSelfBuildLocationDefinition : BuildLocationDefinition
{
	public override string Name => "OnSelf";

	public OnSelfBuildLocationDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
