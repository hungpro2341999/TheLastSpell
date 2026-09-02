using System.Xml.Linq;
using TPLib.Log;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class ResupplySkillsSkillEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "ResupplySkills";
	}

	public int Amount { get; private set; }

	public override string Id => "ResupplySkills";

	public ResupplySkillsSkillEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = (container as XElement).Element("Amount");
		if (xElement != null)
		{
			if (!int.TryParse(xElement.Value, out var result))
			{
				CLoggerManager.Log("An Amount of a SkillEffect ResupplySkills isn't a valid integer !");
			}
			else
			{
				Amount = result;
			}
		}
		else
		{
			CLoggerManager.Log("A SkillEffect ResupplySkills needs an Element Amount !");
		}
	}
}
