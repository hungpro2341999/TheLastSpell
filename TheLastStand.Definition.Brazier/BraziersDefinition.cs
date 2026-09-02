using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Brazier;

public class BraziersDefinition : TheLastStand.Framework.Serialization.Definition
{
	public class GuardiansGroup
	{
		public string Id;

		public List<Tuple<string, int>> GuardiansPerWeight;
	}

	private static class Constants
	{
		public const string BrazierDefinitionElement = "BrazierDefinition";
	}

	public Dictionary<string, BrazierDefinition> BrazierDefinitions { get; private set; }

	public static Dictionary<string, GuardiansGroup> GuardiansGroups { get; private set; }

	public BraziersDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		GuardiansGroups = new Dictionary<string, GuardiansGroup>();
		foreach (XElement item2 in xElement.Element("GuardiansGroups").Elements("GuardiansGroup"))
		{
			XAttribute xAttribute = item2.Attribute("Id");
			List<Tuple<string, int>> list = new List<Tuple<string, int>>();
			foreach (XElement item3 in item2.Elements("Guardian"))
			{
				XAttribute xAttribute2 = item3.Attribute("Id");
				XAttribute xAttribute3 = item3.Attribute("Weight");
				int item = ((xAttribute3 == null) ? 1 : int.Parse(xAttribute3.Value));
				list.Add(new Tuple<string, int>(xAttribute2.Value, item));
			}
			GuardiansGroups.Add(xAttribute.Value, new GuardiansGroup
			{
				Id = xAttribute.Value,
				GuardiansPerWeight = list
			});
		}
		BrazierDefinitions = new Dictionary<string, BrazierDefinition>();
		foreach (XElement item4 in xElement.Elements("BrazierDefinition"))
		{
			BrazierDefinition brazierDefinition = new BrazierDefinition(item4);
			BrazierDefinitions.Add(brazierDefinition.Id, brazierDefinition);
		}
	}
}
