using System.Linq;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Localization;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Unit;

namespace TheLastStand.Manager;

public class TurnEndValidationManager : Manager<TurnEndValidationManager>
{
	public static class Constants
	{
		public const string BulletPointFormat = "\t- {0}\n";

		public const int GoldThresholdWarning = 50;

		public const int MaterialsThresholdWarning = 50;
	}

	private static bool debugByPassEndTurnChecks;

	public static bool AnyBlockingPlayableUnitInFog
	{
		get
		{
			if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day && TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Deployment)
			{
				return PlayableUnitManager.HasUnitInFog;
			}
			return false;
		}
	}

	public static bool AnyPlayableUnitWaitingForLevelUp => TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Find((PlayableUnit x) => x.StatsPoints > 0) != null;

	public static bool AnyProdItemsLeft => TPSingleton<BuildingManager>.Instance.ProductionReport.ProducedObjects.Count > 0;

	public static bool EndTurnIsBlocked
	{
		get
		{
			if (!debugByPassEndTurnChecks)
			{
				if (!AnyProdItemsLeft)
				{
					return AnyPlayableUnitWaitingForLevelUp;
				}
				return true;
			}
			return false;
		}
	}

	public static bool CanEndTurnWithoutConsentAsking(out string localizedConsentAsk)
	{
		localizedConsentAsk = string.Empty;
		if (debugByPassEndTurnChecks)
		{
			return true;
		}
		bool flag;
		int num6;
		int num7;
		int freeActionsLeft;
		int num10;
		float num;
		int num3;
		int num5;
		switch (TPSingleton<GameManager>.Instance.Game.Cycle)
		{
		case Game.E_Cycle.Day:
			if (TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production)
			{
				flag = TPSingleton<ResourceManager>.Instance.Workers > 0;
				num6 = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Where((PlayableUnit o) => o.StatsPoints > 0).Count();
				num7 = 0;
				for (int num8 = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count - 1; num8 >= 0; num8--)
				{
					num7 += TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[num8].PerksPoints;
				}
				freeActionsLeft = 0;
				for (int num9 = TPSingleton<BuildingManager>.Instance.Buildings.Count - 1; num9 >= 0; num9--)
				{
					if (TPSingleton<BuildingManager>.Instance.Buildings[num9].ProductionModule?.BuildingActions != null)
					{
						TPSingleton<BuildingManager>.Instance.Buildings[num9].ProductionModule.BuildingActions.ForEach(delegate(BuildingAction o)
						{
							if (ResourceManager.GetModifiedWorkersCost(o.BuildingActionDefinition) == 0 && o.BuildingActionController.CanExecuteAction() && o.BuildingActionController.CanExecuteActionOnAnyTile())
							{
								freeActionsLeft++;
							}
						});
					}
				}
				if ((!TPSingleton<SettingsManager>.Instance.Settings.EndTurnWarnings[1] || TPSingleton<ResourceManager>.Instance.Gold < 50) && (!TPSingleton<SettingsManager>.Instance.Settings.EndTurnWarnings[1] || TPSingleton<ResourceManager>.Instance.Materials < 50) && (!TPSingleton<SettingsManager>.Instance.Settings.EndTurnWarnings[2] || !flag) && (!TPSingleton<SettingsManager>.Instance.Settings.EndTurnWarnings[3] || freeActionsLeft == 0) && num6 == 0)
				{
					if (TPSingleton<SettingsManager>.Instance.Settings.EndTurnWarnings[6])
					{
						num10 = ((num7 == 0) ? 1 : 0);
						if (num10 == 0)
						{
							goto IL_01f1;
						}
					}
					else
					{
						num10 = 1;
					}
					goto IL_028e;
				}
				num10 = 0;
				goto IL_01f1;
			}
			if (TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Deployment)
			{
				return true;
			}
			TPSingleton<TurnEndValidationManager>.Instance.LogError("Trying to check if an undefined day turn can be ended without consent.");
			return true;
		case Game.E_Cycle.Night:
		{
			num = 0f;
			for (int num2 = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count - 1; num2 >= 0; num2--)
			{
				num += TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[num2].GetClampedStatValue(UnitStatDefinition.E_Stat.ActionPoints);
			}
			num3 = 0;
			for (int num4 = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count - 1; num4 >= 0; num4--)
			{
				PlayableUnit playableUnit = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[num4];
				if (!playableUnit.IsInWatchtower && playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.MovePoints) == playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.MovePointsTotal))
				{
					num3++;
				}
			}
			if (!TPSingleton<SettingsManager>.Instance.Settings.EndTurnWarnings[4] || num == 0f)
			{
				if (TPSingleton<SettingsManager>.Instance.Settings.EndTurnWarnings[5])
				{
					num5 = ((num3 == 0) ? 1 : 0);
					if (num5 == 0)
					{
						goto IL_0390;
					}
				}
				else
				{
					num5 = 1;
				}
				goto IL_03cb;
			}
			num5 = 0;
			goto IL_0390;
		}
		default:
			{
				return true;
			}
			IL_028e:
			return (byte)num10 != 0;
			IL_01f1:
			localizedConsentAsk = GetProductionEndConsentAskLocalizedText(TPSingleton<SettingsManager>.Instance.Settings.EndTurnWarnings[1] && TPSingleton<ResourceManager>.Instance.Gold >= 50, TPSingleton<SettingsManager>.Instance.Settings.EndTurnWarnings[1] && TPSingleton<ResourceManager>.Instance.Materials >= 50, TPSingleton<SettingsManager>.Instance.Settings.EndTurnWarnings[2] && flag, TPSingleton<SettingsManager>.Instance.Settings.EndTurnWarnings[3] ? freeActionsLeft : 0, num6, TPSingleton<SettingsManager>.Instance.Settings.EndTurnWarnings[6] ? num7 : 0);
			goto IL_028e;
			IL_0390:
			localizedConsentAsk = GetNightTurnEndConsentAskLocalizedText(TPSingleton<SettingsManager>.Instance.Settings.EndTurnWarnings[4] ? num : 0f, TPSingleton<SettingsManager>.Instance.Settings.EndTurnWarnings[5] ? num3 : 0);
			goto IL_03cb;
			IL_03cb:
			return (byte)num5 != 0;
		}
	}

	private static string GetDeploymentEndConsentAskLocalizedText(int levelUpLeft, int perksPointsLeft)
	{
		return string.Empty;
	}

	private static string GetProductionEndConsentAskLocalizedText(bool goldLeft, bool materialsLeft, bool workersLeft, int freeActionsLeft, int levelUpLeft, int perksPointsLeft)
	{
		string empty = string.Empty;
		empty = empty + Localizer.Get("TurnEnd_ConsentAsk_Production") + "\n\n";
		if (goldLeft)
		{
			empty += string.Format("\t- {0}\n", Localizer.Format("TurnEnd_ConsentAsk_GoldThresholdTriggered", 50));
		}
		if (materialsLeft)
		{
			empty += string.Format("\t- {0}\n", Localizer.Format("TurnEnd_ConsentAsk_MaterialsThresholdTriggered", 50));
		}
		if (workersLeft)
		{
			empty += string.Format("\t- {0}\n", Localizer.Format("TurnEnd_ConsentAsk_WorkersLeft", TPSingleton<ResourceManager>.Instance.Workers));
		}
		if (freeActionsLeft > 0)
		{
			empty += string.Format("\t- {0}\n", Localizer.Format("TurnEnd_ConsentAsk_FreeActionsLeft", freeActionsLeft));
		}
		if (levelUpLeft > 0)
		{
			empty += string.Format("\t- {0}\n", Localizer.Format("TurnEnd_ConsentAsk_LevelUpLeft", levelUpLeft));
		}
		if (perksPointsLeft > 0)
		{
			empty += string.Format("\t- {0}\n", Localizer.Format("TurnEnd_ConsentAsk_PerksPointsLeft", perksPointsLeft));
		}
		return empty + "\n" + Localizer.Get("TurnEnd_ConsentAsk_AreYouSure");
	}

	private static string GetNightTurnEndConsentAskLocalizedText(float actionPointsLeft, int notMovedHeroes)
	{
		string empty = string.Empty;
		empty = empty + Localizer.Get("TurnEnd_ConsentAsk_Night") + "\n\n";
		if (actionPointsLeft > 0f)
		{
			empty += string.Format("\t- {0}\n", Localizer.Format("TurnEnd_ConsentAsk_ActionPointsLeft", actionPointsLeft));
		}
		if (notMovedHeroes > 0)
		{
			empty += string.Format("\t- {0}\n", Localizer.Format("TurnEnd_ConsentAsk_AnyHeroDidNotMove", notMovedHeroes));
		}
		return empty + "\n" + Localizer.Get("TurnEnd_ConsentAsk_AreYouSure");
	}

	[DevConsoleCommand("EndTurnChecksByPass")]
	public static void DebugByPassEndTurnChecks(bool state = true)
	{
		debugByPassEndTurnChecks = state;
	}
}
