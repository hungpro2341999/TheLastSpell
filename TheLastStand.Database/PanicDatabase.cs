using System.Xml.Linq;
using TheLastStand.Definition.Panic;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database;

public class PanicDatabase : Database<PanicDatabase>
{
	[SerializeField]
	private TextAsset panicDefinitionTextAsset;

	public static PanicDefinition PanicDefinition { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		if (PanicDefinition == null)
		{
			PanicDefinition = new PanicDefinition(XDocument.Parse(panicDefinitionTextAsset.text, LoadOptions.SetBaseUri).Element("PanicDefinition"));
		}
	}
}
