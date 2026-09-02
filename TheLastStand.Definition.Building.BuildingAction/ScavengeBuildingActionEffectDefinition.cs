using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Definition.Item;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingAction;

public class ScavengeBuildingActionEffectDefinition : BuildingActionEffectDefinition
{
	private int gainDamnedSouls;

	private int gainGold;

	private int gainMaterials;

	public List<CreateItemDefinition> CreateItemDefinitions { get; } = new List<CreateItemDefinition>();

	public int Damage { get; private set; }

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

	public ScavengeBuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
		: base(xContainer, buildingActionDefinitionContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		XElement xElement = xContainer as XElement;
		XElement xElement2 = xElement.Element("GainGold");
		if (!xElement2.IsNullOrEmpty())
		{
			if (!int.TryParse(xElement2.Value, out var result))
			{
				Debug.LogError("A ScavengeGold Building ActionEffect must have a valid GainGold (int)");
				return;
			}
			gainGold = result;
		}
		XElement xElement3 = xElement.Element("GainMaterials");
		if (!xElement3.IsNullOrEmpty())
		{
			if (!int.TryParse(xElement3.Value, out var result2))
			{
				Debug.LogError("A ScavengeMaterials Building ActionEffect must have a valid GainMaterials (int)");
				return;
			}
			gainMaterials = result2;
		}
		XElement xElement4 = xElement.Element("GainDamnedSouls");
		if (!xElement4.IsNullOrEmpty())
		{
			if (!int.TryParse(xElement4.Value, out var result3))
			{
				Debug.LogError("A ScavengeDamnedSouls Building ActionEffect must have a valid GainDamnedSouls (int)");
				return;
			}
			gainDamnedSouls = result3;
		}
		foreach (XElement item in xElement.Elements("CreateItem"))
		{
			CreateItemDefinitions.Add(new CreateItemDefinition(item));
		}
		XElement xElement5 = xElement.Element("Damage");
		int result4;
		if (xElement5.IsNullOrEmpty())
		{
			Debug.LogError("A ScavengeGold Building ActionEffect must have a Damage element");
		}
		else if (!int.TryParse(xElement5.Value, out result4))
		{
			Debug.LogError("A ScavengeGold Building ActionEffect must have a valid Damage (int)");
		}
		else
		{
			Damage = result4;
		}
	}
}
