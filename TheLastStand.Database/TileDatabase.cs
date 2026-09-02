using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Database;

public class TileDatabase : Database<TileDatabase>, ILegacyDeserializable
{
	[SerializeField]
	private TextAsset groundDefinitionsTextAsset;

	public static Dictionary<string, GroundDefinition> GroundDefinitions { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		if (GroundDefinitions != null)
		{
			return;
		}
		GroundDefinitions = new Dictionary<string, GroundDefinition>();
		foreach (XElement item in XDocument.Parse(groundDefinitionsTextAsset.text, LoadOptions.SetBaseUri).Element("GroundDefinitions").Elements("GroundDefinition"))
		{
			GroundDefinition groundDefinition = new GroundDefinition(item);
			GroundDefinitions.Add(groundDefinition.Id, groundDefinition);
		}
	}
}
