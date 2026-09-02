using System.Collections;
using System.Collections.Generic;
using TPLib;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Item;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;
using TheLastStand.View.Unit.Perk;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class ReplacePerkEffectController : APerkEffectController
{
	public ReplacePerkEffect ReplacePerkEffect => base.PerkEffect as ReplacePerkEffect;

	public ReplacePerkEffectController(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	public override void OnUnlock(bool onLoad)
	{
		base.PerkEffect.APerkModule.Perk.Owner.PlayableUnitPerksController.AddActiveReplaceEffect(ReplacePerkEffect.ReplacePerkEffectDefinition.PerkToReplaceId, ReplacePerkEffect.ReplacePerkEffectDefinition.PerkReplacementId);
		if (!onLoad)
		{
			ReplacePerk(ReplacePerkEffect.ReplacePerkEffectDefinition.PerkToReplaceId, ReplacePerkEffect.ReplacePerkEffectDefinition.PerkReplacementId, isRevertingEffect: false);
		}
	}

	public override void Lock(bool onLoad)
	{
		base.PerkEffect.APerkModule.Perk.Owner.PlayableUnitPerksController.RemoveActiveReplaceEffect(ReplacePerkEffect.ReplacePerkEffectDefinition.PerkToReplaceId, ReplacePerkEffect.ReplacePerkEffectDefinition.PerkReplacementId);
		if (!onLoad)
		{
			ReplacePerk(ReplacePerkEffect.ReplacePerkEffectDefinition.PerkReplacementId, ReplacePerkEffect.ReplacePerkEffectDefinition.PerkToReplaceId, isRevertingEffect: true);
		}
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new ReplacePerkEffect(aPerkEffectDefinition as ReplacePerkEffectDefinition, this, aPerkModule);
	}

	private bool ReplacePerk(string perkToReplaceId, string perkReplacementId, bool isRevertingEffect)
	{
		PlayableUnit owner = base.PerkEffect.APerkModule.Perk.Owner;
		if (!owner.Perks.ContainsKey(perkToReplaceId) || (owner.Perks.ContainsKey(perkReplacementId) && owner.Perks[perkReplacementId].PerkTier != null))
		{
			return false;
		}
		TheLastStand.Model.Unit.Perk.Perk perkToReplace = owner.Perks[perkToReplaceId];
		MonoBehaviour monoBehaviour = owner.UnitView;
		if (monoBehaviour == null)
		{
			monoBehaviour = TPSingleton<PlayableUnitManager>.Instance;
		}
		monoBehaviour.StartCoroutine(ReplacePerkCoroutine(perkToReplace, perkReplacementId, isRevertingEffect));
		return true;
	}

	private IEnumerator ReplacePerkCoroutine(TheLastStand.Model.Unit.Perk.Perk perkToReplace, string perkReplacementId, bool isRevertingEffect)
	{
		UnitPerkTier unitPerkTier = perkToReplace.PerkTier;
		yield return new WaitForSeconds(UnityEngine.Random.Range(PlayableUnitManager.PerkReplacementRandomDelay.x, PlayableUnitManager.PerkReplacementRandomDelay.y));
		bool unlocked = perkToReplace.Unlocked;
		bool bookmarked = perkToReplace.Bookmarked;
		bool isUnlockedFromPlayableUnit = perkToReplace.IsUnlockedFromPlayableUnit;
		bool isUnlockedFromItem = perkToReplace.IsUnlockedFromItem;
		PlayableUnit owner = perkToReplace.Owner;
		UnitPerkDisplay perkView = perkToReplace.PerkView;
		List<IPerkUnlocker> list = new List<IPerkUnlocker>();
		foreach (IPerkUnlocker unlocker in perkToReplace.Unlockers)
		{
			list.Add(unlocker);
		}
		bool flag = true;
		if (isUnlockedFromItem && isRevertingEffect)
		{
			List<IPerkUnlocker> list2 = new List<IPerkUnlocker>();
			foreach (IPerkUnlocker item2 in list)
			{
				if (item2 is TheLastStand.Model.Item.Item item && item.PerksId.Contains(perkToReplace.PerkDefinition.Id))
				{
					flag = false;
					list2.Add(item2);
				}
			}
			foreach (IPerkUnlocker item3 in list2)
			{
				list.Remove(item3);
			}
		}
		perkToReplace.PerkController.ChangePerkTierAndView(null, null);
		if (flag)
		{
			perkToReplace.PerkController.LockAndClearUnlockers();
		}
		else
		{
			foreach (IPerkUnlocker item4 in list)
			{
				perkToReplace.Unlockers.Remove(item4);
			}
		}
		TheLastStand.Model.Unit.Perk.Perk perk;
		if (perkToReplace.Owner.Perks.ContainsKey(perkReplacementId))
		{
			perk = perkToReplace.Owner.Perks[perkReplacementId];
			perk.PerkController.ChangePerkTierAndView(unitPerkTier, perkView);
		}
		else
		{
			PerkController perkController = new PerkController(PlayableUnitDatabase.PerkDefinitions[perkReplacementId], perkView, owner, unitPerkTier, perkToReplace.CollectionId, perkToReplace.IsNative, perkToReplace.IsFromRace);
			perkToReplace.Owner.PlayableUnitPerksController.TryAddPerk(perkController.Perk);
			perk = perkController.Perk;
		}
		if (perkView != null)
		{
			perk.PerkView.SetContent(perk);
			perk.PerkView.Init();
		}
		if (unitPerkTier != null)
		{
			unitPerkTier.Perks[unitPerkTier.Perks.IndexOf(perkToReplace)] = perk;
			if (unlocked)
			{
				if (isUnlockedFromPlayableUnit)
				{
					base.PerkEffect.APerkModule.Perk.Owner.PerkTree.UnitPerkTreeController.UnlockPerk(perkReplacementId);
					list.Remove(perkToReplace.Owner);
				}
				perk.PerkController.Unlock(list);
			}
			if (bookmarked)
			{
				perk.PerkController.ActiveBookmark(value: true);
			}
		}
		else
		{
			perk.PerkController.Unlock(list);
		}
	}
}
