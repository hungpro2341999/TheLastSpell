using TMPro;
using TPLib.Localization.Fonts;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.View.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Seer;

public class SeerEnemyPortraitPreview : MonoBehaviour
{
	[SerializeField]
	private Image portraitImage;

	[SerializeField]
	[Tooltip("This can be set to null in case we do NOT want to hide boss portraits and use their own portrait.")]
	private Sprite bossPortrait;

	[SerializeField]
	private GameObject quantityPanel;

	[SerializeField]
	private TextMeshProUGUI quantityText;

	[SerializeField]
	private GameObject infiniteQuantityIcon;

	[SerializeField]
	private LocalizedFont quantityLocalizedFont;

	private Sprite enemyPortraitSprite;

	private Sprite hiddenPortraitSprite;

	private int quantity;

	private bool displayed;

	private bool quantityDisplayed;

	public bool Displayed
	{
		get
		{
			return displayed;
		}
		private set
		{
			displayed = value;
			base.gameObject.SetActive(displayed);
			if (!displayed)
			{
				QuantityDisplayed = false;
			}
		}
	}

	public bool HiddenEnemy { get; private set; }

	public bool IsBossEnemy { get; private set; }

	public bool QuantityDisplayed
	{
		get
		{
			return quantityDisplayed;
		}
		set
		{
			quantityDisplayed = value;
			quantityPanel.SetActive(quantityDisplayed);
			if (quantityLocalizedFont != null)
			{
				quantityLocalizedFont.Subscribe(quantityDisplayed);
			}
		}
	}

	public bool ForceHideQuantity { get; private set; }

	public void Display(bool show)
	{
		Displayed = show;
	}

	public void SetEnemyInfo(EnemyUnitTemplateDefinition enemyDefinition, bool hidden, bool isBossUnit)
	{
		IsBossEnemy = isBossUnit;
		HiddenEnemy = hidden;
		enemyPortraitSprite = EnemyUnitView.GetUiSprite(enemyDefinition.AssetsId);
		portraitImage.sprite = (HiddenEnemy ? hiddenPortraitSprite : ((IsBossEnemy && bossPortrait != null) ? bossPortrait : enemyPortraitSprite));
	}

	public void SetEnemyQuantity(int enemyQuantity, bool display, bool forceHideQuantity)
	{
		quantity = enemyQuantity;
		ForceHideQuantity = forceHideQuantity;
		infiniteQuantityIcon.SetActive(enemyQuantity == -1);
		quantityText.gameObject.SetActive(enemyQuantity > -1);
		if (enemyQuantity > -1)
		{
			quantityText.text = quantity.ToString();
			if (quantityLocalizedFont != null)
			{
				quantityLocalizedFont.Subscribe(quantityDisplayed);
			}
		}
		QuantityDisplayed = display;
	}

	private void Awake()
	{
		hiddenPortraitSprite = EnemyUnitView.GetHiddenEnemySprite();
	}
}
