using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.BonePile;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database;

public class BonePileDatabase : Database<BonePileDatabase>
{
	[SerializeField]
	private TextAsset boneZoneDefinitions;

	[SerializeField]
	private TextAsset bonePileGeneratorsDefinitions;

	public static BoneZonesDefinition BoneZonesDefinition { get; private set; }

	public static BonePileGeneratorsDefinition BonePileGeneratorsDefinition { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		DeserializeBoneZoneDefinitions();
		DeserializeBonePileGeneratorsDefinitions();
	}

	private void DeserializeBoneZoneDefinitions()
	{
		if (BoneZonesDefinition == null)
		{
			XElement xElement = XDocument.Parse(boneZoneDefinitions.text, LoadOptions.SetBaseUri).Element("BoneZoneDefinitions");
			if (xElement == null)
			{
				CLoggerManager.Log("boneZoneDefinitions document must have a BoneZoneDefinitions Element", LogType.Error);
			}
			else
			{
				BoneZonesDefinition = new BoneZonesDefinition(xElement);
			}
		}
	}

	private void DeserializeBonePileGeneratorsDefinitions()
	{
		if (BonePileGeneratorsDefinition == null)
		{
			XElement xElement = XDocument.Parse(bonePileGeneratorsDefinitions.text, LoadOptions.SetBaseUri).Element("BonePileGeneratorDefinitions");
			if (xElement == null)
			{
				CLoggerManager.Log("bonePileGeneratorsDefinitions document must have a BonePileGeneratorDefinitions Element", LogType.Error);
			}
			else
			{
				BonePileGeneratorsDefinition = new BonePileGeneratorsDefinition(xElement);
			}
		}
	}
}
