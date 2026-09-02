using System;
using System.Collections.Generic;
using TheLastStand.Framework;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View;

public class GaugeMarkersDisplayer
{
	private struct GaugeMarkersData
	{
		public int Threshold;

		public int MinIndex;

		public int MaxIndex;
	}

	private readonly RectTransform markersContainer;

	private int lastEnabledMarkerIndex;

	private float lastNormalizedValue;

	private readonly List<Tuple<Image, float>> markers = new List<Tuple<Image, float>>();

	public GaugeMarkersDisplayer(RectTransform markersContainer)
	{
		this.markersContainer = markersContainer;
	}

	public void RefreshEnabledMarkers(float normalizedValue)
	{
		if (lastNormalizedValue < normalizedValue)
		{
			while (lastEnabledMarkerIndex + 1 < markers.Count && markers[lastEnabledMarkerIndex + 1].Item2 < normalizedValue)
			{
				lastEnabledMarkerIndex++;
				markers[lastEnabledMarkerIndex].Item1.enabled = true;
			}
		}
		else
		{
			while (lastEnabledMarkerIndex >= 0 && markers[lastEnabledMarkerIndex].Item2 >= normalizedValue)
			{
				markers[lastEnabledMarkerIndex--].Item1.enabled = false;
			}
		}
		lastNormalizedValue = normalizedValue;
	}

	public void RefreshMarkers(float gaugeMaxValue, float gaugeValue, int markerWidth = 1)
	{
		DeactivatesMarkers();
		GaugeMarkersData gaugeMarkersData = FetchGaugeMarkersData(gaugeMaxValue, markerWidth);
		InstantiateGaugeMarkers(gaugeMarkersData, gaugeMaxValue, gaugeValue, markerWidth);
		lastNormalizedValue = gaugeValue / gaugeMaxValue;
	}

	private void DeactivatesMarkers()
	{
		for (int num = markers.Count - 1; num >= 0; num--)
		{
			markers[num].Item1.gameObject.SetActive(value: false);
		}
		markers.Clear();
		lastEnabledMarkerIndex = -1;
	}

	private GaugeMarkersData FetchGaugeMarkersData(float gaugeMaxValue, int markerWidth)
	{
		GaugeMarkersData result = new GaugeMarkersData
		{
			MinIndex = 0,
			MaxIndex = -1
		};
		int num = 0;
		while (num < UIManager.GaugeMarkersData.Count && IsThresholdTooSmall(UIManager.GaugeMarkersData[num], gaugeMaxValue, markerWidth))
		{
			num++;
			result.MinIndex++;
			result.MaxIndex++;
		}
		while (num < UIManager.GaugeMarkersData.Count && (float)UIManager.GaugeMarkersData[num].Threshold < gaugeMaxValue)
		{
			num++;
			result.MaxIndex++;
		}
		if (result.MinIndex < UIManager.GaugeMarkersData.Count)
		{
			result.Threshold = UIManager.GaugeMarkersData[result.MinIndex].Threshold;
		}
		return result;
	}

	private void InstantiateGaugeMarkers(GaugeMarkersData gaugeMarkersData, float gaugeMaxValue, float gaugeValue, int markerWidth)
	{
		if (gaugeMarkersData.MaxIndex < gaugeMarkersData.MinIndex)
		{
			return;
		}
		float num = gaugeMaxValue / (float)gaugeMarkersData.Threshold;
		int num2 = (int)num;
		if (num2 <= 0)
		{
			return;
		}
		float width = markersContainer.rect.width;
		float num3 = width / num;
		float num4 = gaugeValue / gaugeMaxValue;
		for (int i = 0; i < num2; i++)
		{
			float num5 = (float)(i + 1) * num3 / width;
			for (int num6 = gaugeMarkersData.MaxIndex; num6 >= gaugeMarkersData.MinIndex; num6--)
			{
				if ((i + 1) % (UIManager.GaugeMarkersData[num6].Threshold / UIManager.GaugeMarkersData[gaugeMarkersData.MinIndex].Threshold) == 0)
				{
					InstantiateGaugeMarker(UIManager.GaugeMarkersData[num6], width, markerWidth, num5, num5 < num4);
					break;
				}
			}
		}
	}

	private void InstantiateGaugeMarker(GaugeMarkerData gaugeMarkerData, float containerWidth, int markerWidth, float markerNormalizedPosition, bool enabled)
	{
		Transform transform = ObjectPooler.GetPooledGameObject("GaugeMarker", UIManager.GaugeMarker, markersContainer, dontSetParent: true).transform;
		if (enabled)
		{
			lastEnabledMarkerIndex++;
		}
		Image component = transform.GetComponent<Image>();
		component.enabled = enabled;
		component.color = gaugeMarkerData.Color;
		transform.SetParent(markersContainer, worldPositionStays: false);
		Transform transform2 = transform.transform;
		Vector3 localPosition = transform2.localPosition;
		transform.localPosition = new Vector3(markerNormalizedPosition * containerWidth - (float)markerWidth / 2f, localPosition.y, localPosition.z);
		((RectTransform)transform2).sizeDelta = new Vector2(markerWidth, markersContainer.rect.height);
		markers.Add(new Tuple<Image, float>(component, markerNormalizedPosition));
	}

	private bool IsThresholdTooSmall(GaugeMarkerData gaugeMarkerData, float gaugeMaxValue, int markerWidth)
	{
		float num = gaugeMaxValue / (float)gaugeMarkerData.Threshold;
		float num2 = markersContainer.rect.width / num;
		if (gaugeMarkerData.MaxValueToUse <= 0 || !((float)gaugeMarkerData.MaxValueToUse < gaugeMaxValue))
		{
			return num2 < (float)markerWidth * UIManager.GaugeMarkerSectionSizeRatio;
		}
		return true;
	}
}
