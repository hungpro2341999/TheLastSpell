using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Controller.Meta;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Building;

public class BuildingLimitGroupDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<string> BuildingIds = new List<string>();

	private int nativeLimit = -1;

	public string Id { get; set; }

	public BuildingLimitGroupDefinition(XContainer container)
		: base(container)
	{
	}

	public int GetBuildLimit(bool useDefault = false)
	{
		if (useDefault)
		{
			return nativeLimit;
		}
		int num = 0;
		if (nativeLimit != -1)
		{
			if (MetaUpgradeEffectsController.TryGetEffectsOfType<BuildingModifierMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
			{
				for (int num2 = effects.Length - 1; num2 >= 0; num2--)
				{
					if (effects[num2].BuildingId == Id && effects[num2].MaxCityInstancesBonus != -1)
					{
						num += effects[num2].MaxCityInstancesBonus;
					}
				}
			}
			num += TPSingleton<GlyphManager>.Instance.BuildLimitModifiers.GetValueOrDefault(Id);
		}
		return nativeLimit + num;
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		Id = xElement.Attribute("Id").Value;
		XAttribute xAttribute = xElement.Attribute("Limit");
		if (xAttribute.IsNullOrEmpty())
		{
			return;
		}
		if (sbyte.TryParse(xAttribute.Value, out var result))
		{
			if (result == 0)
			{
				Debug.LogError("BuildingLimitGroupDefinition " + Id + " has an invalid Limit (must be in the range -1 to 127, excluding 0)!");
			}
			else
			{
				nativeLimit = result;
			}
		}
		else
		{
			Debug.LogError("BuildingLimitGroupDefinition " + Id + " has an invalid Limit (must be in the range -1 to 127, excluding 0)!");
		}
	}
}
