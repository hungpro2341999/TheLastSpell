using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TheLastStand.Manager;
using TheLastStand.Model;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.ToDoList;

public class ToDoListNotificationView : MonoBehaviour
{
	private const string TriggerName = "appear";

	[SerializeField]
	private Game.E_DayTurn onlyShowDuringDayTurn;

	[SerializeField]
	private bool hideIfEmpty = true;

	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	[SerializeField]
	protected List<TextMeshProUGUI> toDoTexts = new List<TextMeshProUGUI>();

	[SerializeField]
	protected RectTransform notificationRectTransform;

	[SerializeField]
	protected RectTransform notificationGroupRectTransform;

	[SerializeField]
	private VerticalLayoutGroup notificationLayoutGroup;

	[SerializeField]
	private Animator notificationAnimator;

	[SerializeField]
	protected TextMeshProUGUI toDoTextPrefab;

	protected Game.E_DayTurn OnlyShowDuringDayTurn => onlyShowDuringDayTurn;

	public void Display(bool show)
	{
		if (show)
		{
			simpleFontLocalizedParent.RegisterChildren();
		}
		else
		{
			simpleFontLocalizedParent.UnregisterChildren();
		}
		if (!(base.gameObject.activeInHierarchy && show))
		{
			base.gameObject.SetActive(show);
			notificationAnimator.SetTrigger("appear");
		}
	}

	public virtual void Refresh()
	{
		if (onlyShowDuringDayTurn != Game.E_DayTurn.Undefined)
		{
			Display(onlyShowDuringDayTurn == TPSingleton<GameManager>.Instance.Game.DayTurn);
		}
	}

	protected virtual void CheckDisplay()
	{
		if (hideIfEmpty)
		{
			Display(toDoTexts.Count > 0);
		}
		if (toDoTexts.Count > 0)
		{
			notificationGroupRectTransform.sizeDelta = new Vector2(notificationRectTransform.sizeDelta.x, (float)(notificationLayoutGroup.padding.top + notificationLayoutGroup.padding.bottom) + (float)toDoTexts.Count * toDoTextPrefab.rectTransform.sizeDelta.y + (float)Mathf.Max(0, toDoTexts.Count - 1) * notificationLayoutGroup.spacing);
			notificationRectTransform.sizeDelta = notificationGroupRectTransform.sizeDelta;
		}
	}

	private void Start()
	{
		if (simpleFontLocalizedParent != null)
		{
			simpleFontLocalizedParent.RegisterChildren();
		}
	}
}
