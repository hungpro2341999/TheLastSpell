using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Database.Unit;
using TheLastStand.Manager.Item;
using TheLastStand.Model.Item;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Perk;

namespace TheLastStand.Controller.Unit.Perk;

public class PlayableUnitPerksController
{
	public PlayableUnitPerks PlayableUnitPerks { get; private set; }

	public PlayableUnitPerksController(PlayableUnit playableUnit)
	{
		PlayableUnitPerks = new PlayableUnitPerks(this, playableUnit);
	}

	public bool TryAddPerk(TheLastStand.Model.Unit.Perk.Perk perk)
	{
		if (perk == null)
		{
			return false;
		}
		if (!PlayableUnitPerks.Perks.ContainsKey(perk.PerkDefinition.Id))
		{
			PlayableUnitPerks.Perks.Add(perk.PerkDefinition.Id, perk);
			return true;
		}
		return false;
	}

	public void AddActiveReplaceEffect(string perkToReplaceId, string newPerkId)
	{
		if (!PlayableUnitPerks.ActiveReplaceEffects.ContainsKey(perkToReplaceId))
		{
			PlayableUnitPerks.ActiveReplaceEffects.Add(perkToReplaceId, new List<string> { newPerkId });
		}
		else
		{
			PlayableUnitPerks.ActiveReplaceEffects[perkToReplaceId].Add(newPerkId);
		}
	}

	public void RemoveActiveReplaceEffect(string perkToReplaceId, string newPerkId)
	{
		if (PlayableUnitPerks.ActiveReplaceEffects.ContainsKey(perkToReplaceId))
		{
			PlayableUnitPerks.ActiveReplaceEffects[perkToReplaceId].Remove(newPerkId);
			if (PlayableUnitPerks.ActiveReplaceEffects[perkToReplaceId].Count == 0)
			{
				PlayableUnitPerks.ActiveReplaceEffects.Remove(perkToReplaceId);
			}
		}
	}

	public void OnItemEquipped(EquipmentSlot equipmentSlot, TheLastStand.Model.Item.Item item)
	{
		if (item == null)
		{
			return;
		}
		foreach (string item2 in item.PerksId)
		{
			string key = item2;
			if (PlayableUnitPerks.ActiveReplaceEffects.ContainsKey(item2))
			{
				key = PlayableUnitPerks.ActiveReplaceEffects[item2].FirstOrDefault();
			}
			if (!PlayableUnitPerks.Perks.ContainsKey(key))
			{
				if (PlayableUnitDatabase.PerkDefinitions.TryGetValue(key, out var value))
				{
					new PerkController(value, null, PlayableUnitPerks.PlayableUnit, null, string.Empty, isNative: false, isFromRace: false).Perk.PerkController.Unlock(item);
				}
				else
				{
					TPSingleton<ItemManager>.Instance.LogError("Couldn't create a perk " + item2 + " when equipping item " + item.ItemDefinition.Id);
				}
			}
			else
			{
				PlayableUnitPerks.Perks[key].PerkController.Unlock(item);
			}
		}
	}

	public void OnItemUnequipped(EquipmentSlot equipmentSlot)
	{
		if (equipmentSlot.Item == null)
		{
			return;
		}
		foreach (string item in equipmentSlot.Item.PerksId)
		{
			string key = item;
			if (PlayableUnitPerks.ActiveReplaceEffects.ContainsKey(item))
			{
				key = PlayableUnitPerks.ActiveReplaceEffects[item].FirstOrDefault();
			}
			if (PlayableUnitPerks.PlayableUnit.Perks.TryGetValue(key, out var value))
			{
				value.PerkController.Lock(equipmentSlot.Item);
			}
		}
	}

	public void ResetLockedPerksModulesData()
	{
		foreach (TheLastStand.Model.Unit.Perk.Perk value in PlayableUnitPerks.Perks.Values)
		{
			if (!value.Unlocked)
			{
				value.PerkController.ResetModulesDynamicData();
			}
		}
	}
}
