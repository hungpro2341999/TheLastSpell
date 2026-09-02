using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Skill.SkillEffect;

namespace TheLastStand.Definition.Skill.SkillAction;

public class GenericSkillActionDefinition : SkillActionDefinition
{
	public const string Name = "Generic";

	public bool CasterEffectOnly { get; private set; }

	public GenericSkillActionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		CasterEffectOnly = HasEffect("CasterEffect");
		if (!CasterEffectOnly)
		{
			return;
		}
		foreach (KeyValuePair<string, List<SkillEffectDefinition>> skillEffectDefinition in base.SkillEffectDefinitions)
		{
			if (skillEffectDefinition.Key != "CasterEffect")
			{
				CasterEffectOnly = false;
				break;
			}
		}
	}
}
