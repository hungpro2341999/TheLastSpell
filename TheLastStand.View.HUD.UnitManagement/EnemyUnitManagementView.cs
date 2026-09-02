using System;
using System.Collections;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Skill.UI;
using TheLastStand.View.Unit;
using TheLastStand.View.Unit.Stat;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.UnitManagement;

public class EnemyUnitManagementView : UnitManagementView<EnemyUnitManagementView>
{
	[SerializeField]
	protected UnitStatDisplay healthDisplay;

	[SerializeField]
	protected UnitStatDisplay stunResistDisplay;

	[SerializeField]
	private EnemyPortraitView unitPortraitView;

	[SerializeField]
	protected Image healthGaugeImage;

	[SerializeField]
	protected Sprite healthGaugeBaseSprite;

	[SerializeField]
	protected Sprite healthGaugeInvincibleSprite;

	[SerializeField]
	private TextMeshProUGUI enemyUnitDescriptionText;

	[SerializeField]
	private Canvas enemyUnitDescriptionCanvas;

	[SerializeField]
	private Image unitInfoBackgroundImageTop;

	[SerializeField]
	private Image unitInfoBackgroundImageArmor;

	[SerializeField]
	private Image unitInfoBackgroundImageBottom;

	[SerializeField]
	private Sprite unitInfoBackgroundImageBaseTop;

	[SerializeField]
	private Sprite unitInfoBackgroundImageBaseArmor;

	[SerializeField]
	private Sprite unitInfoBackgroundImageBaseBottom;

	[SerializeField]
	private Sprite unitInfoBackgroundImageEliteTop;

	[SerializeField]
	private Sprite unitInfoBackgroundImageEliteArmor;

	[SerializeField]
	private Sprite unitInfoBackgroundImageEliteBottom;

	public override void Init()
	{
		base.Init();
		healthDisplay.Init();
		stunResistDisplay.Init();
		healthDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Health];
		healthDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.HealthTotal];
		stunResistDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.StunResistance];
	}

	protected override void Awake()
	{
		base.Awake();
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	protected override void RefreshView()
	{
		if (TileObjectSelectionManager.SelectedUnit == null || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Shopping || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.NightReport || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.ProductionReport || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CharacterSheet || TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			UnitManagementView<EnemyUnitManagementView>.DisplayCanvas(display: false);
			if (simpleFontLocalizedParent != null)
			{
				simpleFontLocalizedParent.UnregisterChildren();
			}
			return;
		}
		base.RefreshView();
		EnemyUnit selectedEnemyUnit = TileObjectSelectionManager.SelectedEnemyUnit;
		healthDisplay.Refresh();
		healthGaugeImage.sprite = (selectedEnemyUnit.IsInvulnerable ? healthGaugeInvincibleSprite : healthGaugeBaseSprite);
		stunResistDisplay.Refresh();
		((EnemySkillBar)TPSingleton<EnemyUnitManagementView>.Instance.skillBar).Refresh();
		unitPortraitView.EnemyUnit = selectedEnemyUnit;
		unitPortraitView.RefreshPortrait();
		unitName.text = selectedEnemyUnit.Name;
		injuriesDisplay.Refresh(selectedEnemyUnit);
		bool flag = selectedEnemyUnit is EliteEnemyUnit;
		unitInfoBackgroundImageTop.sprite = (flag ? unitInfoBackgroundImageEliteTop : unitInfoBackgroundImageBaseTop);
		unitInfoBackgroundImageArmor.sprite = (flag ? unitInfoBackgroundImageEliteArmor : unitInfoBackgroundImageBaseArmor);
		unitInfoBackgroundImageBottom.sprite = (flag ? unitInfoBackgroundImageEliteBottom : unitInfoBackgroundImageBaseBottom);
		unitInfoBackgroundImageTop.SetNativeSize();
		unitInfoBackgroundImageArmor.SetNativeSize();
		unitInfoBackgroundImageBottom.SetNativeSize();
		RefreshLocalizedTexts();
		StartCoroutine(ForceUpdateDescriptionCanvas());
	}

	private IEnumerator ForceUpdateDescriptionCanvas()
	{
		enemyUnitDescriptionCanvas.sortingOrder++;
		yield return null;
		enemyUnitDescriptionCanvas.sortingOrder--;
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		if (canvas.enabled)
		{
			RefreshLocalizedTexts();
		}
	}

	private void RefreshLocalizedTexts()
	{
		EnemyUnit selectedEnemyUnit = TileObjectSelectionManager.SelectedEnemyUnit;
		unitName.text = selectedEnemyUnit.Name;
		enemyUnitDescriptionText.text = selectedEnemyUnit.Description;
	}
}
