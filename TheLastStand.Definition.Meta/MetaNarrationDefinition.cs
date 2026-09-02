using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Meta;

public class MetaNarrationDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<string> DialogueGreetings { get; private set; }

	public List<MetaReplicaDefinition> MandatoryReplicaDefinitions { get; private set; }

	public string NameRevealDialogueId { get; private set; }

	public List<MetaReplicaDefinition> ReplicaDefinitions { get; private set; }

	public List<string> ShopGreetings { get; private set; }

	public List<MetaNarrationConditionsDefinition> VisualEvolutions { get; private set; }

	public MetaNarrationDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		DialogueGreetings = new List<string>();
		ShopGreetings = new List<string>();
		ReplicaDefinitions = new List<MetaReplicaDefinition>();
		MandatoryReplicaDefinitions = new List<MetaReplicaDefinition>();
		VisualEvolutions = new List<MetaNarrationConditionsDefinition>();
		XElement xElement2 = xElement.Element("NameRevealDialogueId");
		NameRevealDialogueId = xElement2.Value;
		foreach (XElement item in xElement.Element("DialogueGreetings").Elements("DialogueGreeting"))
		{
			DialogueGreetings.Add(item.Value);
		}
		foreach (XElement item2 in xElement.Element("ShopGreetings").Elements("ShopGreeting"))
		{
			ShopGreetings.Add(item2.Value);
		}
		foreach (XElement item3 in xElement.Element("Replicas").Elements("Replica"))
		{
			MetaReplicaDefinition metaReplicaDefinition = new MetaReplicaDefinition(item3);
			if (metaReplicaDefinition.Mandatory)
			{
				MandatoryReplicaDefinitions.Add(metaReplicaDefinition);
			}
			else
			{
				ReplicaDefinitions.Add(metaReplicaDefinition);
			}
		}
		foreach (XElement item4 in xElement.Element("VisualEvolutions").Elements("VisualEvolution"))
		{
			VisualEvolutions.Add(new MetaNarrationConditionsDefinition(item4.Element("Conditions")));
		}
	}
}
