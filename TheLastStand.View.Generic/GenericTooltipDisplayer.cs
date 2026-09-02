using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.Generic;

public class GenericTooltipDisplayer : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	protected string locaKey;

	[SerializeField]
	private string rewiredActionKey;

	[Tooltip("If null, will use the one defined in Manager.UIManager.genericTooltip")]
	[SerializeField]
	protected GenericTooltip targetTooltip;

	protected bool HasFocus { get; set; }

	public string LocaKey
	{
		get
		{
			return locaKey;
		}
		set
		{
			locaKey = value;
		}
	}

	public object[] LocalizationArguments { get; set; }

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		HasFocus = true;
		if (CanDisplayTooltip())
		{
			DisplayTooltip(display: true);
		}
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		HasFocus = false;
		DisplayTooltip(display: false);
	}

	public virtual void OnDisable()
	{
		if (HasFocus && TPSingleton<UIManager>.Exist())
		{
			HasFocus = false;
			DisplayTooltip(display: false);
		}
	}

	public void SetTargetTooltip(GenericTooltip genericTooltip)
	{
		targetTooltip = genericTooltip;
	}

	public void DisplayTooltip()
	{
		DisplayTooltip(display: true);
	}

	public void HideTooltip()
	{
		DisplayTooltip(display: false);
	}

	public virtual bool CanDisplayTooltip()
	{
		if (!string.IsNullOrEmpty(LocaKey))
		{
			return base.enabled;
		}
		return false;
	}

	protected virtual void DisplayTooltip(bool display)
	{
		GenericTooltip genericTooltip = targetTooltip ?? UIManager.GenericTooltip;
		if (display)
		{
			SetTooltipContent(genericTooltip);
			genericTooltip.Display();
		}
		else
		{
			genericTooltip.Hide();
		}
	}

	public FollowElement GetTooltipFollowElement()
	{
		return (targetTooltip ?? UIManager.GenericTooltip).FollowElement;
	}

	protected virtual void SetTooltipContent(GenericTooltip tooltip)
	{
		if (rewiredActionKey != null)
		{
			if (LocalizationArguments != null)
			{
				tooltip.SetContentWithHotkeys(Localizer.Format(LocaKey, LocalizationArguments), rewiredActionKey);
			}
			else
			{
				tooltip.SetContentWithHotkeys(Localizer.Get(LocaKey), rewiredActionKey);
			}
		}
		else
		{
			tooltip.SetContent(LocaKey, LocalizationArguments);
		}
	}
}
