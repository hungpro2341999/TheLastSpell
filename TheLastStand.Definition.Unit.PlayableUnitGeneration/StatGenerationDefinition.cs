using System;
using System.Globalization;
using System.Xml.Linq;
using TheLastStand.Controller.Meta;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Unit.PlayableUnitGeneration;

public class StatGenerationDefinition : TheLastStand.Framework.Serialization.Definition
{
	private string Archetype { get; set; }

	private Vector2 Boundaries { get; set; }

	public UnitStatDefinition.E_Stat Stat { get; private set; }

	public StatGenerationDefinition(XContainer container, string archetype)
		: base(container)
	{
		Archetype = archetype;
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		Stat = (UnitStatDefinition.E_Stat)Enum.Parse(value: obj.Attribute("Stat").Value, enumType: typeof(UnitStatDefinition.E_Stat));
		float x = float.Parse(obj.Element("Min").Value, NumberStyles.Float, CultureInfo.InvariantCulture);
		float y = float.Parse(obj.Element("Max").Value, NumberStyles.Float, CultureInfo.InvariantCulture);
		Boundaries = new Vector2(x, y);
	}

	public Vector2 GetBoundaries()
	{
		Vector2 boundaries = Boundaries;
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnitAttributeModifierMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
		{
			for (int num = effects.Length - 1; num >= 0; num--)
			{
				UnitAttributeModifierMetaEffectDefinition unitAttributeModifierMetaEffectDefinition = effects[num];
				if ((unitAttributeModifierMetaEffectDefinition.Archetype == Archetype || unitAttributeModifierMetaEffectDefinition.AllArchetypes) && unitAttributeModifierMetaEffectDefinition.StatAndValue.TryGetValue(Stat, out var value))
				{
					boundaries += value;
				}
			}
		}
		return boundaries;
	}
}
