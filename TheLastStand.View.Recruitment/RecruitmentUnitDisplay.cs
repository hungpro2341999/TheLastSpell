using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TheLastStand.Controller.Unit;
using TheLastStand.Manager;
using TheLastStand.Model.Unit;
using TheLastStand.View.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Recruitment;

public class RecruitmentUnitDisplay : RecruitDisplay
{
	[SerializeField]
	private DataColor buyableColor;

	[SerializeField]
	private DataColor unbuyableColor;

	[SerializeField]
	private UnitPortraitView unitPortraitView;

	[SerializeField]
	private TextMeshProUGUI unitName;

	[SerializeField]
	private Toggle unitToggle;

	[SerializeField]
	private TextMeshProUGUI goldValue;

	[SerializeField]
	private List<LocalizedFont> localizedFonts;

	public List<LocalizedFont> LocalizedFonts => localizedFonts;

	public override Toggle Toggle => unitToggle;

	public PlayableUnit Unit { get; private set; }

	public void Refresh(PlayableUnit newUnit = null)
	{
		Unit = newUnit;
		base.gameObject.SetActive(newUnit != null);
		if (newUnit != null)
		{
			unitPortraitView.PlayableUnit = newUnit;
			unitPortraitView.PlayableUnit.PlayableUnitView.Unit = unitPortraitView.PlayableUnit;
			unitPortraitView.PlayableUnit.PlayableUnitView.InitVisuals(playSpawnAnim: false);
			unitPortraitView.RefreshPortrait();
			unitName.text = ((Unit != null) ? newUnit.Name : string.Empty);
			int num = RecruitmentController.ComputeUnitCost(Unit);
			goldValue.text = ((Unit != null) ? $"{num}" : string.Empty);
			RefreshToggleDisplay(Unit != null && num <= TPSingleton<ResourceManager>.Instance.Gold && newUnit != null);
		}
	}

	public void RefreshToggleDisplay(bool buyable)
	{
		goldValue.color = (buyable ? buyableColor._Color : unbuyableColor._Color);
	}
}
