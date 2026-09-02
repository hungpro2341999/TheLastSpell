using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss;

public class BossPhasesDefinition : TheLastStand.Framework.Serialization.Definition
{
	public Dictionary<string, BossPhaseDefinition> BossPhaseDefinitions { get; } = new Dictionary<string, BossPhaseDefinition>();

	public string Id { get; private set; }

	public BossPhasesDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute.IsNullOrEmpty())
		{
			Debug.LogError("BossPhasesDefinition has no Id!");
			return;
		}
		Id = xAttribute.Value;
		foreach (XElement item in xElement.Elements("Phase"))
		{
			XAttribute xAttribute2 = item.Attribute("Id");
			BossPhaseDefinitions[xAttribute2.Value] = new BossPhaseDefinition(item, xAttribute2.Value);
		}
	}
}
