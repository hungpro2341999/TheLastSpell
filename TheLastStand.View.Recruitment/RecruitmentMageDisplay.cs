using TMPro;
using TPLib;
using TheLastStand.Controller.Unit;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Recruitment;

public class RecruitmentMageDisplay : RecruitDisplay
{
	[SerializeField]
	private TextMeshProUGUI mageTitle;

	[SerializeField]
	private TextMeshProUGUI noMageTitle;

	[SerializeField]
	private TextMeshProUGUI noMageSubtitle;

	[SerializeField]
	private Image costIcon;

	[SerializeField]
	private TextMeshProUGUI costText;

	[SerializeField]
	private Toggle mageToggle;

	[SerializeField]
	private Image mageImage;

	[SerializeField]
	private Sprite mageNormalAvailableSprite;

	[SerializeField]
	private Sprite mageNormalUnavailableSprite;

	[SerializeField]
	private Sprite mageHoverAvailableSprite;

	[SerializeField]
	private Sprite mageHoverUnavailableSprite;

	public override Toggle Toggle => mageToggle;

	public void Refresh(bool hasMage)
	{
		SpriteState spriteState = mageToggle.spriteState;
		spriteState.highlightedSprite = (hasMage ? mageHoverAvailableSprite : mageHoverUnavailableSprite);
		mageToggle.spriteState = spriteState;
		mageImage.sprite = (hasMage ? mageNormalAvailableSprite : mageNormalUnavailableSprite);
		mageTitle.enabled = hasMage;
		noMageTitle.enabled = !hasMage;
		noMageSubtitle.enabled = !hasMage;
		costText.text = (hasMage ? $"{RecruitmentController.ComputeMageCost()}" : string.Empty);
		costIcon.enabled = hasMage;
		RefreshButtonDisplay();
	}

	public void RefreshButtonDisplay()
	{
		costText.color = ((RecruitmentController.ComputeMageCost() <= TPSingleton<ResourceManager>.Instance.Gold) ? Color.white : Color.red);
	}

	protected override void OnUnitToggleValueChanged(bool value)
	{
		if (InputManager.IsLastControllerJoystick)
		{
			base.OnUnitToggleValueChanged(value);
			if (value)
			{
				TPSingleton<RecruitmentView>.Instance.SelectRecruitButton();
			}
			else
			{
				RecruitmentView.WarningTooltip.Hide();
			}
		}
	}
}
