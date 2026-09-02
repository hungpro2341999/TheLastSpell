using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.Dev;

public class MetaNarrationHelpBox : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private GameObject helpPanel;

	public void OnPointerEnter(PointerEventData eventData)
	{
		helpPanel.SetActive(value: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		helpPanel.SetActive(value: false);
	}
}
