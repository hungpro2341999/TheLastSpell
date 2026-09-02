using System;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Serialization.Perk;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.HUD.UnitManagement;
using TheLastStand.View.Unit.Perk;

namespace TheLastStand.Controller.Unit.Perk;

public class UnitPerkTreeController
{
	public UnitPerkTree UnitPerkTree { get; private set; }

	public UnitPerkTreeController(UnitPerkTreeView view, PlayableUnit playableUnit)
	{
		UnitPerkTree = new UnitPerkTree(this, view, playableUnit);
		if (!TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.Inited)
		{
			TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.Init();
		}
		if (UnitPerkTree.UnitPerkTreeView.UnitPerkTierViews.Count < PlayableUnitDatabase.UnitPerkTemplateDefinition.TierCount)
		{
			TPSingleton<CharacterSheetManager>.Instance.LogError("UnitPerkTreeView must have as many UnitPerkTierViews as in PlayableUnitDatabase.UnitPerkTierDefinitions --> Have you lost a reference on UnitPerkTierViews prefab?");
		}
	}

	public void BuyPerk()
	{
		if (UnitPerkTree.CanBuyPerk())
		{
			UnitPerkTree.UnitPerkTreeView.SelectedPerk.Perk.PerkController.Unlock(UnitPerkTree.PlayableUnit);
			UnitPerkTree.PlayableUnit.PerksPoints--;
			UnitPerkTree.UnitPerkTreeView.RefreshSelectedPerk(UnitPerkTree.UnitPerkTreeView.SelectedPerk);
			OnSetNewPerk(UnitPerkTree.UnitPerkTreeView.SelectedPerk.PerkDefinition.Id, UnitPerkTree.UnitPerkTreeView.SelectedPerk.Perk);
			UpdateTiersAvailability();
			UnitPerkTree.UnitPerkTreeView.RefreshPerkPoints();
			TPSingleton<CharacterSheetPanel>.Instance.RefreshStats();
			TPSingleton<CharacterSheetPanel>.Instance.RefreshPerkAvailableNotif(TileObjectSelectionManager.SelectedPlayableUnit);
			TPSingleton<CharacterSheetPanel>.Instance.RefreshOpenedPage();
		}
	}

	public void GeneratePerkTree(PlayableUnit owner, bool checkExistingPerks = false)
	{
		List<UnitPerkCollectionDefinition> list = PickRandomCollections();
		UnitPerkTree.SetCollectionIds(list);
		for (int i = 0; i < PlayableUnitDatabase.UnitPerkTemplateDefinition.TierCount; i++)
		{
			if (!PlayableUnitDatabase.UnitPerkTemplateDefinition.RequiredPerksCountPerTier.TryGetValue(i + 1, out var value))
			{
				TPSingleton<PlayableUnitManager>.Instance.LogError($"Missing tier for the requiredPerksCount in UnitPerkTemplateDefinition. Tier missing : \"{i}\". Skip.");
				continue;
			}
			UnitPerkTier unitPerkTier = new UnitPerkTierController(UnitPerkTree.UnitPerkTreeView.UnitPerkTierViews[i], UnitPerkTree, value, i).UnitPerkTier;
			UnitPerkTree.UnitPerkTiers.Add(unitPerkTier);
			for (int j = 0; j < list.Count; j++)
			{
				PerkDefinition perkDefinition = PickRandomPerkDefinition(list, i, j);
				UnitPerkDisplay unitPerkDisplay = TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.UnitPerkTierViews[i].PerkDisplays[j];
				TheLastStand.Model.Unit.Perk.Perk perk = null;
				if (perkDefinition != null)
				{
					if (checkExistingPerks && owner.Perks.ContainsKey(perkDefinition.Id))
					{
						perk = owner.Perks[perkDefinition.Id];
						perk.PerkController.ChangePerkTierAndView(unitPerkTier, unitPerkDisplay);
						perk.PerkController.ChangeCollection(list[j].Id);
					}
					else
					{
						perk = new PerkController(perkDefinition, unitPerkDisplay, owner, unitPerkTier, list[j].Id, isNative: false, isFromRace: false).Perk;
					}
					owner.PlayableUnitPerksController.TryAddPerk(perk);
				}
				unitPerkTier.Perks.Add((perkDefinition == null) ? null : perk);
			}
			if (i == 0)
			{
				unitPerkTier.UnitPerkTierController.Unlock();
			}
		}
	}

	public void GeneratePerkTree(List<SerializedPerkCollection> perkCollections, PlayableUnit owner, bool isOwnerDead)
	{
		FixHumanPerkCollectionNotBeingSet(perkCollections, owner);
		UnitPerkTree.SetCollectionIds(perkCollections.Select((SerializedPerkCollection perkCollection) => perkCollection.Id).ToList());
		for (int num = 0; num < PlayableUnitDatabase.UnitPerkTemplateDefinition.TierCount; num++)
		{
			if (!PlayableUnitDatabase.UnitPerkTemplateDefinition.RequiredPerksCountPerTier.TryGetValue(num + 1, out var value))
			{
				TPSingleton<PlayableUnitManager>.Instance.LogError($"Missing tier for the requiredPerksCount in UnitPerkTemplateDefinition. Tier missing : \"{num}\". Skip.");
				continue;
			}
			UnitPerkTier unitPerkTier = new UnitPerkTierController(UnitPerkTree.UnitPerkTreeView.UnitPerkTierViews[num], UnitPerkTree, value, num).UnitPerkTier;
			UnitPerkTree.UnitPerkTiers.Add(unitPerkTier);
			for (int num2 = 0; num2 < perkCollections.Count; num2++)
			{
				bool num3 = perkCollections[num2].Perks.Count > num;
				SerializedPerk serializedPerk = (num3 ? perkCollections[num2].Perks[num] : null);
				if (num3 && PlayableUnitDatabase.PerkDefinitions.ContainsKey(serializedPerk.Id))
				{
					PerkDefinition perkDefinition = PlayableUnitDatabase.PerkDefinitions[perkCollections[num2].Perks[num].Id];
					UnitPerkDisplay perkView = TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.UnitPerkTierViews[num].PerkDisplays[num2];
					unitPerkTier.Perks.Add(new PerkController(serializedPerk, perkDefinition, perkView, owner, unitPerkTier, perkCollections[num2].Id, isOwnerDead, isNative: false, isFromRace: false).Perk);
				}
				else
				{
					unitPerkTier.Perks.Add(null);
				}
			}
			UpdateTiersAvailability(0, refreshView: false);
		}
	}

	public bool IsSkillLockedByPerks(TheLastStand.Model.Skill.Skill skill)
	{
		if (!skill.SkillDefinition.IsLockedByPerk)
		{
			return false;
		}
		return !UnitPerkTree.PlayableUnit.ContextualSkills.Contains(skill);
	}

	public void SelectPerk(UnitPerkDisplay selectedPerkDisplay)
	{
		if (!(UnitPerkTree.UnitPerkTreeView.SelectedPerk == selectedPerkDisplay) && (!(selectedPerkDisplay != null) || selectedPerkDisplay.Perk != null))
		{
			UnitPerkTree.UnitPerkTreeView.RefreshSelectedPerk(selectedPerkDisplay);
		}
	}

	public void UnlockPerk(string perkDefinitionId)
	{
		for (int num = UnitPerkTree.UnitPerkTiers.Count - 1; num >= 0; num--)
		{
			for (int num2 = UnitPerkTree.UnitPerkTiers[num].Perks.Count - 1; num2 >= 0; num2--)
			{
				TheLastStand.Model.Unit.Perk.Perk perk = UnitPerkTree.UnitPerkTiers[num].Perks[num2];
				if (perk != null && perk.PerkDefinition.Id == perkDefinitionId)
				{
					perk.PerkController.Unlock(UnitPerkTree.PlayableUnit);
					TPSingleton<CharacterSheetPanel>.Instance.RefreshStats();
					TPSingleton<CharacterSheetPanel>.Instance.RefreshOpenedPage();
					UnitPerkTree.UnitPerkTreeView.RefreshSelectedPerk(UnitPerkTree.UnitPerkTreeView.SelectedPerk);
				}
			}
		}
		UpdateTiersAvailability(1, refreshView: false);
	}

	public void LockPerk(string perkDefinitionId)
	{
		for (int num = UnitPerkTree.UnitPerkTiers.Count - 1; num >= 0; num--)
		{
			for (int num2 = UnitPerkTree.UnitPerkTiers[num].Perks.Count - 1; num2 >= 0; num2--)
			{
				TheLastStand.Model.Unit.Perk.Perk perk = UnitPerkTree.UnitPerkTiers[num].Perks[num2];
				if (perk != null && perk.PerkDefinition.Id == perkDefinitionId)
				{
					perk.PerkController.Lock(UnitPerkTree.PlayableUnit);
					TPSingleton<CharacterSheetPanel>.Instance.RefreshStats();
					TPSingleton<CharacterSheetPanel>.Instance.RefreshOpenedPage();
					UnitPerkTree.UnitPerkTreeView.RefreshSelectedPerk(UnitPerkTree.UnitPerkTreeView.SelectedPerk);
				}
			}
		}
		UpdateTiersAvailability(1, refreshView: false);
	}

	public bool HasPerkAlready(PerkDefinition perkDefinition)
	{
		if (!TPSingleton<GlyphManager>.Instance.NativePerksToUnlock.ContainsKey(perkDefinition.Id))
		{
			return UnitPerkTree.UnitPerkTiers.Any((UnitPerkTier perkTier) => perkTier.Perks.Any((TheLastStand.Model.Unit.Perk.Perk perk) => perk != null && perk.PerkDefinition == perkDefinition));
		}
		return true;
	}

	private void OnSetNewPerk(string id, TheLastStand.Model.Unit.Perk.Perk unitPerk, bool shouldRefreshHud = true)
	{
		TPSingleton<PlayableUnitManagementView>.Instance.PlayableSkillBar.Refresh(fullRefresh: true);
	}

	private List<UnitPerkCollectionDefinition> PickRandomCollections()
	{
		List<UnitPerkCollectionDefinition> list = new List<UnitPerkCollectionDefinition>();
		HashSet<UnitPerkCollectionDefinition> hashSet = new HashSet<UnitPerkCollectionDefinition>();
		for (int i = 0; i < PlayableUnitDatabase.UnitPerkTemplateDefinition.UnitPerkCollectionSetDefinitions.Count; i++)
		{
			UnitPerkCollectionDefinition item = PickRandomCollection(i, hashSet.Select((UnitPerkCollectionDefinition collection) => collection.Id).ToList());
			list.Add(item);
			hashSet.Add(item);
		}
		return list;
	}

	private UnitPerkCollectionDefinition PickRandomCollection(int collectionIndex, List<string> alreadyPickedCollectionIds)
	{
		UnitPerkCollectionDefinition result = null;
		List<Tuple<UnitPerkCollectionDefinition, int>> list = new List<Tuple<UnitPerkCollectionDefinition, int>>();
		int num = 0;
		foreach (Tuple<string, int, string> item in PlayableUnitDatabase.UnitPerkTemplateDefinition.UnitPerkCollectionSetDefinitions[collectionIndex].CollectionsPerWeight)
		{
			UnitPerkCollectionDefinition unitPerkCollectionDefinition = PlayableUnitDatabase.UnitPerkCollectionDefinitions[item.Item1];
			if (IsPerkCollectionAvailableToRace(item.Item3) && (unitPerkCollectionDefinition.MultipleAllowed || !alreadyPickedCollectionIds.Contains(unitPerkCollectionDefinition.Id)))
			{
				num += item.Item2;
				list.Add(new Tuple<UnitPerkCollectionDefinition, int>(unitPerkCollectionDefinition, item.Item2));
			}
		}
		if (list.Count == 0)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError($"Something went wrong on the perk tree generation : there are no potential collections for slot \"{collectionIndex + 1}\". Use default instead.");
			return null;
		}
		int randomRange = RandomManager.GetRandomRange(TPSingleton<PlayableUnitManager>.Instance, 0, num);
		foreach (Tuple<UnitPerkCollectionDefinition, int> item2 in list)
		{
			num -= item2.Item2;
			if (randomRange >= num)
			{
				result = item2.Item1;
				break;
			}
		}
		return result;
	}

	private PerkDefinition PickRandomPerkDefinition(List<UnitPerkCollectionDefinition> collections, int perkTierIndex, int perkIndex, string forbiddenPerkId = null)
	{
		List<Tuple<PerkDefinition, int>> list = new List<Tuple<PerkDefinition, int>>();
		int num = 0;
		if (collections[perkIndex] != null)
		{
			foreach (Tuple<PerkDefinition, int> item2 in collections[perkIndex].PerksFromTier[perkTierIndex + 1])
			{
				PerkDefinition value = item2.Item1;
				if (UnitPerkTree.PlayableUnit.PlayableUnitPerksController.PlayableUnitPerks.ActiveReplaceEffects.TryGetValue(value.Id, out var value2))
				{
					string text = value2.FirstOrDefault();
					if (text == null)
					{
						TPSingleton<PlayableUnitManager>.Instance.LogError($"During the reroll of slot : {perkIndex + 1} ; tier : {perkTierIndex + 1}: Empty list in ActiveReplaceEffects.");
						return null;
					}
					if (!PlayableUnitDatabase.PerkDefinitions.TryGetValue(text, out value))
					{
						TPSingleton<PlayableUnitManager>.Instance.LogError($"During the reroll of slot : {perkIndex + 1} ; tier : {perkTierIndex + 1}: Couldn't find the perk definition {text} during the perk reroll and replace.");
						return null;
					}
				}
				if (!HasPerkAlready(value))
				{
					int item = item2.Item2;
					list.Add(new Tuple<PerkDefinition, int>(value, item));
					num += item;
				}
			}
		}
		if (list.Count > 1)
		{
			Tuple<PerkDefinition, int> tuple = list.Find((Tuple<PerkDefinition, int> potentialPerk) => potentialPerk.Item1.Id == forbiddenPerkId);
			if (tuple != null)
			{
				num -= tuple.Item2;
				list.Remove(tuple);
			}
		}
		if (list.Count == 0)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Something went wrong on the perk tree generation :\n" + $"There are no potential perks for slot : {perkIndex + 1} ; tier : {perkTierIndex + 1} ; Collection : {collections[perkIndex]?.Id}. Use default instead.");
			return null;
		}
		if (list.Count == 1)
		{
			return list[0].Item1;
		}
		int randomRange = RandomManager.GetRandomRange(TPSingleton<PlayableUnitManager>.Instance, 0, num);
		foreach (Tuple<PerkDefinition, int> item3 in list)
		{
			num -= item3.Item2;
			if (randomRange >= num)
			{
				return item3.Item1;
			}
		}
		TPSingleton<PlayableUnitManager>.Instance.LogError("Something went wrong with the weight algorithm.", CLogLevel.MAJOR);
		return null;
	}

	private bool IsPerkCollectionAvailableToRace(string perkCollectionRaceId)
	{
		if (UnitPerkTree.PlayableUnit == null)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(perkCollectionRaceId))
		{
			return perkCollectionRaceId == UnitPerkTree.PlayableUnit.RaceDefinition.Id;
		}
		return true;
	}

	private void UpdateTiersAvailability(int startIndex = 1, bool refreshView = true)
	{
		int i = startIndex;
		bool flag = true;
		for (; i < UnitPerkTree.UnitPerkTiers.Count; i++)
		{
			if (UnitPerkTree.UnitPerkTiers[i].RequiredPerksCount <= UnitPerkTree.PlayableUnit.UnlockedPerksCount)
			{
				UnitPerkTree.UnitPerkTiers[i].UnitPerkTierController.Unlock();
			}
			if (refreshView)
			{
				UnitPerkTree.UnitPerkTiers[i].UnitPerkTierView.RefreshAvailability(flag && !UnitPerkTree.UnitPerkTiers[i].Available);
			}
			flag = UnitPerkTree.UnitPerkTiers[i].Available;
		}
	}

	private void FixHumanPerkCollectionNotBeingSet(List<SerializedPerkCollection> perkCollections, PlayableUnit owner)
	{
		if (owner.RaceDefinition?.Id != "Human" || perkCollections.Any((SerializedPerkCollection perkCollection) => perkCollection.Id == "Human"))
		{
			return;
		}
		string text = "Human";
		int num = -1;
		HashSet<string> hashSet = new HashSet<string>();
		for (int num2 = 0; num2 < PlayableUnitDatabase.UnitPerkTemplateDefinition.UnitPerkCollectionSetDefinitions.Count; num2++)
		{
			foreach (Tuple<string, int, string> item in PlayableUnitDatabase.UnitPerkTemplateDefinition.UnitPerkCollectionSetDefinitions[num2].CollectionsPerWeight)
			{
				if (item.Item3 == text)
				{
					num = num2;
					break;
				}
			}
		}
		if (num == -1)
		{
			return;
		}
		foreach (Tuple<string, int, string> item2 in PlayableUnitDatabase.UnitPerkTemplateDefinition.UnitPerkCollectionSetDefinitions[num].CollectionsPerWeight)
		{
			hashSet.Add(item2.Item3);
		}
		if (num < perkCollections.Count && hashSet.Contains(text) && !hashSet.Contains(perkCollections[num].Id) && perkCollections[num].Id == "Misc")
		{
			perkCollections[num].Id = text;
		}
	}

	public void DebugUpdateAllTiersAvailability()
	{
		foreach (UnitPerkTier unitPerkTier in UnitPerkTree.UnitPerkTiers)
		{
			unitPerkTier.UnitPerkTierController.Unlock();
		}
		UnitPerkTree.UnitPerkTreeView.Refresh();
	}

	public bool CollectionHasPotentialReroll(int collectionIndex)
	{
		if (UnitPerkTree.DoesCollectionRerollsCompletely(collectionIndex))
		{
			return !UnitPerkTree.UnitPerkTiers.Any((UnitPerkTier tier) => tier.Perks[collectionIndex].UnlockedInPerkTree);
		}
		return UnitPerkTree.UnitPerkTiers.Any((UnitPerkTier tier) => !tier.Perks[collectionIndex].UnlockedInPerkTree && tier.Perks[collectionIndex].HasPotentialReroll());
	}

	public static void UpdateRerollFormula(IEnumerable<TheLastStand.Model.Unit.Perk.Perk> perksToReroll)
	{
		TPSingleton<SinkManager>.Instance.PerkSinkData.Perks = perksToReroll.Count((TheLastStand.Model.Unit.Perk.Perk perk) => perk.UnlockedInPerkTree);
	}

	public void ComputeRerollPrices()
	{
		TPSingleton<SinkManager>.Instance.PerkSinkData.Rerolls = UnitPerkTree.PlayableUnit.PerkRerollCount;
		foreach (UnitPerkTier unitPerkTier in UnitPerkTree.UnitPerkTiers)
		{
			UpdateRerollFormula(unitPerkTier.Perks);
			unitPerkTier.RerollPrice = TPSingleton<SinkManager>.Instance.PerkSinkData.GetFinalPrice();
		}
		UnitPerkTree.CollectionRerollPrices.Clear();
		for (int i = 0; i < UnitPerkTree.UnitPerkCollectionIds.Count; i++)
		{
			int perkIndex = i;
			UpdateRerollFormula(UnitPerkTree.UnitPerkTiers.Select((UnitPerkTier tier) => tier.Perks[perkIndex]));
			UnitPerkTree.CollectionRerollPrices.Add(TPSingleton<SinkManager>.Instance.PerkSinkData.GetFinalPrice());
		}
	}

	public void RerollPerksInTier(int tierIndex, bool payDamnedSouls = true)
	{
		UnitPerkTier unitPerkTier = UnitPerkTree.UnitPerkTiers[tierIndex];
		List<UnitPerkCollectionDefinition> list = UnitPerkTree.UnitPerkCollectionIds.Select((string id) => PlayableUnitDatabase.UnitPerkCollectionDefinitions[id]).ToList();
		HashSet<int> lockedPerkCollectionSlots = TPSingleton<MetaUpgradesManager>.Instance.GetLockedPerkCollectionSlots();
		uint rerollPrice = (uint)UnitPerkTree.UnitPerkTiers[tierIndex].RerollPrice;
		UnitPerkTree.UnitPerkTreeView.CanPlayRerollSound = true;
		for (int num = 0; num < list.Count; num++)
		{
			if (!lockedPerkCollectionSlots.Contains(num + 1))
			{
				TheLastStand.Model.Unit.Perk.Perk perk = unitPerkTier.Perks[num];
				if (!perk.UnlockedInPerkTree && perk.HasPotentialReroll() && CheckRandomRerollPerk())
				{
					RerollPerk(tierIndex, perk, list, num);
				}
			}
		}
		if (payDamnedSouls)
		{
			ApplicationManager.Application.DamnedSouls -= rerollPrice;
		}
		IncreaseRerollNbAndRefreshInterpreter();
	}

	public bool TryRerollPerksInCollection(int collectionIndex, bool payDamnedSouls = true)
	{
		uint num = (uint)UnitPerkTree.CollectionRerollPrices[collectionIndex];
		UnitPerkTree.UnitPerkTreeView.CanPlayRerollSound = true;
		if (UnitPerkTree.DoesCollectionRerollsCompletely(collectionIndex))
		{
			if (!TryRerollCollection(collectionIndex))
			{
				return false;
			}
		}
		else
		{
			List<UnitPerkCollectionDefinition> collections = UnitPerkTree.UnitPerkCollectionIds.Select((string id) => PlayableUnitDatabase.UnitPerkCollectionDefinitions[id]).ToList();
			RerollPerksInCollection(collectionIndex, collections);
		}
		if (payDamnedSouls)
		{
			ApplicationManager.Application.DamnedSouls -= num;
		}
		IncreaseRerollNbAndRefreshInterpreter();
		return true;
	}

	private void RerollPerksInCollection(int collectionIndex, List<UnitPerkCollectionDefinition> collections)
	{
		PlayableUnit playableUnit = UnitPerkTree.PlayableUnit;
		for (int i = 0; i < playableUnit.PerkTree.UnitPerkTiers.Count; i++)
		{
			TheLastStand.Model.Unit.Perk.Perk perk = playableUnit.PerkTree.UnitPerkTiers[i].Perks[collectionIndex];
			if (!perk.UnlockedInPerkTree && perk.HasPotentialReroll() && CheckRandomRerollPerk())
			{
				RerollPerk(i, perk, collections, collectionIndex);
			}
		}
	}

	public static bool CheckRandomRerollPerk()
	{
		float rerollBaseChances = TPSingleton<SinkManager>.Instance.PerkSinkData.RerollBaseChances;
		return RandomManager.GetRandomRange(TPSingleton<SinkManager>.Instance, 0f, 1f) < rerollBaseChances;
	}

	private bool TryRerollCollection(int columnIndex)
	{
		PlayableUnit playableUnit = UnitPerkTree.PlayableUnit;
		for (int i = 0; i < playableUnit.PerkTree.UnitPerkTiers.Count; i++)
		{
			UnitPerkTier unitPerkTier = playableUnit.PerkTree.UnitPerkTiers[i];
			TheLastStand.Model.Unit.Perk.Perk perk = unitPerkTier.Perks[columnIndex];
			if (perk.UnlockedInPerkTree)
			{
				return false;
			}
		}
		for (int j = 0; j < playableUnit.PerkTree.UnitPerkTiers.Count; j++)
		{
			UnitPerkTier unitPerkTier = playableUnit.PerkTree.UnitPerkTiers[j];
			TheLastStand.Model.Unit.Perk.Perk perk = unitPerkTier.Perks[columnIndex];
			RemovePerkFromTree(columnIndex, perk, unitPerkTier);
		}
		UnitPerkCollectionDefinition unitPerkCollectionDefinition = playableUnit.PerkTree.UnitPerkTreeController.PickRandomCollection(columnIndex, playableUnit.PerkTree.UnitPerkCollectionIds);
		playableUnit.PerkTree.UnitPerkCollectionIds[columnIndex] = unitPerkCollectionDefinition.Id;
		List<UnitPerkCollectionDefinition> list = playableUnit.PerkTree.UnitPerkCollectionIds.Select((string key) => PlayableUnitDatabase.UnitPerkCollectionDefinitions[key]).ToList();
		for (int num = 0; num < playableUnit.PerkTree.UnitPerkTiers.Count; num++)
		{
			PerkDefinition perkDefinition = playableUnit.PerkTree.UnitPerkTreeController.PickRandomPerkDefinition(list, num, columnIndex);
			string id = list[columnIndex].Id;
			AddNewPerk(columnIndex, num, perkDefinition, id);
		}
		return true;
	}

	private void AddNewPerk(int collectionIndex, int tierIndex, PerkDefinition perkDefinition, string collectionId)
	{
		UnitPerkDisplay unitPerkDisplay = TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.UnitPerkTierViews[tierIndex].PerkDisplays[collectionIndex];
		UnitPerkTier unitPerkTier = UnitPerkTree.UnitPerkTiers[tierIndex];
		PlayableUnit playableUnit = UnitPerkTree.PlayableUnit;
		TheLastStand.Model.Unit.Perk.Perk perk;
		if (playableUnit.Perks.TryGetValue(perkDefinition.Id, out var value))
		{
			perk = value;
			perk.PerkController.ChangePerkTierAndView(unitPerkTier, unitPerkDisplay);
			perk.PerkController.ChangeCollection(collectionId);
		}
		else
		{
			perk = new PerkController(perkDefinition, unitPerkDisplay, playableUnit, unitPerkTier, collectionId, isNative: false, isFromRace: false).Perk;
		}
		playableUnit.PlayableUnitPerksController.TryAddPerk(perk);
		unitPerkTier.Perks[collectionIndex] = perk;
		unitPerkDisplay.SetContent(perk);
		unitPerkDisplay.Init();
		unitPerkDisplay.PlayRerollSuccess();
	}

	private void RemovePerkFromTree(int columnIndex, TheLastStand.Model.Unit.Perk.Perk perk, UnitPerkTier perkTier)
	{
		perk.PerkController.ChangePerkTierAndView(null, null);
		perkTier.Perks[columnIndex] = null;
	}

	private void RerollPerk(int tierIndex, TheLastStand.Model.Unit.Perk.Perk perk, List<UnitPerkCollectionDefinition> collections, int collectionIndex)
	{
		PlayableUnit playableUnit = UnitPerkTree.PlayableUnit;
		RemovePerkFromTree(collectionIndex, perk, UnitPerkTree.UnitPerkTiers[tierIndex]);
		PerkDefinition perkDefinition = playableUnit.PerkTree.UnitPerkTreeController.PickRandomPerkDefinition(collections, tierIndex, collectionIndex, perk.PerkDefinition.Id);
		if (perkDefinition == null)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("During the reroll of " + perk.PerkDefinition.Id + ": Couldn't find an availble perk definition.");
			return;
		}
		string id = collections[collectionIndex].Id;
		AddNewPerk(collectionIndex, tierIndex, perkDefinition, id);
	}

	public void ResetSinkNbReroll()
	{
		UnitPerkTree.PlayableUnit.PerkRerollCount = 0;
	}

	private void IncreaseRerollNbAndRefreshInterpreter()
	{
		UnitPerkTree.PlayableUnit.PerkRerollCount++;
		TPSingleton<SinkManager>.Instance.PerkSinkData.Rerolls = UnitPerkTree.PlayableUnit.PerkRerollCount;
	}
}
