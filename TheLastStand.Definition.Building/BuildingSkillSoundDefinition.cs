using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Building;

public class BuildingSkillSoundDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<string> SoundsIds = new List<string>();

	public Node Delay { get; private set; }

	public string BuildingTemplateDefinitionId { get; private set; }

	public int MaximumSimultaneousSounds { get; private set; }

	public BuildingSkillSoundDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XElement xElement = obj.Element("BuildingTemplateDefinitionId");
		XElement xElement2 = obj.Element("MaximumSimultaneousSounds");
		XElement xElement3 = obj.Element("Sounds");
		if (xElement != null && xElement.Attribute("Value") != null)
		{
			BuildingTemplateDefinitionId = xElement.Attribute("Value").Value;
		}
		if (xElement2 != null)
		{
			if (!int.TryParse(xElement2.Value, out var result))
			{
				CLoggerManager.Log("MaximumSimultaneousSounds should be of type int !");
				return;
			}
			MaximumSimultaneousSounds = result;
		}
		if (xElement3 == null)
		{
			return;
		}
		foreach (XElement item in xElement3.Elements("SoundId"))
		{
			if (item.Attribute("Value") != null)
			{
				SoundsIds.Add(item.Attribute("Value").Value);
			}
		}
		XAttribute xAttribute = xElement3.Attribute("Delay");
		Delay = ((xAttribute != null) ? Parser.Parse(xAttribute.Value) : new NodeNumber(0.0));
	}
}
