using System;
using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkEvent;
using TheLastStand.Model.Unit.Perk.PerkModule;
using TheLastStand.Serialization.Perk;
using TheLastStand.View.HUD.UnitManagement;
using TheLastStand.View.TileMap;
using TheLastStand.View.Unit.Perk;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Perk;

public class PerkController
{
	public bool CanUnlockPerk
	{
		get
		{
			if (Perk.Owner != null)
			{
				return !Perk.Owner.IsDead;
			}
			return false;
		}
	}

	public TheLastStand.Model.Unit.Perk.Perk Perk { get; }

	public PerkController(PerkDefinition perkDefinition, UnitPerkDisplay perkView, PlayableUnit owner, UnitPerkTier perkTier, string collectionId, bool isNative, bool isFromRace)
	{
		Perk = new TheLastStand.Model.Unit.Perk.Perk(perkDefinition, this, perkView, owner, perkTier, collectionId, isNative, isFromRace);
	}

	public PerkController(SerializedPerk serializedPerk, PerkDefinition perkDefinition, UnitPerkDisplay perkView, PlayableUnit owner, UnitPerkTier perkTier, string collectionId, bool isOwnerDead, bool isNative, bool isFromRace)
	{
		Perk = new TheLastStand.Model.Unit.Perk.Perk(serializedPerk, perkDefinition, this, perkView, owner, perkTier, collectionId, isNative, isFromRace);
		if (isOwnerDead || Perk.Owner == null)
		{
			return;
		}
		Perk.Owner.PlayableUnitPerksController.TryAddPerk(Perk);
		if (Perk.Unlocked)
		{
			if (!Perk.Unlockers.Contains(owner))
			{
				Perk.Unlockers.Add(owner);
			}
			Hook(onLoad: true);
		}
	}

	public void ActiveBookmark(bool value)
	{
		Perk.Bookmarked = value;
		UnitPerkDisplay perkView = Perk.PerkView;
		if (!(perkView == null))
		{
			if (value)
			{
				perkView.DisplayBookmark();
			}
			else
			{
				perkView.HideBookmark();
			}
		}
	}

	public void ChangeCollection(string newCollectionId)
	{
		Perk.CollectionId = newCollectionId;
	}

	public void ChangeOwner(PlayableUnit newOwner)
	{
		Perk.Owner = newOwner;
	}

	public void ChangePerkTierAndView(UnitPerkTier newPerkTier, UnitPerkDisplay newPerkDisplay)
	{
		Perk.PerkTier = newPerkTier;
		Perk.PerkView = newPerkDisplay;
	}

	public void DisplayRange(bool show)
	{
		if (Perk.PerkDefinition.HoverRanges.Count != 0 || Perk.PerkDefinition.DisplayHavenArea)
		{
			if (show)
			{
				TPSingleton<TileMapView>.Instance.DisplayPerkHoverRangeTiles(Perk);
			}
			else
			{
				TPSingleton<TileMapView>.Instance.ClearPerkHoverRangeTiles();
			}
		}
	}

	public void LockAndClearUnlockers(bool removePerkFromOwner = false)
	{
		if (Perk.Unlocked)
		{
			Perk.Unlockers.Clear();
			Lock();
			if (removePerkFromOwner && Perk.Owner.Perks.ContainsKey(Perk.PerkDefinition.Id))
			{
				Perk.Owner.Perks.Remove(Perk.PerkDefinition.Id);
			}
		}
	}

	public void Lock(IPerkUnlocker perkUnlocker)
	{
		if (Perk.Unlockers.Contains(perkUnlocker))
		{
			Perk.Unlockers.Remove(perkUnlocker);
		}
		if (Perk.Unlockers.Count == 0)
		{
			Lock();
		}
	}

	public void Unlock(IPerkUnlocker perkUnlocker)
	{
		if (Perk.Owner == null)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Trying to unlock a perk " + Perk.PerkDefinition.Id + " with no owner !");
		}
		else
		{
			if (!CanUnlockPerk)
			{
				return;
			}
			Perk.Owner.PlayableUnitPerksController.TryAddPerk(Perk);
			int count = Perk.Unlockers.Count;
			if (!Perk.Unlockers.Contains(perkUnlocker))
			{
				Perk.Unlockers.Add(perkUnlocker);
				if (perkUnlocker is PlayableUnit && Perk.PerkTier != null && Analytics.AllowedToSendData)
				{
					Analytics.SendPerkUnlockEvent(Perk.Owner.AnalyticsIdentifier, Perk.PerkDefinition.Id);
				}
			}
			if (count > 0 && Perk.Unlocked)
			{
				if (perkUnlocker is PlayableUnit && Perk.PerkView != null)
				{
					Perk.PerkController.ActiveBookmark(value: false);
					Perk.PerkView.Unlock();
				}
			}
			else
			{
				Unlock();
			}
		}
	}

	public void Unlock(List<IPerkUnlocker> perkUnlockers)
	{
		if (!CanUnlockPerk || perkUnlockers == null || perkUnlockers.Count <= 0)
		{
			return;
		}
		foreach (IPerkUnlocker perkUnlocker in perkUnlockers)
		{
			Unlock(perkUnlocker);
		}
	}

	public void UnHook(bool onLoad)
	{
		HandleHook((Action<PerkDataContainer> eventContainer, Action<PerkDataContainer> eventToHook) => (Action<PerkDataContainer>)Delegate.Remove(eventContainer, eventToHook), isHooking: false, onLoad);
	}

	private void Hook(bool onLoad)
	{
		HandleHook((Action<PerkDataContainer> eventContainer, Action<PerkDataContainer> eventToHook) => (Action<PerkDataContainer>)Delegate.Combine(eventContainer, eventToHook), isHooking: true, onLoad);
	}

	public void ResetModulesDynamicData()
	{
		foreach (APerkModule perkModule in Perk.PerkModules)
		{
			perkModule.ResetDynamicData();
		}
	}

	private void Lock()
	{
		Perk.PerkController.ActiveBookmark(value: false);
		if (Perk.Unlocked)
		{
			Perk.Unlocked = false;
			Perk.Owner.Log("Perk " + Perk.PerkDefinition.Id + " Locked.", CLogLevel.DETAILED);
			UnHook(onLoad: false);
			TPSingleton<PlayableUnitManagementView>.Instance.PlayableSkillBar.RefreshSkillDisplays();
		}
	}

	private void Unlock()
	{
		if (!Perk.Unlocked)
		{
			Perk.Unlocked = true;
			Perk.PerkController.ActiveBookmark(value: false);
			if (Perk.PerkView != null && Perk.UnlockedInPerkTree)
			{
				Perk.PerkView.Unlock();
			}
			Perk.Owner.Log("Perk " + Perk.PerkDefinition.Id + " Unlocked.", CLogLevel.DETAILED);
			Hook(onLoad: false);
			TPSingleton<MetaConditionManager>.Instance.RefreshMaxPlayableUnitStatReached(Perk.Owner);
			TPSingleton<PlayableUnitManagementView>.Instance.PlayableSkillBar.RefreshSkillDisplays();
		}
	}

	private void HandleHook(Func<Action<PerkDataContainer>, Action<PerkDataContainer>, Action<PerkDataContainer>> hookOperator, bool isHooking, bool onLoad)
	{
		foreach (APerkModule perkModule in Perk.PerkModules)
		{
			if (isHooking)
			{
				perkModule.OnUnlock(onLoad);
			}
			else
			{
				perkModule.Lock(onLoad);
			}
			foreach (APerkEffect perkEffect in perkModule.PerkEffects)
			{
				if (isHooking)
				{
					perkEffect.APerkEffectController.OnUnlock(onLoad);
				}
				else
				{
					perkEffect.APerkEffectController.Lock(onLoad);
				}
			}
			foreach (PerkEvent perkEvent in perkModule.PerkEvents)
			{
				switch (perkEvent.PerkEventDefinition.EffectTime)
				{
				case E_EffectTime.OnCreation:
					perkEvent.TryTrigger(null);
					break;
				case E_EffectTime.OnDeath:
				case E_EffectTime.OnTileCrossed:
				case E_EffectTime.OnMovementEnd:
				case E_EffectTime.OnHitTaken:
				case E_EffectTime.OnDodge:
				case E_EffectTime.OnTargetHit:
				case E_EffectTime.OnTargetDodge:
				case E_EffectTime.OnTargetKilled:
				case E_EffectTime.OnAttackDataComputed:
				case E_EffectTime.OnSkillNextHit:
				case E_EffectTime.OnSkillCastBegin:
				case E_EffectTime.OnSkillCastEnd:
				case E_EffectTime.OnSkillStatusApplied:
				case E_EffectTime.OnStatusApplied:
				case E_EffectTime.OnEnemyMovementEnd:
				case E_EffectTime.OnSkillUndo:
				case E_EffectTime.OnDealDamageTargetHit:
				case E_EffectTime.OnDealDamageTargetKill:
				case E_EffectTime.OnDealDamageExecutionEnd:
				case E_EffectTime.OnPerkApplyStatusEnd:
				case E_EffectTime.OnRemoveStatus:
				case E_EffectTime.OnUnitSpawned:
				case E_EffectTime.OnTargetHitBeforeSkillEffectsApplied:
					if (!Perk.Owner.Events.ContainsKey(perkEvent.PerkEventDefinition.EffectTime))
					{
						CLoggerManager.Log($"Tried to link a perkEvent to an owner event, but it was not present in the event dictionary: {perkEvent.PerkEventDefinition.EffectTime}", LogType.Error, CLogLevel.MAJOR);
					}
					else
					{
						Perk.Owner.Events[perkEvent.PerkEventDefinition.EffectTime] = hookOperator(Perk.Owner.Events[perkEvent.PerkEventDefinition.EffectTime], perkEvent.TryTrigger);
					}
					break;
				default:
					if (!TPSingleton<EffectTimeEventManager>.Instance.Events.ContainsKey(perkEvent.PerkEventDefinition.EffectTime))
					{
						CLoggerManager.Log($"Tried to link a perkEvent to the global EffectTimeEventManager, but it was not present in the event dictionary: {perkEvent.PerkEventDefinition.EffectTime}", LogType.Error, CLogLevel.MAJOR);
					}
					else
					{
						TPSingleton<EffectTimeEventManager>.Instance.Events[perkEvent.PerkEventDefinition.EffectTime] = hookOperator(TPSingleton<EffectTimeEventManager>.Instance.Events[perkEvent.PerkEventDefinition.EffectTime], perkEvent.TryTrigger);
					}
					break;
				case E_EffectTime.None:
				case E_EffectTime.Permanent:
				case E_EffectTime.OnCreationAfterViewInitialized:
					break;
				}
			}
		}
	}
}
