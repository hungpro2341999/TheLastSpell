using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class MetaReplicaDefinition : TheLastStand.Framework.Serialization.Definition
{
	public int AnswersCount { get; private set; }

	public List<string> BlockReplicas { get; private set; }

	public MetaNarrationConditionsDefinition ConditionsDefinition { get; private set; }

	public string Id { get; private set; }

	public bool Mandatory { get; private set; }

	public MetaReplicaDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		Id = xAttribute.Value;
		XAttribute xAttribute2 = xElement.Attribute("AnswersCount");
		if (xAttribute2 != null)
		{
			if (!int.TryParse(xAttribute2.Value, out var result))
			{
				CLoggerManager.Log("Could not parse " + xAttribute2.Value + " to a valid int value for replica \"" + Id + "\".", LogType.Error);
				return;
			}
			AnswersCount = result;
		}
		else
		{
			AnswersCount = 1;
		}
		XElement xElement2 = xElement.Element("BlockReplicas");
		if (xElement2 != null)
		{
			BlockReplicas = new List<string>();
			foreach (XElement item in xElement2.Elements("ReplicaId"))
			{
				BlockReplicas.Add(item.Value);
			}
		}
		XAttribute xAttribute3 = xElement.Attribute("Mandatory");
		if (xAttribute3 != null)
		{
			if (!bool.TryParse(xAttribute3.Value, out var result2))
			{
				CLoggerManager.Log("Could not parse Mandatory Attribute " + xAttribute3.Value + " to a valid boolean value for replica \"" + Id + "\".", LogType.Error);
				return;
			}
			Mandatory = result2;
		}
		XElement xElement3 = xElement.Element("Conditions");
		if (xElement3 != null)
		{
			ConditionsDefinition = new MetaNarrationConditionsDefinition(xElement3);
		}
	}
}
