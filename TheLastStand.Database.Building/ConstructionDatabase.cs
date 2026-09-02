using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Building;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database.Building;

public class ConstructionDatabase : Database<ConstructionDatabase>
{
	[SerializeField]
	private TextAsset constructionDefinition;

	public static ConstructionDefinition ConstructionDefinition;

	public override void Deserialize(XContainer container = null)
	{
		if (ConstructionDefinition == null)
		{
			XElement xElement = XDocument.Parse(constructionDefinition.text, LoadOptions.SetBaseUri).Element("ConstructionDefinition");
			if (xElement == null)
			{
				CLoggerManager.Log("ConstructionDefinitionDocument must have a ConstructionDefinition", LogType.Error);
			}
			else
			{
				ConstructionDefinition = new ConstructionDefinition(xElement);
			}
		}
	}
}
