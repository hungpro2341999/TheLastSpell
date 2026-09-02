using System.Collections.Generic;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Localization;
using TheLastStand.Controller.Meta;
using TheLastStand.Database;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.DLC;
using TheLastStand.Manager.Meta;
using TheLastStand.Serialization.Meta;
using TheLastStand.View.MetaShops;

namespace TheLastStand.Model.Meta;

public class MetaUpgrade : ISerializable, IDeserializable
{
	public class StringToMetaUpgradeIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(MetaDatabase.MetaUpgradesDefinitions.Keys);
	}

	public uint InvestedSouls;

	public string Name => Localizer.Get("Meta_UpgradeName_" + MetaUpgradeDefinition.Id);

	public string Description => Localizer.Get("Meta_UpgradeDescription_" + MetaUpgradeDefinition.Id);

	public bool IsAvailableWithCurrentDLCs
	{
		get
		{
			if (MetaUpgradeDefinition.IsLinkedToDLC)
			{
				return IsLinkedDLCOwned;
			}
			return true;
		}
	}

	public bool IsLinkedDLCOwned
	{
		get
		{
			if (MetaUpgradeDefinition.IsLinkedToDLC)
			{
				return TPSingleton<DLCManager>.Instance.IsDLCOwned(MetaUpgradeDefinition.DLCId);
			}
			return false;
		}
	}

	public MetaUpgradeController MetaUpgradeController { get; }

	public MetaUpgradeDefinition MetaUpgradeDefinition { get; }

	public uint SoulsLeftToFulfill => MetaUpgradeDefinition.Price - InvestedSouls;

	public MetaUpgrade(MetaUpgradeDefinition definition, MetaUpgradeController controller)
	{
		MetaUpgradeDefinition = definition;
		MetaUpgradeController = controller;
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
	}

	public List<List<MetaConditionController>> GetConditions(MetaCondition.E_MetaConditionCategory category)
	{
		return MetaConditionManager.SplitConditionsByGroupIndex(TPSingleton<MetaConditionManager>.Instance.GetControllers(this, category));
	}

	public bool IsValidated(MetaUpgradeDefinition.E_MetaUpgradeCategory category, MetaUpgradeDefinition.E_MetaUpgradeFilter filter, Dictionary<MetaUpgrade, MetaUpgradeLineView> newMetaUpgrades)
	{
		if (IsValidatedByCategory(category))
		{
			return IsValidatedByFilter(filter, newMetaUpgrades);
		}
		return false;
	}

	public bool IsValidatedByCategory(MetaUpgradeDefinition.E_MetaUpgradeCategory category)
	{
		return (MetaUpgradeDefinition.Category & category) != 0;
	}

	public bool IsValidatedByFilter(MetaUpgradeDefinition.E_MetaUpgradeFilter filter, Dictionary<MetaUpgrade, MetaUpgradeLineView> newMetaUpgrades)
	{
		if ((MetaUpgradeDefinition.E_MetaUpgradeFilter.Acquired & filter) != MetaUpgradeDefinition.E_MetaUpgradeFilter.None && TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades.Contains(this))
		{
			return true;
		}
		if ((MetaUpgradeDefinition.E_MetaUpgradeFilter.Locked & filter) != MetaUpgradeDefinition.E_MetaUpgradeFilter.None && TPSingleton<MetaUpgradesManager>.Instance.LockedUpgrades.Contains(this))
		{
			return true;
		}
		if ((MetaUpgradeDefinition.E_MetaUpgradeFilter.New & filter) != MetaUpgradeDefinition.E_MetaUpgradeFilter.None && newMetaUpgrades.ContainsKey(this))
		{
			return true;
		}
		if ((MetaUpgradeDefinition.E_MetaUpgradeFilter.NotAcquiredYet & filter) != MetaUpgradeDefinition.E_MetaUpgradeFilter.None && (TPSingleton<MetaUpgradesManager>.Instance.UnlockedUpgrades.Contains(this) || TPSingleton<MetaUpgradesManager>.Instance.FulfilledUpgrades.Contains(this)))
		{
			return true;
		}
		return false;
	}

	public ISerializedData Serialize()
	{
		return new SerializedMetaUpgrade
		{
			InvestedSouls = InvestedSouls,
			Id = MetaUpgradeDefinition.Id
		};
	}
}
