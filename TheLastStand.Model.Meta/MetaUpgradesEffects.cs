using System;
using System.Collections.Generic;
using TheLastStand.Controller.Meta;
using TheLastStand.Definition.Meta;

namespace TheLastStand.Model.Meta;

public class MetaUpgradesEffects
{
	public MetaUpgradeEffectsController MetaUpgradeEffectsController { get; private set; }

	public Dictionary<Type, List<MetaEffectDefinition>> UpgradeEffectsByType { get; private set; }

	public MetaUpgradesEffects(MetaUpgradeEffectsController controller)
	{
		MetaUpgradeEffectsController = controller;
		UpgradeEffectsByType = new Dictionary<Type, List<MetaEffectDefinition>>();
	}
}
