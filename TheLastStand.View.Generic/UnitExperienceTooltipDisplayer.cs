using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Unit;
using TheLastStand.View.Unit;
using UnityEngine;

namespace TheLastStand.View.Generic;

public class UnitExperienceTooltipDisplayer : GenericTitledTooltipDisplayer
{
	[SerializeField]
	private string levelUpNoticeLocaKey = string.Empty;

	[SerializeField]
	private Vector3 followDatasOffset = Vector3.zero;

	public PlayableUnit PlayableUnit { get; set; }

	public void Refresh()
	{
		DisplayTooltip();
	}

	public override void DisplayTooltip()
	{
		PlayableUnit playableUnit = PlayableUnit ?? TileObjectSelectionManager.SelectedPlayableUnit;
		GenericTitledTooltip genericTitledTooltip = (targetTooltip as GenericTitledTooltip) ?? UIManager.UnitExperienceTooltip;
		if (playableUnit == null)
		{
			genericTitledTooltip.Hide();
			return;
		}
		string title = Localizer.Format(titleLocaKey, Mathf.FloorToInt(playableUnit.ExperienceInCurrentLevel), Mathf.CeilToInt(playableUnit.ExperienceNeededToNextLevel));
		genericTitledTooltip.SetContentLocalized(title, GetLocalizedDescription(playableUnit), icon);
		if (overrideTitleColor)
		{
			genericTitledTooltip.SetTitleColor(titleColor);
		}
		genericTitledTooltip.FollowElement.ChangeOffset(followDatasOffset);
		genericTitledTooltip.FollowElement.ChangeTarget(base.transform);
		genericTitledTooltip.Display();
	}

	public override void HideTooltip()
	{
		_ = PlayableUnit ?? TileObjectSelectionManager.SelectedPlayableUnit;
		GenericTitledTooltip genericTitledTooltip = (targetTooltip as GenericTitledTooltip) ?? UIManager.UnitExperienceTooltip;
		genericTitledTooltip.Hide();
	}

	private void Awake()
	{
		TileObjectSelectionManager.OnUnitSelectionChange += OnUnitSelectionChange;
	}

	private string GetLocalizedDescription(PlayableUnit unit)
	{
		string text = Localizer.Get(descriptionLocaKey);
		if (unit.StatsPoints > 0 && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.NightReport && !TPSingleton<UnitLevelUpView>.Instance.IsOpened)
		{
			text = text + "\n\n" + Localizer.Get(levelUpNoticeLocaKey);
		}
		return text;
	}

	private void OnDestroy()
	{
		TileObjectSelectionManager.OnUnitSelectionChange -= OnUnitSelectionChange;
	}

	private void OnUnitSelectionChange()
	{
		PlayableUnit playableUnit = PlayableUnit ?? TileObjectSelectionManager.SelectedPlayableUnit;
		GenericTitledTooltip genericTitledTooltip = (targetTooltip as GenericTitledTooltip) ?? UIManager.UnitExperienceTooltip;
		if (playableUnit == null)
		{
			HideTooltip();
		}
		else if (base.HasFocus)
		{
			if (genericTitledTooltip.Displayed)
			{
				string title = Localizer.Format(titleLocaKey, Mathf.FloorToInt(playableUnit.ExperienceInCurrentLevel), Mathf.CeilToInt(playableUnit.ExperienceNeededToNextLevel));
				genericTitledTooltip.SetContentLocalized(title, GetLocalizedDescription(playableUnit), icon);
				genericTitledTooltip.Refresh();
			}
			else
			{
				DisplayTooltip();
			}
		}
	}
}
