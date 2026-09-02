using TPLib;
using TheLastStand.Definition.Meta.Glyphs;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Sound;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.Glyphs;

public abstract class AGlyphDisplay : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	private static class Constants
	{
		public const string GlyphIconsPath = "View/Sprites/UI/Meta/Glyphs/Orbs/Glyphs_Orbs_";

		public const string GlyphIconDefault = "Default";
	}

	[SerializeField]
	private FollowElement.FollowDatas followDatas;

	[SerializeField]
	protected Image glyphIcon;

	[SerializeField]
	private Image hoveredFeedback;

	[SerializeField]
	protected Image customModeFeedback;

	[SerializeField]
	protected JoystickSelectable joystickSelectable;

	[SerializeField]
	protected AudioClip hoverClip;

	public GlyphDefinition GlyphDefinition { get; private set; }

	public JoystickSelectable JoystickSelectable => joystickSelectable;

	public bool JoystickSelected { get; private set; }

	protected virtual bool CanDisplayTooltip => true;

	public static Sprite GetGlyphIcon(GlyphDefinition glyphDefinition)
	{
		Sprite sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Meta/Glyphs/Orbs/Glyphs_Orbs_" + glyphDefinition.Id, failSilently: true);
		if (!sprite)
		{
			return ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Meta/Glyphs/Orbs/Glyphs_Orbs_Default");
		}
		return sprite;
	}

	public virtual void Init(GlyphDefinition glyphDefinition)
	{
		GlyphDefinition = glyphDefinition;
		glyphIcon.sprite = GetGlyphIcon(glyphDefinition);
		customModeFeedback.enabled = glyphDefinition.IsCustom;
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		if (CanDisplayTooltip && (!InputManager.IsLastControllerJoystick || TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips))
		{
			ShowTooltip();
		}
		hoveredFeedback.enabled = true;
		SoundManager.PlayAudioClip(TPSingleton<GlyphSelectionPanel>.Instance.GetNextAudioSource(), hoverClip);
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		hoveredFeedback.enabled = false;
		TPSingleton<GlyphManager>.Instance.GlyphTooltip.Hide();
	}

	public virtual void OnSelect(BaseEventData eventData)
	{
		OnPointerEnter(null);
		JoystickSelected = true;
		HUDJoystickNavigationManager.TooltipsToggled += OnTooltipsToggled;
	}

	public virtual void OnDeselect(BaseEventData eventData)
	{
		OnPointerExit(null);
		JoystickSelected = false;
		HUDJoystickNavigationManager.TooltipsToggled -= OnTooltipsToggled;
	}

	private void OnTooltipsToggled(bool showTooltips)
	{
		if (CanDisplayTooltip && showTooltips && JoystickSelected)
		{
			ShowTooltip();
		}
		else
		{
			TPSingleton<GlyphManager>.Instance.GlyphTooltip.Hide();
		}
	}

	private void ShowTooltip()
	{
		TPSingleton<GlyphManager>.Instance.GlyphTooltip.Init(GlyphDefinition);
		TPSingleton<GlyphManager>.Instance.GlyphTooltip.FollowElement.ChangeFollowDatas(followDatas);
		TPSingleton<GlyphManager>.Instance.GlyphTooltip.Display();
	}
}
