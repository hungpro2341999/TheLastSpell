using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Skill.SkillAction.BuildLocation;

public abstract class BuildLocationDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Names
	{
		public const string AroundTheCasterName = "AroundTheCaster";

		public const string OnSelfName = "OnSelf";
	}

	public abstract string Name { get; }

	public BuildLocationDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
