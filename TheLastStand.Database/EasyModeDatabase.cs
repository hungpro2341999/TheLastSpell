using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.EasyMode;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Database;

public class EasyModeDatabase : Database<EasyModeDatabase>
{
	[SerializeField]
	private TextAsset easyModeDefinitionTextAsset;

	public static EasyModeDefinition EasyModeDefinition { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		XElement xElement = XDocument.Parse(easyModeDefinitionTextAsset.text, LoadOptions.SetBaseUri).Element("EasyModeDefinition");
		if (xElement.IsNullOrEmpty())
		{
			CLoggerManager.Log("The document " + easyModeDefinitionTextAsset.name + " must have an EasyModeDefinition!", LogType.Error);
		}
		else
		{
			EasyModeDefinition = new EasyModeDefinition(xElement);
		}
	}
}
