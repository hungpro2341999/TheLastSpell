using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;

namespace TheLastStand.Definition.Skill.SkillEffect;

public abstract class ResupplyBuildingsSkillEffectDefinition : SkillEffectDefinition
{
	public int Amount { get; private set; }

	public List<string> TargetIds { get; private set; } = new List<string>();

	public ResupplyBuildingsSkillEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XElement xElement = obj.Element("Targets");
		XElement xElement2 = obj.Element("Amount");
		if (xElement != null)
		{
			foreach (XElement item in xElement.Elements("TargetsListId"))
			{
				XAttribute xAttribute = item.Attribute("Value");
				foreach (string id in GenericDatabase.IdsListDefinitions[xAttribute.Value].Ids)
				{
					if (!TargetIds.Contains(id))
					{
						TargetIds.Add(id);
					}
				}
			}
			foreach (XElement item2 in xElement.Elements("TargetId"))
			{
				XAttribute xAttribute2 = item2.Attribute("Value");
				if (!TargetIds.Contains(xAttribute2.Value))
				{
					TargetIds.Add(xAttribute2.Value);
				}
			}
		}
		if (xElement2 != null)
		{
			if (!int.TryParse(xElement2.Value, out var result))
			{
				CLoggerManager.Log("An Amount of a SkillEffect ResupplyBuildings(ResupplyOverallUses or ResupplyCharges) isn't a valid integer !");
			}
			else
			{
				Amount = result;
			}
		}
		else
		{
			CLoggerManager.Log("A SkillEffect ResupplyBuildings(ResupplyOverallUses or ResupplyCharges) needs an Element Amount !");
		}
	}
}
