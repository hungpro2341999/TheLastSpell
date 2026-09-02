using TPLib;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.UnitManagement;

public class GuardianIconDisplay : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private class Constants
	{
		public const string GuardianTooltipBrazier = "GuardianTooltip_Brazier";

		public const string GuardianTooltipBoss = "GuardianTooltip_Boss";

		private const string GuardianTooltipPrefix = "GuardianTooltip_";

		private const string GuardianTooltipBrazierSuffix = "Brazier";

		private const string GuardianTooltipBossSuffix = "Boss";
	}

	[SerializeField]
	private Image image;

	[SerializeField]
	private Image hoverImage;

	[SerializeField]
	private GameObject hover;

	[SerializeField]
	private Sprite defaultImage;

	[SerializeField]
	private Sprite defaultHoverImage;

	[SerializeField]
	private Sprite bossImage;

	[SerializeField]
	private Sprite bossHoverImage;

	[SerializeField]
	private JoystickSelectable joystickSelectable;

	private bool displayBossAssets;

	public JoystickSelectable JoystickSelectable => joystickSelectable;

	public void Display(bool isBossGuardian)
	{
		if (displayBossAssets != isBossGuardian)
		{
			displayBossAssets = isBossGuardian;
			image.sprite = (displayBossAssets ? bossImage : defaultImage);
			hoverImage.sprite = (displayBossAssets ? bossHoverImage : defaultHoverImage);
		}
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
		hover.SetActive(value: false);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		hover.SetActive(value: true);
		UIManager.GenericTooltip.SetContent(displayBossAssets ? "GuardianTooltip_Boss" : "GuardianTooltip_Brazier", null);
		UIManager.GenericTooltip.Display();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		hover.SetActive(value: false);
		UIManager.GenericTooltip.Hide();
	}

	public void OnJoystickSelect()
	{
		hover.SetActive(value: true);
		if (TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
		{
			UIManager.GenericTooltip.SetContent(displayBossAssets ? "GuardianTooltip_Boss" : "GuardianTooltip_Brazier", null);
			UIManager.GenericTooltip.Display();
		}
	}

	public void OnJoystickDeselect()
	{
		OnPointerExit(null);
	}

	public void OnTooltipsToggled(bool showTooltips)
	{
		if (showTooltips)
		{
			UIManager.GenericTooltip.SetContent(displayBossAssets ? "GuardianTooltip_Boss" : "GuardianTooltip_Brazier", null);
			UIManager.GenericTooltip.Display();
		}
		else
		{
			UIManager.GenericTooltip.Hide();
		}
	}
}
