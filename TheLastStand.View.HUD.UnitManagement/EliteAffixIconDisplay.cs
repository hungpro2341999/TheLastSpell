using TPLib;
using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model.Unit.Enemy.Affix;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.UnitManagement;

public class EliteAffixIconDisplay : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public class Constants
	{
		public const string IconPathFormat = "View/Sprites/UI/Units/EnemiesAffixes/Icons/EnemyAffix_Icon_{0}";

		public const string BackgroundPathFormat = "View/Sprites/UI/Units/EnemiesAffixes/Backgrounds/EnemyAffix_Box_{0}";

		public const string HoverPathFormat = "View/Sprites/UI/Units/EnemiesAffixes/Backgrounds/EnemyAffix_Box_{0}_Hovered";

		private const string GlobalPath = "View/Sprites/UI/Units/EnemiesAffixes/";
	}

	[SerializeField]
	private Image enemyAffixBackground;

	[SerializeField]
	private Image enemyAffixIcon;

	[SerializeField]
	private Image hover;

	[SerializeField]
	private JoystickSelectable joystickSelectable;

	private EnemyAffix affix;

	public JoystickSelectable JoystickSelectable => joystickSelectable;

	public void Display(EnemyAffix newAffix, EnemyAffixEffectDefinition.E_EnemyAffixBoxType boxType)
	{
		affix = newAffix;
		enemyAffixIcon.sprite = ResourcePooler.LoadOnce<Sprite>($"View/Sprites/UI/Units/EnemiesAffixes/Icons/EnemyAffix_Icon_{affix.EnemyAffixDefinition.EnemyAffixEffectDefinition.EnemyAffixEffect.ToString()}");
		enemyAffixBackground.sprite = ResourcePooler.LoadOnce<Sprite>($"View/Sprites/UI/Units/EnemiesAffixes/Backgrounds/EnemyAffix_Box_{boxType.ToString()}");
		hover.sprite = ResourcePooler.LoadOnce<Sprite>($"View/Sprites/UI/Units/EnemiesAffixes/Backgrounds/EnemyAffix_Box_{boxType.ToString()}_Hovered");
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
		hover.enabled = false;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		hover.enabled = true;
		UIManager.EliteAffixTooltip.Affix = affix;
		UIManager.EliteAffixTooltip.FollowElement.ChangeTarget(base.transform);
		UIManager.EliteAffixTooltip.Display();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		hover.enabled = false;
		UIManager.EliteAffixTooltip.Hide();
	}

	public void OnJoystickSelect()
	{
		hover.enabled = true;
		if (TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
		{
			UIManager.EliteAffixTooltip.Affix = affix;
			UIManager.EliteAffixTooltip.FollowElement.ChangeTarget(base.transform);
			UIManager.EliteAffixTooltip.Display();
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
			UIManager.EliteAffixTooltip.Affix = affix;
			UIManager.EliteAffixTooltip.FollowElement.ChangeTarget(base.transform);
			UIManager.EliteAffixTooltip.Display();
		}
		else
		{
			UIManager.GenericTooltip.Hide();
		}
	}
}
