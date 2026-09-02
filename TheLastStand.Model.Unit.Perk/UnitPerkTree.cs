using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Controller.Unit.Perk;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Manager;
using TheLastStand.Serialization.Perk;
using TheLastStand.View.Unit.Perk;

namespace TheLastStand.Model.Unit.Perk;

public class UnitPerkTree
{
	public PlayableUnit PlayableUnit { get; }

	public List<string> UnitPerkCollectionIds { get; private set; }

	public List<int> CollectionRerollPrices { get; } = new List<int>();

	public List<UnitPerkTier> UnitPerkTiers { get; } = new List<UnitPerkTier>();

	public UnitPerkTreeController UnitPerkTreeController { get; }

	public UnitPerkTreeView UnitPerkTreeView { get; }

	public UnitPerkTree(UnitPerkTreeController controller, UnitPerkTreeView view, PlayableUnit playableUnit)
	{
		PlayableUnit = playableUnit;
		UnitPerkTreeController = controller;
		UnitPerkTreeView = view;
	}

	public bool CanBuyPerk()
	{
		if (PlayableUnit.PerksPoints > 0)
		{
			return TPSingleton<GameManager>.Instance.Game.State != Game.E_State.GameOver;
		}
		return false;
	}

	public bool DoesCollectionRerollsCompletely(int collectionIndex)
	{
		if (UnitPerkCollectionIds != null && UnitPerkCollectionIds.Count > collectionIndex)
		{
			return PlayableUnitDatabase.UnitPerkCollectionDefinitions[UnitPerkCollectionIds[collectionIndex]].DoesCollectionRerollsCompletely;
		}
		return false;
	}

	public bool HasReachedMaxPerks()
	{
		if (PlayableUnit.PerksPoints == 0)
		{
			return (double)PlayableUnitDatabase.PerksPointsPerLevel.Max((KeyValuePair<int, int> o) => o.Key) <= PlayableUnit.Level;
		}
		return false;
	}

	public void SetCollectionIds(List<UnitPerkCollectionDefinition> collections)
	{
		UnitPerkCollectionIds = new List<string>();
		for (int i = 0; i < collections.Count; i++)
		{
			UnitPerkCollectionIds.Add(collections[i]?.Id);
		}
	}

	public void SetCollectionIds(List<string> collectionIds)
	{
		UnitPerkCollectionIds = collectionIds;
	}

	public List<SerializedPerkCollection> Serialize()
	{
		List<SerializedPerkCollection> list = new List<SerializedPerkCollection>();
		for (int i = 0; i < UnitPerkCollectionIds.Count; i++)
		{
			List<SerializedPerk> list2 = new List<SerializedPerk>();
			for (int j = 0; j < UnitPerkTiers.Count; j++)
			{
				if (UnitPerkTiers[j].Perks.Count > i && UnitPerkTiers[j].Perks[i] != null)
				{
					list2.Add(UnitPerkTiers[j].Perks[i].Serialize() as SerializedPerk);
				}
			}
			list.Add(new SerializedPerkCollection
			{
				Id = UnitPerkCollectionIds[i],
				Perks = list2
			});
		}
		return list;
	}
}
