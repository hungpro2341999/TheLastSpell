using TPLib;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using UnityEngine;

namespace TheLastStand.View.Building.UI;

public class MagicCircleHUD : BuildingHUD
{
	[SerializeField]
	private ProductionPanel productionPanelMagicCircle;

	public MagicCircle MagicCircle => Building as MagicCircle;

	public ProductionPanel ProductionPanelMagicCircle => productionPanelMagicCircle;

	public override TheLastStand.Model.Building.Building Building
	{
		get
		{
			return base.Building;
		}
		set
		{
			if (value != null)
			{
				base.Building = value;
				ProductionPanelMagicCircle.Init();
				ProductionPanelMagicCircle.gameObject.SetActive(value: true);
				ProductionPanelMagicCircle.AnimateUnitsIncrement();
			}
		}
	}

	protected override void DisplayProductionIfNeeded()
	{
		ProductionPanelMagicCircle.DisplayIfNeeded();
	}

	protected override float GetHealthTotal()
	{
		if (MagicCircle == null)
		{
			Debug.LogError(Building?.Id);
			return -1f;
		}
		return MagicCircle.CurrentHealthTotal;
	}

	protected override bool ShouldHealthBeDisplayed()
	{
		if (MagicCircle.CurrentHealthTotal > 0f && (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Construction || MagicCircle.DamageableModule.DamageableModuleDefinition.KeepHUDDisplayedWhenDamaged) && MagicCircle.DamageableModule.Health < MagicCircle.CurrentHealthTotal && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.CutscenePlaying)
		{
			return TPSingleton<GameManager>.Instance.Game.State != Game.E_State.GameOver;
		}
		return false;
	}
}
