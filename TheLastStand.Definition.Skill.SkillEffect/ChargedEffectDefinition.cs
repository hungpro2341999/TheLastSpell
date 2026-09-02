using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Model.Status;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class ChargedEffectDefinition : StatusEffectDefinition
{
	public static class Constants
	{
		public const string Id = "Charged";
	}

	public override string Id => "Charged";

	public override Status.E_StatusType StatusType => Status.E_StatusType.Charged;

	public ChargedEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}
}
