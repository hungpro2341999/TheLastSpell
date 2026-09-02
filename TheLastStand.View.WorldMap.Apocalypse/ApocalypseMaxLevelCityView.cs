using TMPro;
using TPLib;
using TheLastStand.Definition.WorldMap;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.WorldMap;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.Apocalypse;

public class ApocalypseMaxLevelCityView : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	[SerializeField]
	private ApocalypseLevelView apocalypseLevelView;

	[SerializeField]
	private TextMeshProUGUI cityName;

	[SerializeField]
	private ApocalypseHeaderSealDisplay sealDisplay;

	[SerializeField]
	private Selectable selectable;

	private CityDefinition cityDefinition;

	private int highestApocalypseLevelReached;

	private bool isCompleted;

	public Selectable Selectable => selectable;

	public void ContinueAnimations()
	{
		apocalypseLevelView.StopAnimation(stopAnimations: false);
		if (sealDisplay != null)
		{
			sealDisplay.ContinueAnimations();
		}
	}

	public void OnCityViewButtonClick()
	{
		WorldMapCity worldMapCity = TPSingleton<WorldMapCityManager>.Instance.Cities.Find((WorldMapCity aCity) => aCity.CityDefinition.Id == cityDefinition.Id);
		if (worldMapCity != null)
		{
			TPSingleton<WorldMapApocalypseMaxLevelView>.Instance.OnExitJoystickFocus(triggerChangeState: false);
			TPSingleton<WorldMapCityManager>.Instance.SelectCity(worldMapCity);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		cityName.gameObject.SetActive(value: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		cityName.gameObject.SetActive(value: false);
	}

	public void OnSelect(BaseEventData eventData)
	{
		OnPointerEnter(null);
		if (TPSingleton<WorldMapApocalypseMaxLevelView>.Instance != null)
		{
			TPSingleton<WorldMapApocalypseMaxLevelView>.Instance.AdjustScrollView(base.transform as RectTransform);
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		OnPointerExit(null);
	}

	public void PauseAnimations()
	{
		apocalypseLevelView.StopAnimation(stopAnimations: true);
		if (sealDisplay != null)
		{
			sealDisplay.PauseAnimations();
		}
	}

	public void Init(CityDefinition worldMapCityDefinition, int highestApocalypseLevel, bool isCityCompleted)
	{
		cityDefinition = worldMapCityDefinition;
		highestApocalypseLevelReached = highestApocalypseLevel;
		isCompleted = isCityCompleted;
		apocalypseLevelView.Init(highestApocalypseLevel);
		sealDisplay.Init(cityDefinition.Id, isCompleted, highestApocalypseLevelReached >= 1);
		Refresh();
	}

	public void Refresh()
	{
		if (cityDefinition != null)
		{
			cityName.text = FormatCityName(cityDefinition.Name, highestApocalypseLevelReached > 0);
		}
	}

	private string FormatCityName(string localizedCityName, bool formatForApo)
	{
		if (formatForApo)
		{
			return "<style=Bad>" + localizedCityName + "</style>";
		}
		return localizedCityName;
	}
}
