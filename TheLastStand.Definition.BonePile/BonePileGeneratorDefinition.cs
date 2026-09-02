using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.BonePile;

public class BonePileGeneratorDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Constants
	{
		public const string BlockingBonePilesBuildingsIdsList = "BlockingBonePilesBuildings";
	}

	public struct BoneGroup
	{
		public int Tier;

		public bool Elite;

		public List<BonePileGenerationInfo> BonePileGenerationInfo;
	}

	public struct BonePileGenerationInfo
	{
		public string BuildingId;

		public int AddedPercentage;
	}

	public string ZoneId { get; private set; }

	public List<BoneGroup> BoneGroupGenerations { get; } = new List<BoneGroup>();

	public BonePileGeneratorDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public bool TryGetGroup(int tier, bool elite, out BoneGroup boneGroup)
	{
		for (int num = BoneGroupGenerations.Count - 1; num >= 0; num--)
		{
			boneGroup = BoneGroupGenerations[num];
			if (boneGroup.Tier == tier && boneGroup.Elite == elite)
			{
				return true;
			}
		}
		boneGroup = default(BoneGroup);
		return false;
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("ZoneId");
		ZoneId = xAttribute.Value;
		foreach (XElement item in obj.Elements("BoneGroup"))
		{
			DeserializeBoneGroup(item);
		}
	}

	private void DeserializeBoneGroup(XElement boneGroupElement)
	{
		XAttribute xAttribute = boneGroupElement.Attribute("Tier");
		if (!int.TryParse(xAttribute.Value, out var result))
		{
			CLoggerManager.Log("Could not parse BoneGroup Tier attribute value " + xAttribute.Value + " to a valid int!", LogType.Error);
			return;
		}
		XAttribute xAttribute2 = boneGroupElement.Attribute("Elite");
		if (!bool.TryParse(xAttribute2.Value, out var result2))
		{
			CLoggerManager.Log("Could not parse BoneGroup Elite attribute value " + xAttribute2.Value + " to a valid bool!", LogType.Error);
			return;
		}
		List<BonePileGenerationInfo> list = new List<BonePileGenerationInfo>();
		foreach (XElement item in boneGroupElement.Elements("BonePile"))
		{
			string value = item.Attribute("Id").Value;
			XAttribute xAttribute3 = item.Attribute("AddedPercentage");
			if (!int.TryParse(xAttribute3.Value, out var result3))
			{
				CLoggerManager.Log("Could not parse BonePile AddedPercentage attribute value " + xAttribute3.Value + " to a valid int!", LogType.Error);
				return;
			}
			list.Add(new BonePileGenerationInfo
			{
				BuildingId = value,
				AddedPercentage = result3
			});
		}
		BoneGroupGenerations.Add(new BoneGroup
		{
			Tier = result,
			Elite = result2,
			BonePileGenerationInfo = list
		});
	}
}
