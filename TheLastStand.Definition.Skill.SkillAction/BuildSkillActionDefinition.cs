using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Definition.Skill.SkillAction.BuildLocation;
using UnityEngine;

namespace TheLastStand.Definition.Skill.SkillAction;

public class BuildSkillActionDefinition : SkillActionDefinition
{
	public const string Name = "Build";

	public Dictionary<string, int> Buildings { get; set; } = new Dictionary<string, int>();

	public float Delay { get; private set; }

	public BuildLocationDefinition BuildLocationDefinition { get; private set; }

	public BuildSkillActionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container.Element("Build");
		XAttribute xAttribute = xElement.Attribute("Delay");
		XElement xElement2 = xElement.Element("Location");
		XElement xElement3 = xElement.Element("Buildings");
		if (!float.TryParse(xAttribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			TPDebug.LogError("Delay of skill build action should be of type float !");
		}
		Delay = result;
		foreach (XElement item in xElement2.Elements())
		{
			switch (item.Name.LocalName)
			{
			case "AroundTheCaster":
				BuildLocationDefinition = new AroundTheCasterBuildLocationDefinition(item);
				break;
			case "OnSelf":
				BuildLocationDefinition = new OnSelfBuildLocationDefinition(item);
				break;
			}
		}
		foreach (XElement item2 in xElement3.Elements("Building"))
		{
			XAttribute xAttribute2 = item2.Attribute("Id");
			if (!int.TryParse(item2.Attribute("Weight").Value, out var result2))
			{
				Debug.LogError("Building " + xAttribute2.Value + " in a building skill action have an invalid Weight !");
			}
			if (Buildings.ContainsKey(xAttribute2.Value))
			{
				Buildings[xAttribute2.Value] += result2;
			}
			else
			{
				Buildings.Add(xAttribute2.Value, result2);
			}
		}
	}
}
