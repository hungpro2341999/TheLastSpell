using System.Xml.Linq;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database;

public class MetaNarrationDatabase : Database<MetaNarrationDatabase>
{
	[SerializeField]
	private TextAsset narrationDefinition;

	public static MetaNarrationDefinition DarkGoddessNarrationDefinition { get; private set; }

	public static MetaNarrationDefinition LightGoddessNarrationDefinition { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		XElement xElement = XDocument.Parse(narrationDefinition.text, LoadOptions.SetBaseUri).Element("MetaNarrationDefinition");
		DarkGoddessNarrationDefinition = new MetaNarrationDefinition(xElement.Element("Dark"));
		LightGoddessNarrationDefinition = new MetaNarrationDefinition(xElement.Element("Light"));
	}
}
