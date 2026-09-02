using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingPassive;

public class GainResourcesDefinition : BuildingPassiveEffectDefinition
{
	private int gainDamnedSouls;

	private int gainGold;

	private int gainMaterials;

	public int GainGold
	{
		get
		{
			int num = 0;
			if (TPSingleton<GlyphManager>.Exist())
			{
				num += TPSingleton<GlyphManager>.Instance.GoldScavengingPercentageModifier;
			}
			float num2 = 1f + (float)num / 100f;
			return (int)((float)gainGold * num2);
		}
	}

	public int GainDamnedSouls
	{
		get
		{
			uint num = TPSingleton<ApocalypseManager>.Instance.DamnedSoulsPercentageModifier;
			if (TPSingleton<GlyphManager>.Exist())
			{
				num += (uint)TPSingleton<GlyphManager>.Instance.DamnedSoulsScavengingPercentageModifier;
				num += TPSingleton<GlyphManager>.Instance.DamnedSoulsPercentageModifier;
			}
			float num2 = 1f + (float)num / 100f;
			return (int)((float)gainDamnedSouls * num2);
		}
	}

	public int GainMaterials
	{
		get
		{
			int num = 0;
			if (TPSingleton<GlyphManager>.Exist())
			{
				num += TPSingleton<GlyphManager>.Instance.MaterialScavengingPercentageModifier;
			}
			float num2 = 1f + (float)num / 100f;
			return (int)((float)gainMaterials * num2);
		}
	}

	public GainResourcesDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		XElement xElement = xContainer as XElement;
		XAttribute xAttribute = xElement.Attribute("Gold");
		if (!string.IsNullOrEmpty(xAttribute?.Value))
		{
			if (!int.TryParse(xAttribute.Value, out var result))
			{
				CLoggerManager.Log("A GainResources BuildingPassiveEffect has an incorrect gold value", LogType.Error, CLogLevel.MAJOR);
				return;
			}
			gainGold = result;
		}
		XAttribute xAttribute2 = xElement.Attribute("Materials");
		if (!string.IsNullOrEmpty(xAttribute2?.Value))
		{
			if (!int.TryParse(xAttribute2.Value, out var result2))
			{
				CLoggerManager.Log("A GainResources BuildingPassiveEffect has an incorrect Materials value", LogType.Error, CLogLevel.MAJOR);
				return;
			}
			gainMaterials = result2;
		}
		XAttribute xAttribute3 = xElement.Attribute("DamnedSouls");
		if (!string.IsNullOrEmpty(xAttribute3?.Value))
		{
			if (!int.TryParse(xAttribute3.Value, out var result3))
			{
				CLoggerManager.Log("A GainResources BuildingPassiveEffect has an incorrect DamnedSouls value", LogType.Error, CLogLevel.MAJOR);
			}
			else
			{
				gainDamnedSouls = result3;
			}
		}
	}
}
