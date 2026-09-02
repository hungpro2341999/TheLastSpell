using System;
using System.Collections.Generic;
using System.Linq;
using TheLastStand.Definition.Meta;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Meta;

namespace TheLastStand.Controller.Meta;

public class MetaUpgradeEffectsController
{
	public MetaUpgradesEffects UpgradesEffects { get; }

	public MetaUpgradeEffectsController()
	{
		UpgradesEffects = new MetaUpgradesEffects(this);
	}

	public static bool TryGetEffectsOfType<T>(out T[] effects, MetaUpgradesManager.E_MetaState state) where T : MetaEffectDefinition
	{
		effects = null;
		if (MetaUpgradesManager.ActivatedUpgradesEffects == null || MetaUpgradesManager.FulfilledUpgradesEffects == null || MetaUpgradesManager.UnlockedUpgradesEffects == null || MetaUpgradesManager.LockedUpgradesEffects == null)
		{
			return false;
		}
		bool result = false;
		switch (state)
		{
		case MetaUpgradesManager.E_MetaState.Activated:
			result = DoEffectsByTypeContainsKey<T>(MetaUpgradesManager.ActivatedUpgradesEffects.UpgradeEffectsByType, typeof(T), out effects);
			break;
		case MetaUpgradesManager.E_MetaState.Unlocked:
		{
			result = DoEffectsByTypeContainsKey<T>(MetaUpgradesManager.UnlockedUpgradesEffects.UpgradeEffectsByType, typeof(T), out effects);
			if (!DoEffectsByTypeContainsKey<T>(MetaUpgradesManager.FulfilledUpgradesEffects.UpgradeEffectsByType, typeof(T), out var effects2))
			{
				break;
			}
			result = true;
			if (effects == null)
			{
				effects = effects2;
				break;
			}
			T[] array = new T[effects.Length + effects2.Length];
			for (int i = 0; i < effects.Length; i++)
			{
				array[i] = effects[i];
			}
			for (int j = 0; j < effects2.Length; j++)
			{
				array[j + effects.Length] = effects2[j];
			}
			effects = array;
			break;
		}
		case MetaUpgradesManager.E_MetaState.Locked:
			result = DoEffectsByTypeContainsKey<T>(MetaUpgradesManager.LockedUpgradesEffects.UpgradeEffectsByType, typeof(T), out effects);
			break;
		}
		return result;
	}

	private static bool DoEffectsByTypeContainsKey<T>(Dictionary<Type, List<MetaEffectDefinition>> upgradeEffectsByType, Type type, out T[] effects)
	{
		if (upgradeEffectsByType.ContainsKey(type))
		{
			effects = upgradeEffectsByType[type].Cast<T>().ToArray();
			return true;
		}
		effects = null;
		return false;
	}

	public void AddUpgradeEffects(MetaUpgrade upgrade)
	{
		AddUpgradeEffects(upgrade.MetaUpgradeDefinition);
	}

	public void AddUpgradeEffects(MetaUpgradeDefinition upgradeDefinition)
	{
		for (int num = upgradeDefinition.UpgradeEffectDefinitions.Count - 1; num >= 0; num--)
		{
			MetaEffectDefinition metaEffectDefinition = upgradeDefinition.UpgradeEffectDefinitions[num];
			Type type = metaEffectDefinition.GetType();
			if (!UpgradesEffects.UpgradeEffectsByType.ContainsKey(type))
			{
				UpgradesEffects.UpgradeEffectsByType.Add(type, new List<MetaEffectDefinition>());
			}
			UpgradesEffects.UpgradeEffectsByType[type].Add(metaEffectDefinition);
			metaEffectDefinition.OnMetaEffectActivated(hasBeenActivated: true);
		}
	}

	public void RemoveUpgradesEffects(MetaUpgrade upgrade)
	{
		RemoveUpgradesEffects(upgrade.MetaUpgradeDefinition);
	}

	public void RemoveUpgradesEffects(MetaUpgradeDefinition upgradeDefinition)
	{
		for (int num = upgradeDefinition.UpgradeEffectDefinitions.Count - 1; num >= 0; num--)
		{
			MetaEffectDefinition metaEffectDefinition = upgradeDefinition.UpgradeEffectDefinitions[num];
			Type type = metaEffectDefinition.GetType();
			if (UpgradesEffects.UpgradeEffectsByType.ContainsKey(type))
			{
				UpgradesEffects.UpgradeEffectsByType[type].Remove(metaEffectDefinition);
				metaEffectDefinition.OnMetaEffectActivated(hasBeenActivated: false);
			}
		}
	}
}
