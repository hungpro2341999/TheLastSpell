using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TPLib;
using TPLib.Localization.Fonts;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Unit.Enemy;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Seer;

public class SeerPreviewDisplay : TPSingleton<SeerPreviewDisplay>
{
	[SerializeField]
	private Canvas previewCanvas;

	[SerializeField]
	private GraphicRaycaster graphicRaycaster;

	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	[SerializeField]
	private RectTransform titleRectTransform;

	[SerializeField]
	private RectTransform portraitsContainerRectTransform;

	[SerializeField]
	private Selectable foldButton;

	[SerializeField]
	private ScrollRect previewScrollRect;

	[SerializeField]
	private RectTransform contentRectTransform;

	[SerializeField]
	private RectTransform scrollViewRectTransform;

	[SerializeField]
	private Selectable scrollTopButton;

	[SerializeField]
	private Selectable scrollBotButton;

	[SerializeField]
	[Range(0f, 1000f)]
	private float scrollButtonsSensitivity = 100f;

	[SerializeField]
	private RectTransform foldButtonRectTransform;

	[SerializeField]
	private float foldedTitlePosition = 200f;

	[SerializeField]
	[Range(0f, 2f)]
	private float unfoldTitleDuration = 0.4f;

	[SerializeField]
	private Ease unfoldTitleEasing = Ease.InCubic;

	[SerializeField]
	[Range(0f, 2f)]
	private float foldTitleDuration = 0.4f;

	[SerializeField]
	private Ease foldTitleEasing = Ease.OutCubic;

	[SerializeField]
	private float foldedPosition = 200f;

	[SerializeField]
	private float foldedWithQuantitiesPosition = 250f;

	[SerializeField]
	private float unfoldedPosition = 200f;

	[SerializeField]
	[Range(0f, 2f)]
	private float unfoldDuration = 0.4f;

	[SerializeField]
	private Ease unfoldEasing = Ease.InCubic;

	[SerializeField]
	[Range(0f, 2f)]
	private float foldDuration = 0.4f;

	[SerializeField]
	private Ease foldEasing = Ease.OutCubic;

	[SerializeField]
	private SeerEnemyPortraitPreview portraitPrefab;

	[SerializeField]
	private List<SeerEnemyPortraitPreview> portraits;

	private bool canScroll;

	private bool displayed;

	private readonly List<EnemyUnitTemplateDefinition> enemyDefinitions = new List<EnemyUnitTemplateDefinition>();

	private readonly Dictionary<string, SeerAdditionalPortraitSettings> seerAdditionalPortraits = new Dictionary<string, SeerAdditionalPortraitSettings>();

	private bool isFolded;

	private Sequence previewFoldSequence;

	public bool Displayed
	{
		get
		{
			return displayed;
		}
		set
		{
			displayed = value;
			previewCanvas.enabled = displayed;
			if (displayed)
			{
				simpleFontLocalizedParent?.RegisterChildren();
			}
			else
			{
				simpleFontLocalizedParent?.UnregisterChildren();
			}
		}
	}

	public void DisplayEnemyPortraits()
	{
		enemyDefinitions.Clear();
		seerAdditionalPortraits.Clear();
		SpawnWave currentSpawnWave = SpawnWaveManager.CurrentSpawnWave;
		if (currentSpawnWave.SpawnWaveDefinition.WaveEnemiesDefinition.SeerAdditionalPortraitsSettings.Count > 0)
		{
			for (int i = 0; i < currentSpawnWave.SpawnWaveDefinition.WaveEnemiesDefinition.SeerAdditionalPortraitsSettings.Count; i++)
			{
				SeerAdditionalPortraitSettings seerAdditionalPortraitSettings = currentSpawnWave.SpawnWaveDefinition.WaveEnemiesDefinition.SeerAdditionalPortraitsSettings[i];
				if (seerAdditionalPortraitSettings.PortraitType == DamageableType.Enemy)
				{
					enemyDefinitions.Add(EnemyUnitDatabase.EnemyUnitTemplateDefinitions[seerAdditionalPortraitSettings.PortraitTemplateId]);
				}
				seerAdditionalPortraits.Add(seerAdditionalPortraitSettings.PortraitTemplateId, seerAdditionalPortraitSettings);
			}
		}
		if (SpawnWaveManager.AliveSeer)
		{
			for (int num = currentSpawnWave.RemainingEnemiesToSpawn.Count - 1; num >= 0; num--)
			{
				_ = EnemyUnitDatabase.EnemyUnitTemplateDefinitions[currentSpawnWave.RemainingEnemiesToSpawn[num].Id];
				if (!enemyDefinitions.Contains(currentSpawnWave.RemainingEnemiesToSpawn[num]))
				{
					enemyDefinitions.Add(currentSpawnWave.RemainingEnemiesToSpawn[num]);
				}
			}
			for (int num2 = currentSpawnWave.RemainingEliteEnemiesToSpawn.Count - 1; num2 >= 0; num2--)
			{
				EnemyUnitTemplateDefinition item = EnemyUnitDatabase.EnemyUnitTemplateDefinitions[currentSpawnWave.RemainingEliteEnemiesToSpawn[num2].Id];
				if (!enemyDefinitions.Contains(item))
				{
					enemyDefinitions.Add(item);
				}
			}
		}
		enemyDefinitions.Sort((EnemyUnitTemplateDefinition enemyA, EnemyUnitTemplateDefinition enemyB) => enemyA.Tier.CompareTo(enemyB.Tier));
		if (currentSpawnWave.SpawnWaveDefinition.IsBossWave && currentSpawnWave.SpawnWaveDefinition.DisplayBossInSeer)
		{
			enemyDefinitions.Insert(0, BossUnitDatabase.BossUnitTemplateDefinitions[currentSpawnWave.SpawnWaveDefinition.WaveEnemiesDefinition.BossWaveSettings.BossUnitTemplateId]);
		}
		int num3 = 0;
		InstantiatePortraitsIfNeeded(enemyDefinitions.Count);
		for (; num3 < enemyDefinitions.Count; num3++)
		{
			SeerEnemyPortraitPreview seerEnemyPortraitPreview = portraits[num3];
			bool forceHideQuantity = false;
			EnemyUnitTemplateDefinition enemyDefinition = enemyDefinitions[num3];
			bool flag = enemyDefinition is BossUnitTemplateDefinition;
			bool flag2 = enemyDefinition.Tier == 1 || SpawnWaveManager.DisplayAllEnemyTiers || flag;
			bool display = flag2 && SpawnWaveManager.DisplayQuantities && !flag;
			seerEnemyPortraitPreview.SetEnemyInfo(enemyDefinition, !flag2, flag);
			int num4 = ((!currentSpawnWave.SpawnWaveDefinition.IsBossWave || !currentSpawnWave.SpawnWaveDefinition.IsInfinite) ? (currentSpawnWave.RemainingEnemiesToSpawn.Count((EnemyUnitTemplateDefinition o) => o.Id == enemyDefinition.Id) + currentSpawnWave.RemainingEliteEnemiesToSpawn.Count((EliteEnemyUnitTemplateDefinition o) => o.Id == enemyDefinition.Id)) : (-1));
			if (seerAdditionalPortraits.ContainsKey(enemyDefinition.Id))
			{
				if (num4 != -1)
				{
					num4 += seerAdditionalPortraits[enemyDefinition.Id].PortraitAmount;
				}
				forceHideQuantity = !seerAdditionalPortraits[enemyDefinition.Id].DisplayPortraitAmount && num4 == 0;
			}
			seerEnemyPortraitPreview.SetEnemyQuantity(num4, display, forceHideQuantity);
			seerEnemyPortraitPreview.Display(show: true);
		}
		for (; num3 < portraits.Count; num3++)
		{
			portraits[num3].Display(show: false);
		}
		RefreshPanel();
	}

	public void DisplayQuantitiesOnRevealedEnemies()
	{
		for (int num = portraits.Count - 1; num >= 0; num--)
		{
			SeerEnemyPortraitPreview seerEnemyPortraitPreview = portraits[num];
			seerEnemyPortraitPreview.QuantityDisplayed = seerEnemyPortraitPreview.Displayed && !seerEnemyPortraitPreview.IsBossEnemy && !seerEnemyPortraitPreview.HiddenEnemy && !seerEnemyPortraitPreview.ForceHideQuantity;
		}
	}

	public void OnBotButtonClick()
	{
		previewScrollRect.verticalScrollbar.value = Mathf.Clamp01(previewScrollRect.verticalScrollbar.value - scrollButtonsSensitivity / contentRectTransform.sizeDelta.y);
	}

	public void OnFoldButtonClick()
	{
		if (isFolded)
		{
			Unfold();
		}
		else
		{
			Fold();
		}
	}

	public void OnTopButtonClick()
	{
		previewScrollRect.verticalScrollbar.value = Mathf.Clamp01(previewScrollRect.verticalScrollbar.value + scrollButtonsSensitivity / contentRectTransform.sizeDelta.y);
	}

	protected override void Awake()
	{
		TPSingleton<SettingsManager>.Instance.OnResolutionChangeEvent += OnResolutionChange;
	}

	private void Fold()
	{
		if (!isFolded)
		{
			float endValue = (portraits.Any((SeerEnemyPortraitPreview o) => o.QuantityDisplayed) ? foldedWithQuantitiesPosition : foldedPosition);
			previewFoldSequence?.Kill();
			previewFoldSequence = DOTween.Sequence();
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
			previewFoldSequence.Join(portraitsContainerRectTransform.DOAnchorPosX(endValue, foldDuration).SetEase(foldEasing));
			previewFoldSequence.Join(titleRectTransform.DOAnchorPosX(foldedTitlePosition, foldTitleDuration).SetEase(foldTitleEasing)).SetFullId("SeerPreviewFoldTween", this).OnComplete(delegate
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
			});
			isFolded = true;
			foldButtonRectTransform.localScale = new Vector3(1f, 1f, 1f);
		}
	}

	private void Unfold()
	{
		if (isFolded)
		{
			previewFoldSequence?.Kill();
			previewFoldSequence = DOTween.Sequence();
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
			previewFoldSequence.Join(portraitsContainerRectTransform.DOAnchorPosX(unfoldedPosition, unfoldDuration).SetEase(unfoldEasing));
			previewFoldSequence.Join(titleRectTransform.DOAnchorPosX(0f, unfoldTitleDuration).SetEase(unfoldTitleEasing)).SetFullId("SeerPreviewFoldTween", this).OnComplete(delegate
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
			});
			foldButtonRectTransform.localScale = new Vector3(-1f, 1f, 1f);
			isFolded = false;
		}
	}

	private void InstantiatePortraitsIfNeeded(int count)
	{
		int num = count - portraits.Count;
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				SeerEnemyPortraitPreview item = Object.Instantiate(portraitPrefab, contentRectTransform);
				portraits.Add(item);
			}
		}
	}

	private void OnDestroy()
	{
		if (TPSingleton<SettingsManager>.Exist())
		{
			TPSingleton<SettingsManager>.Instance.OnResolutionChangeEvent -= OnResolutionChange;
		}
	}

	private void OnResolutionChange(Resolution resolution)
	{
		RefreshPanel();
	}

	private void RefreshPanel()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(contentRectTransform);
		canScroll = contentRectTransform.rect.height > portraitsContainerRectTransform.rect.height - Mathf.Abs(scrollViewRectTransform.offsetMax.y) - Mathf.Abs(scrollViewRectTransform.offsetMin.y);
		previewScrollRect.vertical = canScroll;
		scrollTopButton.gameObject.SetActive(canScroll);
		scrollBotButton.gameObject.SetActive(canScroll);
		if (!canScroll)
		{
			previewScrollRect.verticalScrollbar.value = 1f;
		}
		foldButton.SetSelectOnDown(canScroll ? scrollTopButton : null);
	}
}
