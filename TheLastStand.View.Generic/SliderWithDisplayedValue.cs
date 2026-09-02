using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TheLastStand.View.Generic;

public class SliderWithDisplayedValue : MonoBehaviour
{
	[SerializeField]
	private string key = "<sprite name=DamnedSouls>XXX";

	[SerializeField]
	[FormerlySerializedAs("ValueDisplayer")]
	private TextMeshProUGUI valueDisplayer;

	[SerializeField]
	private Slider slider;

	[SerializeField]
	private RectTransform boxRect;

	private void Awake()
	{
		slider.onValueChanged?.AddListener(delegate(float x)
		{
			OnValueChanged(x);
		});
	}

	public void OnValueChanged(float value)
	{
		valueDisplayer.text = key.Replace("XXX", (slider.maxValue - value).ToString());
		boxRect.sizeDelta = new Vector2(valueDisplayer.preferredWidth, boxRect.sizeDelta.y);
	}

	[ContextMenu("ChangeValue")]
	private void Test()
	{
		slider.value++;
	}
}
