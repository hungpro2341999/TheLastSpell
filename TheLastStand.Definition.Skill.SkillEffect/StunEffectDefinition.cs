using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Model.Status;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class StunEffectDefinition : StatusEffectDefinition
{
	public static class Constants
	{
		public const string Id = "Stun";
	}

	public override string Id => "Stun";

	public override Status.E_StatusType StatusType => Status.E_StatusType.Stun;

	public StunEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		AffectedUnits = E_SkillUnitAffect.IgnoreCaster;
		base.Deserialize(container);
	}
}
