using TMPro;
using TheLastStand.Definition.Building;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.LevelEditor;

public class BuildingSettingsHealth : MonoBehaviour
{
	[SerializeField]
	private Slider slider;

	[SerializeField]
	private TextMeshProUGUI healthText;

	private BuildingDefinition buildingDefinition;

	public int CurrentHealth => Mathf.RoundToInt(buildingDefinition.DamageableModuleDefinition.NativeHealthTotal * slider.value);

	public bool IsFullHealth => slider.value == 1f;

	public void Init(BuildingDefinition buildingDefinition)
	{
		this.buildingDefinition = buildingDefinition;
		OnSliderValueChanged(1f);
	}

	public void SetHealth(int value)
	{
		float nativeHealthTotal = buildingDefinition.DamageableModuleDefinition.NativeHealthTotal;
		slider.value = (float)value / nativeHealthTotal;
		healthText.text = $"{value}/{nativeHealthTotal}";
	}

	private void Awake()
	{
		slider.onValueChanged.AddListener(OnSliderValueChanged);
	}

	private void OnSliderValueChanged(float value)
	{
		float nativeHealthTotal = buildingDefinition.DamageableModuleDefinition.NativeHealthTotal;
		healthText.text = $"{Mathf.RoundToInt(nativeHealthTotal * value)}/{nativeHealthTotal}";
	}
}
