using TMPro;
using TheLastStand.Framework.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace TheLastStand.View.Menus;

public class MainMenuChoiceToggle : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler, ISubmitHandler
{
	[SerializeField]
	private TextMeshProUGUI choiceText;

	[SerializeField]
	private BetterToggle choiceToggle;

	[SerializeField]
	private UnityEvent onSubmit;

	private void Start()
	{
		choiceToggle.onValueChanged.AddListener(ChangeFontSize);
	}

	private void ChangeFontSize(bool isOn)
	{
		choiceText.rectTransform.localScale = Vector3.one * (isOn ? 1.2f : 1f);
	}

	public void Submit()
	{
		onSubmit?.Invoke();
	}

	public void OnSelect(BaseEventData eventData)
	{
		choiceToggle.isOn = true;
	}

	public void OnDeselect(BaseEventData eventData)
	{
		choiceToggle.isOn = false;
	}

	public void OnSubmit(BaseEventData eventData)
	{
		Submit();
	}
}
